#!/usr/bin/env python3
import json
import hashlib
import os
from datetime import datetime

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
INDEX_PATH = os.path.join(ROOT, 'artifact_index.json')

def sha256_of_file(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(8192), b''):
            h.update(chunk)
    return h.hexdigest()

def main():
    index = {"generated_at": datetime.utcnow().isoformat() + "Z", "entries": []}
    if os.path.exists(INDEX_PATH):
        with open(INDEX_PATH, 'r') as f:
            try:
                index = json.load(f)
            except Exception:
                index = {"generated_at": datetime.utcnow().isoformat() + "Z", "entries": []}

    assets_dir = os.path.join(ROOT, 'outputs')
    if not os.path.isdir(assets_dir):
        os.makedirs(assets_dir, exist_ok=True)

    existing_ids = {e['AssetId']: e for e in index.get('entries', [])}
    for file in os.listdir(assets_dir):
        if not file.lower().endswith(('.stl', '.mesh', '.obj', '.json')):
            continue
        path = os.path.join(assets_dir, file)
        asset_id = os.path.basename(file)
        sha = sha256_of_file(path)
        if asset_id in existing_ids:
            existing_ids[asset_id]['Checksum'] = sha
            existing_ids[asset_id]['Timestamp'] = datetime.utcnow().isoformat() + "Z"
        else:
            index_entry = {
                "AssetId": asset_id,
                "Name": file,
                "Type": "mesh",
                "Path": path,
                "Version": "1.0.0",
                "Checksum": sha,
                "Dependencies": [],
                "ShadowDeploymentMetadata": "generated",
                "EnvironmentLabel": "dev",
                "Timestamp": datetime.utcnow().isoformat() + "Z"
            }
            index['entries'].append(index_entry)

    with open(INDEX_PATH, 'w') as f:
        json.dump(index, f, indent=2)
    print("artifact_index.json regenerated at", INDEX_PATH)

if __name__ == "__main__":
    main()
