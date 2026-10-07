# Generating 2D assets with Abacus.AI RouteLLM

RouteLLM is Abacus.AI's OpenAI-compatible model router. Image generation
goes through the standard `/chat/completions` endpoint with
`modalities: ["image"]` — there is no separate `/images` endpoint.

This repo wraps it in `art/tools/gen_image.py` (stdlib-only, runs under
Blender's bundled Python and stock Python — no pip installs).

## Auth

- API key from https://abacus.ai/app/route-llm-apis
- Stored as `ROUTELLM_API_KEY=<key>` in the repo-root `.env` (gitignored —
  never commit) or exported as an environment variable
- Sent as `Authorization: Bearer <key>`

## Request shape

```
POST https://routellm.abacus.ai/v1/chat/completions

{
  "model": "seedream",
  "modalities": ["image"],
  "n": 1,
  "image_config": {"aspect_ratio": "1:1"},   // optional
  "messages": [{"role": "user", "content": "<prompt>"}]
}
```

Images come back in `choices[0].message.images[]` — each entry is an
`image_url` whose `url` is either an http URL or a `data:` URI.
`gen_image.py` saves them to disk, sniffing the magic bytes to correct
the extension (`.png` / `.jpg` / `.webp`).

## Models

| Model | Notes |
|---|---|
| `seedream` | Default — good general painterly output, fast |
| `nano_banana_pro`, `gpt_image25` | Higher-quality "hero shot" passes |
| `recraft` | Preferred when transparent-background output matters |
| `flux2_pro`, `ideogram45` | Available alternates |
| `*_edit` variants | Iteration — modify a previous image rather than re-roll |

## Usage

```bash
# CLI — one image
python3 art/tools/gen_image.py \
    -p "hand-painted cozy fantasy bakery storefront, warm golden palette" \
    -o art/bakery_paint.png -m seedream

# CLI — 4 variants + aspect ratio
python3 art/tools/gen_image.py \
    -p "16 village props, 4x4 sprite sheet, flat game art" \
    -o art/props_sheet.png -n 4 \
    --image-config '{"aspect_ratio": "1:1"}'
```

```python
# Library
from gen_image import generate_image
generate_image("oak tree foliage texture, painterly", "art/oak_foliage_paint.png")
```

Output paths print to stdout; `n > 1` appends `_2`, `_3`, ... before the
extension.

## Prompting conventions (what worked here)

- **Style anchor:** reuse one fixed fragment verbatim across all calls
  (e.g. "hand-painted cozy fantasy village, warm golden palette, soft
  watercolor brush strokes, flat game art, clean silhouettes, no text")
  so assets share a look.
- **Asset sheets:** ask for a grid ("16 village props, 4x4 sheet,
  consistent scale") and slice it — one call buys style coherence across
  a whole set.
- **Alpha:** `recraft` (or `gpt_image25`) for transparent backgrounds;
  otherwise generate on a flat background and chroma-key in the pipeline.
- **Iterate with edit models:** use `*_edit` variants to nudge a result
  instead of re-rolling the full prompt.
- **Cost:** generation is cheap — plan on regenerating anything that
  reads wrong in-scene rather than fighting prompts for precision.
