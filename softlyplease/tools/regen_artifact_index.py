#!/usr/bin/env python3
import json, hashlib, time
from pathlib import Path


FORBIDDEN_TOKENS = (b"<SECRET>", b"CHANGEME", b"PLACEHOLDER")


def sha256_file(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
            if any(tok in chunk for tok in FORBIDDEN_TOKENS):
                raise SystemExit(f"ERROR: Forbidden token in {p}")
            h.update(chunk)
    return h.hexdigest()


def main():
    root = Path(__file__).resolve().parents[1]
    module = root / "modules" / "compute_aabb" / "module.py"
    idx = {
        "version": "1.0.0",
        "generated_at": str(int(time.time())),
        "artifacts": [],
        "environments": [{"label": "dev"}, {"label": "ci"}, {"label": "prod"}],
        "policies": {"sandbox": "process", "write_root": ".", "network": "deny"},
    }
    if module.is_file():
        idx["artifacts"].append({
            "path": module.relative_to(root).as_posix(),
            "sha256": sha256_file(module),
            "size": module.stat().st_size,
            "type": "shadow_module",
            "module_id": "compute_aabb",
        })
    json.dump(idx, open(root / "artifact_index.json", "w", encoding="utf-8"), indent=2, sort_keys=True)
    print("Wrote artifact_index.json")


if __name__ == "__main__":
    main()

