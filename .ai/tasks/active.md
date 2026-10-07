# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-142: Fix art tiling + off-model sprites (user-reported)

- **Feature**: none — Bug directly under epic REEB-123 (DEC-10 allows Bug as epic child)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs`, `art/prompts/*.md`, `game/Assets/Art/Backdrops/*`, `game/Assets/Art/Sprites/*` (+ metas), generated `Village.unity`/`Player.prefab`, `tools/deploy-pages.py` (jq quoting fix), `art/staging/` (untracked)
- **Problem** (see `examples/tiling-error.png`): `ground_grass.jpg` is a painted village SCENE (paths + buildings baked in) that `Tiled`-repeats 30×20 → grid of duplicate mini-villages. `fence.png` is an oblique picket segment squashed to 0.3u `Tiled` strips. `reeble_sheet.png` is photorealistic — recraft ignored the anchor.
- **Fix**:
  1. Regenerate the ground as a **single full-map backdrop**, NOT tiled: `seedream` (accepts `--image-config` — try `{"aspect_ratio":"3:2"}` for the 30×20 map), prompt = painterly empty terrain only: "top-down view, flat empty grass meadow with a few soft dirt paths, no buildings, no objects, no characters, no text" + verbatim style anchor. Save as `Backdrops/village_ground.png` (replaces ground_grass.jpg as the used asset; keep or delete the old file — delete if unused). Prompt record required.
  2. Builder: ground becomes a single stretched `SpriteRenderer` (NO Tiled drawMode) sized to map bounds.
  3. Fence: place whole fence-segment sprites side by side along each edge — compute segment count from the sprite's natural aspect at ~1u tall; rotate/reuse art on vertical edges (acceptable v1). Colliders stay thin strips (unchanged).
  4. Reeble re-roll (1-2 attempts max): recraft with stronger anti-photorealism language — "flat 2D game sprite, hand-painted watercolor style, NOT photorealistic, small round fuzzy fantasy creature" + anchor. If still off-model, keep current sheet cell 0 and note it in the report.
  5. Rebuild scene → PlayMode run (**no `-quit`**) must stay 4/4 green → WebGL rebuild → `python3 tools/deploy-pages.py` → verify live URL 200 + new gh-pages commit.
  6. Also fix the `deploy-pages.py` cosmetic jq single-quote bug flagged in REEB-141's report while you're in there.
- **Exit criteria**: live URL serves the fixed build; no tiled repeats of scene content; PlayMode 4/4; prompt records updated.
- **Max new lines**: ~250
- **Status**: in progress
- **Git**: `fix(REEB-142): correct ground tiling, fence segments, reeble style re-roll`
- **CRITICAL**: `.env`/`ROUTELLM_API_KEY` never printed or committed.
