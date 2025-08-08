#!/usr/bin/env python3
import json, sys, os

SCHEMA_VERSION = "1.0"

REQUIRED_TOP_KEYS = ["entries"]

REQUIRED_ENTRY_KEYS = [
    # minimal set
    "EnvironmentLabel",
    "Module",
    "ModuleVersion",
    "Status",
]

def validate_index(path):
    with open(path, 'r') as f:
        data = json.load(f)
    for k in REQUIRED_TOP_KEYS:
        if k not in data:
            raise SystemExit(f"Missing top-level key: {k}")
    if not isinstance(data["entries"], list):
        raise SystemExit("entries must be a list")
    for i, e in enumerate(data["entries"]):
        for k in REQUIRED_ENTRY_KEYS:
            if k not in e and k not in e and k.lower() not in e:
                # allow case variations from different writers
                raise SystemExit(f"Entry {i} missing key: {k}")
    print("artifact_index.json schema OK")

if __name__ == "__main__":
    idx = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.getcwd(), 'artifact_index.json')
    if not os.path.exists(idx):
        print(f"No artifact_index.json at {idx}; skipping.")
        sys.exit(0)
    validate_index(idx)
