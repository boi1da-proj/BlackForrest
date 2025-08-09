def run(inputs: dict) -> dict:
    pts = inputs.get("points", [])
    if not pts:
        return {"bbox": None}
    xs = [p[0] for p in pts]; ys = [p[1] for p in pts]; zs = [p[2] for p in pts]
    return {"bbox": {"min": [min(xs), min(ys), min(zs)], "max": [max(xs), max(ys), max(zs)]}}

