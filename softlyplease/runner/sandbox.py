from pathlib import Path
import os


def ensure_safe_output(project_root: Path, out_path: Path) -> Path:
    out_path = out_path.resolve()
    if project_root not in out_path.parents and out_path != project_root:
        raise RuntimeError("Output path must be inside project root.")
    out_path.parent.mkdir(parents=True, exist_ok=True)
    return out_path


def deny_network_by_default():
    # Placeholder for network egress control (runner-level firewall recommended)
    os.environ.setdefault("SP_NET_POLICY", "deny")

