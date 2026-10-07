"""RouteLLM (Abacus.AI) image generation helper.

Dual interface:
  CLI:  python art/tools/gen_image.py --prompt "..." --out out.png [--model seedream]
  Lib:  from gen_image import generate_image; generate_image("...", "out.png")

Stdlib-only so it runs inside Blender's bundled Python (`blender.exe -b --python`)
and stock Windows python without pip installs.

Auth: reads ROUTELLM_API_KEY from the repo-root .env (or the environment).
API: POST https://routellm.abacus.ai/v1/chat/completions with modalities=["image"];
image(s) come back in choices[0].message.images[] as image_url entries (http URL
or data: URI). Key page: https://abacus.ai/app/route-llm-apis
"""

import argparse
import base64
import json
import os
import re
import sys
import urllib.request

API_URL = "https://routellm.abacus.ai/v1/chat/completions"
DEFAULT_MODEL = "seedream"
TIMEOUT = 180


def _repo_root():
    d = os.path.abspath(os.path.dirname(__file__))
    for _ in range(4):
        if os.path.exists(os.path.join(d, ".env")) or os.path.exists(os.path.join(d, ".git")):
            return d
        d = os.path.dirname(d)
    return d


def _load_key():
    key = os.environ.get("ROUTELLM_API_KEY")
    if key:
        return key
    env_path = os.path.join(_repo_root(), ".env")
    with open(env_path) as f:
        for line in f:
            m = re.match(r"\s*ROUTELLM_API_KEY\s*=\s*(\S+)", line)
            if m:
                return m.group(1)
    raise RuntimeError("ROUTELLM_API_KEY not found in environment or %s" % env_path)


_MAGIC = {b"\x89PNG": ".png", b"\xff\xd8\xff": ".jpg", b"RIFF": ".webp"}


def _save_image(url, out_path):
    os.makedirs(os.path.dirname(os.path.abspath(out_path)), exist_ok=True)
    if url.startswith("data:"):
        b64 = url.split(",", 1)[1]
        data = base64.b64decode(b64)
    else:
        with urllib.request.urlopen(url, timeout=TIMEOUT) as r:
            data = r.read()
    for magic, ext in _MAGIC.items():
        if data.startswith(magic):
            base, cur = os.path.splitext(out_path)
            if cur.lower() != ext:
                out_path = base + ext
            break
    with open(out_path, "wb") as f:
        f.write(data)
    return out_path


def generate_image(prompt, out_path, model=DEFAULT_MODEL, image_config=None,
                   n=1, key=None):
    """Generate image(s) via RouteLLM. Returns list of saved file paths.

    prompt:       text prompt
    out_path:     output PNG path; if n>1, "_2", "_3"... inserted before ext
    model:        image model id, e.g. seedream, nano_banana_pro, flux2_pro,
                  gpt_image25, recraft, ideogram45
    image_config: optional dict passed verbatim, e.g. {"aspect_ratio": "1:1"}
    """
    key = key or _load_key()
    body = {
        "model": model,
        "modalities": ["image"],
        "n": n,
        "messages": [{"role": "user", "content": prompt}],
    }
    if image_config:
        body["image_config"] = image_config

    req = urllib.request.Request(
        API_URL,
        data=json.dumps(body).encode(),
        headers={
            "Authorization": "Bearer " + key,
            "Content-Type": "application/json",
        },
    )
    with urllib.request.urlopen(req, timeout=TIMEOUT) as r:
        resp = json.loads(r.read())

    images = resp["choices"][0]["message"].get("images") or []
    if not images:
        raise RuntimeError("no images in response: " + json.dumps(resp)[:500])

    saved = []
    for i, img in enumerate(images):
        url = img.get("image_url", {}).get("url") or img.get("url", "")
        if not url:
            raise RuntimeError("unrecognized image entry: " + json.dumps(img)[:300])
        path = out_path
        if i > 0:
            base, ext = os.path.splitext(out_path)
            path = "%s_%d%s" % (base, i + 1, ext or ".png")
        saved.append(_save_image(url, path))
    return saved


def main(argv=None):
    p = argparse.ArgumentParser(description="Generate an image via RouteLLM")
    p.add_argument("--prompt", "-p", required=True)
    p.add_argument("--out", "-o", required=True)
    p.add_argument("--model", "-m", default=DEFAULT_MODEL)
    p.add_argument("--n", type=int, default=1)
    p.add_argument("--image-config", type=json.loads, default=None,
                   help='raw JSON, e.g. \'{"aspect_ratio":"1:1"}\'')
    a = p.parse_args(argv)
    for path in generate_image(a.prompt, a.out, model=a.model,
                               image_config=a.image_config, n=a.n):
        print(path)


if __name__ == "__main__":
    main()
