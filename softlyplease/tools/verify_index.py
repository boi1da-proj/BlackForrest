#!/usr/bin/env python3
import json, sys
from pathlib import Path

def main(idx_path: str, schema_path: str) -> int:
    try:
        from jsonschema import validate
    except Exception:
        print("Install jsonschema: pip install jsonschema", file=sys.stderr)
        return 2
    idx = json.load(open(idx_path, 'r', encoding='utf-8'))
    schema = json.load(open(schema_path, 'r', encoding='utf-8'))
    validate(instance=idx, schema=schema)
    print("artifact_index.json schema validation OK")
    return 0

if __name__ == '__main__':
    sys.exit(main(sys.argv[1] if len(sys.argv)>1 else 'artifact_index.json', sys.argv[2] if len(sys.argv)>2 else 'schemas/artifact_index.schema.json'))

