# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-143: Directional reeble sprites + prop shadows matching backdrop light

- **Feature**: none — Task directly under epic REEB-123 (DEC-10)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs`, `game/Assets/Scripts/Player/*.cs` (new `PlayerFacing.cs` or extend `PlayerMovement.cs`), `game/Assets/Art/Sprites/*` (+metas), `art/prompts/*.md`, generated `Village.unity`/`Player.prefab`, `game/Assets/Tests/PlayMode/*.cs` if needed
- **Input**:
  1. **Directional reeble**: goal = 4 views (front/down, back/up, left, right) of the SAME creature as `reeble.png`. Try in order: (a) `recraft` 2×2 sheet — "character turnaround sheet, same creature 4 views in a 2x2 grid: front, back, left side, right side, consistent design" + verbatim style anchor, transparent bg; verify actual grid via alpha-gap analysis before slicing (previous sheets came back 3×3 and 4×2 — do NOT assume the requested grid); (b) if sheet is unusable → `*_edit` variants feeding `reeble.png` ("same creature viewed from behind / left side") producing single sprites. Whatever ships: slice/import, one sprite per direction, prompt records committed.
  2. **Facing logic**: new `PlayerFacing` (or fold into PlayerMovement — your call, keep it thin): on direction change, set `SpriteRenderer.sprite` by dominant input axis — up→back, down→front, left/right→side views (flipX allowed to reuse one side view). Idle keeps last facing. Input comes from the same Move action the movement script reads — read it via PlayerMovement (expose direction) or the action directly.
  3. **Shadows**: one shared procedural soft-ellipse sprite (radial-gradient Texture2D, like the existing UI_Rect generation — dark with slight purple/green tint, ~40-60% alpha, blurred edge) created once in the builder. Place under every building, fountain, lantern, and as a child of the player at feet — each offset by a shared `ShadowOffset` const consistent with the backdrop's light (paths in village_ground.jpg shadow lower-left → offset shadows roughly (-x, -y)/down-left of the object base). Shadow sprites sortingOrder just above ground (-9), below objects; scale to each object's footprint.
  4. Rebuild scene → PlayMode run (**NO `-quit`**) 4/4 green → WebGL rebuild → `python3 tools/deploy-pages.py` → verify new gh-pages commit + live URL 200.
- **Exit criteria**: reeble switches sprite by move direction (verify via a PlayMode test or scene-YAML inspection of the facing component wiring); shadows present under all props; PlayMode green; deployed live.
- **Max new lines**: ~300
- **Status**: in progress
- **Git**: `feat(REEB-143): add directional reeble sprites + prop shadows`
- **CRITICAL**: `.env`/`ROUTELLM_API_KEY` never printed or committed.
