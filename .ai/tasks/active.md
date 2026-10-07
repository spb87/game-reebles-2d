# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-141: Art pass 1B — integrate art into scene builder + rebuild + redeploy

- **Feature**: REEB-139 (`first-art-pass.md`)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs`, generated outputs `game/Assets/Scenes/Village.unity` / `game/Assets/Prefabs/Player.prefab` / sprite-slice assets, `game/Assets/Tests/PlayMode/*.cs` only if a test needs updating
- **Input**: Replace every procedural Texture2D sprite with promoted art from `game/Assets/Art/` (Sprites/: building_{bakery,smithy,herbalist,store,inn}.png, fountain.png, fence.png, lantern.png, reeble_sheet.png; Backdrops/ground_grass.jpg). Add an `EnsureSpriteImport` step (TextureImporter: `textureType = Sprite`, `alphaIsTransparency = true` for RGBA assets, `filterMode = bilinear`, `pixelsPerUnit` tuned so buildings land ~4-6 world units wide — sprites are 1024² so PPU ~170-250 for buildings; scale the look with PPU and/or transform, keep consistent). `reeble_sheet.png` is a 4×2 grid → `spriteImportMode = Multiple` + `spritesheet` slicing to `UseFirstSprite` for the prefab (or just use a sub-slice; simplest: `spriteImportMode Multiple` with a sprite sheet dividing into 4 cols × 2 rows, player uses cell 0). Ground: `SpriteRenderer` `drawMode = Tiled` + sized to 30×20, or stretched single backdrop — pick whichever reads better. Buildings/fountain/fence colliders size to the FOOTPRINT (base ~bottom third of sprite), not the full sprite. Keep layout/positions identical — only visuals change. Then: run builder (batch `-executeMethod`, `-logFile` must be a WINDOWS path), run PlayMode suite (NO `-quit`) — 4/4 must stay green (wall test intact). Then rebuild: `Unity.exe -batchmode -nographics -buildTarget WebGL -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod Reebles2D.Editor.WebBuild.Build -quit -logFile 'C:\Source\game-reebles-2d\webgl-build.log'` (~20min IL2CPP, let it finish). Then `python3 tools/deploy-pages.py`, verify `git ls-remote origin gh-pages` has a new commit and `curl -sI https://spb87.github.io/game-reebles-2d/` → 200.
- **Exit criteria**: Village.unity sprite references point at `Assets/Art/` assets; PlayMode 4/4 green; new gh-pages commit pushed; live URL 200.
- **Max new lines**: ~300
- **Dependencies**: REEB-140 (done)
- **Status**: in progress
- **Git**: `feat(REEB-141): integrate generated art into village scene + redeploy`
