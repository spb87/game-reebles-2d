# Art pipeline

All game art is generated via the RouteLLM image API using `art/tools/gen_image.py`
(stdlib-only Python; reads `ROUTELLM_API_KEY` from the repo-root `.env`).

## Pipeline

1. Compose a per-asset prompt that **includes the style anchor** verbatim
   (see `art/style-anchor.txt`) so all assets share one visual style.
2. Generate into staging:

   ```bash
   python3 art/tools/gen_image.py --prompt "<style anchor>, <asset description>" \
       --out art/staging/<asset>.png [--model seedream]
   ```

   `art/staging/` is gitignored — staging files are regenerable intermediates.
3. Review the staged image. Iterate by re-running with a revised prompt
   (or an `*_edit` model variant) until satisfied.
4. Promote the final image into the build at `game/Assets/Art/{Sprites,Backdrops,Sheets}/`.
5. Record the prompt in `art/prompts/<asset>.md` (committed): model used plus the
   full prompt including the style anchor, so any asset can be regenerated.

## Model menu

- `seedream` — default for general assets (cheap, fast)
- `nano_banana_pro` / `gpt_image25` — hero shots and showcase pieces
- `recraft` — **preferred when transparent background matters** (per `gen-2d-ai.md`)
- `*_edit` variants — iteration on an existing image
- Other ids available: `flux2_pro`, `ideogram45`
- `--image-config '{"aspect_ratio":"1:1"}'` passes raw config through to the API

## Alpha/transparency — SETTLED (REEB-140)

**Winner: `recraft`** — emits real RGBA PNGs with transparent corners
(verified: IHDR color type 6, corner alpha 0-2).

Experiment results (lantern sprite, identical prompt):

| Model | Result |
|-------|--------|
| `recraft` | 1024x1024 PNG, color type 6 (RGBA), corner alpha ≈ 0 — transparent |
| `gpt_image25` | 1024x1024 PNG, color type 2 (RGB), corner alpha 255 — opaque |
| `seedream` | returned a JPEG — no alpha possible |

Caveats discovered:

- `image_config` (e.g. `--image-config '{"aspect_ratio":"1:1"}'`) causes
  HTTP 400 on `recraft` and `gpt_image25`. Omit it for those models;
  default output is 1024x1024 square anyway. `seedream` accepts it.
- `recraft` is nondeterministic about format: most calls return RGBA PNG,
  but some return lossless WebP (VP8L) *without* alpha, and one RGBA PNG
  had a slightly-lit corner (alpha ~23). Always verify alpha on staged
  files before promoting (see `check_alpha` approach: IHDR color type +
  corner-pixel alpha; VP8L alpha flag is bit 28 of the 5-byte field after
  the `VP8L` chunk header).
- `nano_banana_pro` returned opaque RGB PNG on a transparency prompt.

Rule of thumb: use `recraft` for anything needing transparency; use
`seedream` for opaque textures/backdrops.
