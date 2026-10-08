# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-147: W1 — Art batch 2: terrain backdrop + prop set + villager sprite

- **Feature**: REEB-144 (M2: World expansion — outskirts beyond the village)
- **Role**: Developer
- **Files allowed**: `art/prompts/*.md`, `game/Assets/Art/Backdrops/*`, `game/Assets/Art/Sprites/*` (+metas), `art/staging/` (untracked)
- **Input**:
  1. Terrain backdrop for the ~60x40 map: `seedream`, `--image-config '{"aspect_ratio":"4:3"}'`, prompt: 'top-down view, empty painterly meadow terrain, soft dirt paths radiating outward from a central plaza clearing toward all four edges, gentle flower speckles, NO buildings, NO objects, NO characters, NO text' + verbatim style anchor → `Backdrops/world_terrain.jpg`.
  2. Prop batch via `recraft` (NO image_config — it 400s), each 'transparent background' + anchor: `tree_oak.png`, `tree_pine.png`, `tree_round.png`, `bush.png`, `bush_berry.png` (distinct red-berry bush — future quest item), `flowers.png` (low patch, walk-through), `rock.png`, `npc_villager.png` (friendly painterly villager, front-facing, full body, ~1.2x reeble height — used by REEB-152).
  3. One prompt record per promoted file in `art/prompts/`. Verify alpha on every sprite (IHDR color type 6 + transparent corners).
- **Exit criteria**: all files promoted w/ real alpha where required; prompt records committed; staging untracked.
- **Max new lines**: ~200 (prompt records)
- **Status**: in progress
- **Git**: `feat(REEB-147): generate world expansion art (terrain + props + villager)`
- **CRITICAL**: `.env`/`ROUTELLM_API_KEY` never printed or committed.
