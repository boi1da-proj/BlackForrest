SoftlyPlease Shadow Scaffold (artifact-index centric)

This folder contains a minimal, auditable Shadow Code scaffold: static artifact index, sandboxed runner, minimal API, and CI hooks.

Key pieces:
- `artifact_index.json` (static)
- `schemas/artifact_index.schema.json`
- `tools/regen_artifact_index.py` (deterministic)
- `runner/shadow_runner.py` (process-isolated adapter)
- `modules/compute_aabb/module.py` (example)
- `api/server.py` (FastAPI wrapper)
- `requirements.txt`

