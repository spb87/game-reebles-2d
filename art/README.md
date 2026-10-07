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

## Open question: alpha/transparency

`recraft` (or `gpt_image25`) reportedly emits usable transparent backgrounds;
the fallback is a flat solid background + chroma-key. Settled by the 3-call
experiment in the first art-pass task.
