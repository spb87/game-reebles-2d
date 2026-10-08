# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-148: W2 — Builder: expanded map + treeline perimeter + prop scatter

- **Feature**: REEB-144 (M2: World expansion — outskirts beyond the village)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs`, generated `Village.unity`/`Player.prefab`, `game/Assets/Tests/PlayMode/*.cs` if tests need adjusting
- **Input**: Extend `VillageSceneBuilder.Build` — MapBounds → 60x40 centered on origin. Backdrop: `Backdrops/world_terrain.jpg` single stretched SpriteRenderer (NO tiling — the 4:3 image covers 60x40 ≈ 3:2; mild vertical squash acceptable). Village core UNCHANGED (buildings/fountain/plaza positions as-is, centered — the backdrop's painted plaza circle lands near the fountain). Perimeter: replace perimeter picket fence with a **treeline** — repeating tree sprites (alternate tree_oak/tree_pine/tree_round) spaced ~1.2-1.5u hugging all 4 edges just inside bounds + 4 thin BoxCollider2D strips enclosing the map (collider = the wall). Prop scatter between village core and treeline: ~15-25 trees/bushes/rocks with footprint colliders + pass-through flower patches; keep paths/plaza clear and routes to buildings open. 2-3 `bush_berry` bushes in the outskirts (future quest items). CameraBounds/confiner → 60x40. Extend the existing shared-shadow placement to new props (trees/bushes/rocks; flowers optional — they're flat on ground, skip shadow).
- **Exit criteria**: builder exits 0; Village.unity regenerated w/ 60x40 bounds + treeline + props; PlayMode suite stays green (existing tests + wall-blocking still passes).
- **Max new lines**: ~250
- **Status**: in progress
- **Git**: `feat(REEB-148): expand world to 60x40 with treeline edge + props`
