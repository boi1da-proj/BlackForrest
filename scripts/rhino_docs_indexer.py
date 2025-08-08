#!/usr/bin/env python3
"""
Rhino Developer Docs Indexer
- Crawls developer.rhino3d.com (scoped and rate-limited)
- Extracts title, headings (h1-h3), code blocks, and section anchors
- Writes a compact JSON index to data/rhino_docs_index.json

Usage:
  python3 scripts/rhino_docs_indexer.py \
    --start https://developer.rhino3d.com/guides \
    --max-pages 300 \
    --out data/rhino_docs_index.json

Requires: requests, beautifulsoup4, lxml (recommended)
  pip install -r requirements-docs.txt
"""
from __future__ import annotations
import argparse
import json
import os
import re
import sys
import time
from urllib.parse import urljoin, urlparse, urldefrag

try:
    import requests
    from bs4 import BeautifulSoup  # type: ignore
except Exception as e:
    print("Missing dependencies. Please run: pip install -r requirements-docs.txt", file=sys.stderr)
    raise

ALLOWED_HOST = "developer.rhino3d.com"
DEFAULT_STARTS = [
    "https://developer.rhino3d.com/",
    "https://developer.rhino3d.com/guides/",
    "https://developer.rhino3d.com/api/",
]

USER_AGENT = "SoftlyPlease-Indexer/1.0 (+https://github.com/boi1da-proj/BlackForrest)"
HEADERS = {"User-Agent": USER_AGENT}

def is_allowed(url: str) -> bool:
    parsed = urlparse(url)
    if parsed.scheme not in ("http", "https"):
        return False
    if parsed.netloc != ALLOWED_HOST:
        return False
    # Limit to relevant sections
    if not (parsed.path.startswith("/") and not parsed.path.startswith("/admin")):
        return False
    return True

def normalize_url(base: str, href: str) -> str | None:
    if not href:
        return None
    abs_url = urljoin(base, href)
    abs_url, _frag = urldefrag(abs_url)
    if is_allowed(abs_url):
        return abs_url
    return None

def extract_doc(url: str, html: str) -> dict:
    soup = BeautifulSoup(html, "lxml")
    title = (soup.title.string.strip() if soup.title and soup.title.string else "").strip()
    # Headings and code blocks
    headings: list[str] = []
    for tag in soup.find_all(["h1", "h2", "h3"]):
        txt = tag.get_text(" ", strip=True)
        if txt:
            headings.append(txt)
    code_blocks: list[str] = []
    for pre in soup.find_all("pre"):
        # capture compacted code; limit size
        txt = pre.get_text("\n", strip=True)
        if txt:
            code_blocks.append(txt[:2000])
    # A short text snippet for search
    paragraphs: list[str] = []
    for p in soup.find_all("p"):
        txt = p.get_text(" ", strip=True)
        if txt:
            paragraphs.append(txt)
        if len(paragraphs) >= 5:
            break
    snippet = " ".join(paragraphs)[:500]
    return {
        "url": url,
        "title": title,
        "headings": headings,
        "code": code_blocks,
        "snippet": snippet,
    }

def crawl(starts: list[str], max_pages: int, delay: float) -> list[dict]:
    seen: set[str] = set()
    queue: list[str] = []
    for s in starts:
        if is_allowed(s):
            queue.append(s)
    out: list[dict] = []
    while queue and len(out) < max_pages:
        url = queue.pop(0)
        if url in seen:
            continue
        seen.add(url)
        try:
            resp = requests.get(url, headers=HEADERS, timeout=15)
            if resp.status_code != 200 or "text/html" not in resp.headers.get("Content-Type", ""):
                continue
            doc = extract_doc(url, resp.text)
            out.append(doc)
            # Discover links
            soup = BeautifulSoup(resp.text, "lxml")
            for a in soup.find_all("a"):
                href = a.get("href")
                norm = normalize_url(url, href) if href else None
                if norm and norm not in seen and norm not in queue:
                    queue.append(norm)
            time.sleep(delay)
        except Exception as e:
            # be resilient
            continue
    return out

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--start", action="append", help="Start URL(s)")
    ap.add_argument("--max-pages", type=int, default=200, help="Max pages to crawl")
    ap.add_argument("--delay", type=float, default=0.2, help="Polite delay between requests (seconds)")
    ap.add_argument("--out", default="data/rhino_docs_index.json", help="Output JSON index path")
    args = ap.parse_args()

    starts = args.start or DEFAULT_STARTS
    docs = crawl(starts, args.max_pages, args.delay)
    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    with open(args.out, "w") as f:
        json.dump({"source": ALLOWED_HOST, "count": len(docs), "docs": docs}, f, indent=2)
    print(f"Indexed {len(docs)} pages -> {args.out}")

if __name__ == "__main__":
    main()
