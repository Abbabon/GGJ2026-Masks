#!/usr/bin/env python3
import http.server
import os
import sys

BUILD_DIR = sys.argv[1] if len(sys.argv) > 1 else "Builds/WebGL"
PORT = int(sys.argv[2]) if len(sys.argv) > 2 else 8080

if not os.path.isfile(os.path.join(BUILD_DIR, "index.html")):
    print(f"No index.html found in '{BUILD_DIR}'")
    print("Usage: python3 serve-webgl.py [build-folder] [port]")
    sys.exit(1)

CONTENT_TYPES = {
    ".js": "application/javascript",
    ".wasm": "application/wasm",
    ".data": "application/octet-stream",
    ".json": "application/json",
    ".html": "text/html",
    ".css": "text/css",
    ".png": "image/png",
    ".jpg": "image/jpeg",
    ".ico": "image/x-icon",
}

class WebGLHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=BUILD_DIR, **kwargs)

    def end_headers(self):
        path = self.translate_path(self.path)
        if path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        if path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        super().end_headers()

    def guess_type(self, path):
        # For .br and .gz files, return the type of the underlying file
        if path.endswith(".br") or path.endswith(".gz"):
            path = path.rsplit(".", 1)[0]
        ext = os.path.splitext(path)[1].lower()
        return CONTENT_TYPES.get(ext, "application/octet-stream")

print(f"Serving WebGL build from '{BUILD_DIR}' at http://localhost:{PORT}")
http.server.HTTPServer(("", PORT), WebGLHandler).serve_forever()
