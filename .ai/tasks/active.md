# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-134: VillageSceneBuilder — greybox scene

- **Feature**: REEB-125 (`m1-greybox-scene-movement.md`)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs` (+ generated `game/Assets/Scenes/Village.unity`, `game/Assets/Prefabs/*.prefab`, `game/Assets/Art/Greybox/*` placeholder sprites — authorized outputs, committed)
- **Input**: Single static `Build()` via `-executeMethod Reebles2D.Editor.VillageSceneBuilder.Build`: (a) `Player.prefab` — kinematic `Rigidbody2D`, `CircleCollider2D`, `SpriteRenderer` w/ generated placeholder sprite (Texture2D colored rect), `PlayerMovement` wired to `ReeblesInput` (assign the `inputActions` serialized field); (b) `Village.unity` — ground backdrop rect (~30×20 world units, ~2-3 screens), 5 building rects (bakery/smithy/herbalist/store/inn, muted terracotta) each with blocking `BoxCollider2D`, fountain circle at plaza center w/ `CircleCollider2D`, perimeter fence: visible thin rects + colliders enclosing the map (fence principle — visible boundary), Player instance at plaza, camera + `CinemachineCamera` 2D follow + `CinemachineConfiner2D` to map bounds (confiner bounds collider must NOT block the player). Greybox palette per `.ai/design/brief.md`. Idempotent: re-running Build regenerates cleanly.
- **Exit criteria**: `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -nographics -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod Reebles2D.Editor.VillageSceneBuilder.Build -quit -logFile unity-scene.log` exits 0; `Village.unity` + `Player.prefab` exist; no `error CS`.
- **Max new lines**: ~350
- **Dependencies**: REEB-133 (PlayerMovement + input asset — done)
- **Status**: in progress
- **Git**: `feat(REEB-134): add VillageSceneBuilder generating greybox village`
