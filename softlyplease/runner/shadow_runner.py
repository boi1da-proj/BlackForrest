#!/usr/bin/env python3
import importlib.util
import json
import multiprocessing as mp
import os
import time
import uuid
from pathlib import Path
from typing import Any, Dict

from .hashing import sha256_file
from .sandbox import ensure_safe_output, deny_network_by_default


def _worker(module_path: str, inputs: Dict[str, Any], work_dir: str, q: mp.Queue) -> None:
    os.chdir(work_dir)
    spec = importlib.util.spec_from_file_location("shadow_module", module_path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Failed to load module at {module_path}")
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)  # type: ignore[attr-defined]
    if not hasattr(mod, "run"):
        raise RuntimeError("Module missing run(inputs: dict) -> dict")
    q.put(mod.run(inputs))


def run(config_path: Path, payload_in: Path, payload_out: Path, timeout: int = 60) -> int:
    project_root = Path(os.getenv("SP_PROJECT_ROOT", Path.cwd())).resolve()
    index_path = project_root / "artifact_index.json"
    deny_network_by_default()

    with open(payload_in, "r", encoding="utf-8") as f:
        payload: Dict[str, Any] = json.load(f)
    module_rel = payload["module_path"]  # e.g., modules/compute_aabb/module.py
    inputs = payload.get("inputs", {})

    # Verify against artifact index
    idx = json.load(open(index_path, "r", encoding="utf-8"))
    arts = {a["path"]: a for a in idx.get("artifacts", [])}
    if module_rel not in arts:
        raise RuntimeError("Module not indexed/allowed")
    expected = arts[module_rel].get("sha256")
    module_path = (project_root / module_rel).resolve()
    if expected and sha256_file(module_path) != expected:
        raise RuntimeError("Checksum mismatch. Regenerate artifact index.")

    out_path = ensure_safe_output(project_root, payload_out)
    run_id = str(uuid.uuid4())
    started = time.time()
    status = "ok"
    result_obj: Any = None

    with mp.Manager() as manager:
        q: mp.Queue = manager.Queue()
        proc = mp.Process(target=_worker, args=(str(module_path), inputs, str(project_root), q), daemon=True)
        proc.start()
        proc.join(timeout)
        if proc.is_alive():
            proc.terminate(); proc.join(5); status = "timeout"
        elif proc.exitcode != 0:
            status = "error"
        else:
            try:
                result_obj = q.get_nowait()
            except Exception:
                status = "error"

    duration_ms = int((time.time() - started) * 1000)
    envelope = {
        "run_id": run_id,
        "module_path": module_rel,
        "environment_label": os.getenv("SP_ENV", "dev"),
        "summary": {"duration_ms": duration_ms, "status": status},
        "result": result_obj if status == "ok" else None,
    }
    json.dump(envelope, open(out_path, "w", encoding="utf-8"), indent=2)
    return 0 if status == "ok" else 1


if __name__ == "__main__":
    import argparse
    ap = argparse.ArgumentParser()
    ap.add_argument("--config", default="shadow/shadow_config.json")
    ap.add_argument("--in", dest="inp", required=True)
    ap.add_argument("--out", dest="out", required=True)
    ap.add_argument("--timeout", type=int, default=60)
    args = ap.parse_args()
    raise SystemExit(run(Path(args.config), Path(args.inp), Path(args.out), timeout=args.timeout))

