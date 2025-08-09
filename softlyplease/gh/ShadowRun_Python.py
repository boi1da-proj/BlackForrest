# GhPython: Inputs P (List[Rhino.Geometry.Point3d]), Density (float); Output: A
import os, json, sys, subprocess, tempfile
pts = [[p.X,p.Y,p.Z] for p in (P or [])]
payload = {"module_path":"modules/compute_aabb/module.py","inputs":{"points":pts,"density":Density}}
root = os.getcwd()
fd_in, path_in = tempfile.mkstemp(suffix='.json'); os.close(fd_in)
fd_out, path_out = tempfile.mkstemp(suffix='.json'); os.close(fd_out)
json.dump(payload, open(path_in,'w'))
cmd = [sys.executable, os.path.join(root,'runner','shadow_runner.py'), '--config', os.path.join(root,'shadow','shadow_config.json'), '--in', path_in, '--out', path_out]
subprocess.run(cmd, check=False, timeout=60)
A = json.load(open(path_out,'r')) if os.path.exists(path_out) else None

