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

Runbook
- python -m pip install -r requirements.txt
- python tools/regen_artifact_index.py && python tools/verify_index.py artifact_index.json schemas/artifact_index.schema.json
- uvicorn api.server:app --reload
- open viewer/index.html (or proxy /run to the API)

