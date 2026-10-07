# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-140: Art pass 1A — alpha experiment + first asset batch

- **Feature**: REEB-139 (`first-art-pass.md`)
- **Role**: Developer
- **Files allowed**: `art/prompts/*.md`, `game/Assets/Art/Sprites/*` + `.meta`, `game/Assets/Art/Backdrops/*` + `.meta`, `art/README.md` (alpha verdict), `art/staging/` outputs (gitignored — never committed)
- **Input**: (1) Run the deferred alpha experiment — 3 calls for a transparent-background sprite (e.g. a lantern): (a) `recraft` with explicit 'transparent background, alpha channel', (b) `seedream` on flat solid background (chroma-key candidate), (c) `gpt_image25` transparent attempt. Check PNG IHDR color type (6=RGBA) + whether corner pixels are actually transparent — write a tiny stdlib PNG check (zlib+struct, no PIL). Pick winner, document verdict in `art/README.md`. (2) Generate the batch with the winning approach — EVERY prompt ends with the verbatim style anchor from `art/style-anchor.txt`: tileable ground texture (no alpha needed); 5 buildings (bakery, smithy, herbalist, general store, inn — 3/4 oblique front view, transparent bg); fountain; horizontal fence segment (tileable); player 'Reeble' (small round cute creature — try a 4-dir × 2-frame sheet in one call, fall back to single front-facing sprite); optional small props sheet. `--image-config '{"aspect_ratio":"1:1"}'` where sensible. (3) Promote winners to `game/Assets/Art/Sprites/` (+`Backdrops/` for ground); one prompt record per promoted asset in `art/prompts/<asset>.md` (model + full prompt incl. anchor).
- **Exit criteria**: every promoted PNG needing transparency has a real alpha channel (verified by the check script — report it); ≥1 prompt record per promoted asset; `art/README.md` alpha section updated with verdict; no staging files committed.
- **Max new lines**: ~200 (+ binary art)
- **Dependencies**: none
- **Status**: in progress
- **Git**: `feat(REEB-140): generate first art batch via RouteLLM + settle alpha approach`
- **CRITICAL**: `.env` contains the real `ROUTELLM_API_KEY` — NEVER print, log, commit, or paste it. `gen_image.py` reads it itself. Committing a tracked file containing the key is a failure of the whole task.
