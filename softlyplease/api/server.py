from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from pathlib import Path
import json
import tempfile
import subprocess
import sys

app = FastAPI()


class ShadowRequest(BaseModel):
    module_path: str
    inputs: dict
    timeout: int = 60


@app.post("/run")
def run_shadow(req: ShadowRequest):
    project_root = Path.cwd()
    runner = project_root / "runner" / "shadow_runner.py"
    if not runner.is_file():
        raise HTTPException(500, "Runner not found")
    with tempfile.NamedTemporaryFile(delete=False, suffix=".json") as tf_in, tempfile.NamedTemporaryFile(delete=False, suffix=".json") as tf_out:
        json.dump({"module_path": req.module_path, "inputs": req.inputs}, open(tf_in.name, "w", encoding="utf-8"))
        cmd = [sys.executable, str(runner), "--config", str(project_root / "shadow" / "shadow_config.json"), "--in", tf_in.name, "--out", tf_out.name, "--timeout", str(req.timeout)]
        proc = subprocess.run(cmd, capture_output=True, text=True)
        if proc.returncode != 0:
            raise HTTPException(500, proc.stderr or "Shadow run failed")
        return json.load(open(tf_out.name, "r", encoding="utf-8"))

