#!/usr/bin/env python3
"""Static file server for Unity WebGL builds with Brotli compression.

Unity emits assets as `*.js.br`, `*.wasm.br`, `*.data.br`, etc. Browsers only
decode them when the response carries `Content-Encoding: br` plus the
Content-Type of the *uncompressed* resource. Python's default
`http.server` does neither, so this subclass adds both.

Usage (from repo root):
    python3 tools/serve-webgl.py [--port 8080] [--dir game/Builds/Web]
"""

import argparse
import http.server
import os

# Unity's Brotli outputs and the MIME type the browser must see for each.
BROTLI_TYPES = {
    ".js.br": "application/javascript",
    ".wasm.br": "application/wasm",
    ".data.br": "application/octet-stream",
    ".symbols.json.br": "application/json",
    ".html.br": "text/html",
    ".css.br": "text/css",
}

# PWA files: the manifest needs the spec MIME for installability; sw.js must
# carry a JavaScript MIME and live at root scope (it does — the handler serves
# it from the build root like any other file).
EXTRA_TYPES = {
    ".webmanifest": "application/manifest+json",
    ".js": "text/javascript",
}


class WebGLRequestHandler(http.server.SimpleHTTPRequestHandler):
    """Serve Brotli files with Content-Encoding: br and the inner MIME type."""

    def _is_brotli_request(self):
        return any(self.path.endswith(s) for s in BROTLI_TYPES)

    def guess_type(self, path):
        # Map a *.br path to the MIME type of its uncompressed resource.
        for suffix, mime in BROTLI_TYPES.items():
            if path.endswith(suffix):
                return mime
        for suffix, mime in EXTRA_TYPES.items():
            if path.endswith(suffix):
                return mime
        return super().guess_type(path)

    def send_response_only(self, code, message=None):
        super().send_response_only(code, message)
        if self._is_brotli_request():
            self.send_header("Content-Encoding", "br")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", type=int, default=8080)
    parser.add_argument("--dir", default="game/Builds/Web")
    args = parser.parse_args()

    os.chdir(args.dir)
    server = http.server.ThreadingHTTPServer(
        ("0.0.0.0", args.port), WebGLRequestHandler)
    print(f"Serving {os.getcwd()} at http://localhost:{args.port}")
    server.serve_forever()


if __name__ == "__main__":
    main()
