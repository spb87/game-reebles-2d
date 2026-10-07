# Active tasks

Tasks currently being worked on by a Developer agent. Maximum one task active at a time.

---

### TASK-5: URP 2D pipeline bootstrap

- **Feature**: m1-repo-toolchain-scaffold.md
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/ProjectBootstrap.cs` (+ generated `game/Assets/Settings/*.asset` files — authorized outputs, committed)
- **Input**: Write `Reebles2D.Editor.ProjectBootstrap` with a static `Setup()` method run via `-executeMethod Reebles2D.Editor.ProjectBootstrap.Setup`: create `Renderer2DData` (`UnityEngine.Rendering.Universal.Renderer2DData` — URP 17.x API) and `UniversalRenderPipelineAsset` under `game/Assets/Settings/` via `AssetDatabase.CreateAsset`, assign `GraphicsSettings.defaultRenderPipeline` and `QualitySettings.renderPipeline`, `AssetDatabase.SaveAssets()`. Note: in URP 17 the create call is `UniversalRenderPipelineAsset.Create(rendererData)` (ScriptableRendererData arg); ensure the script sits under `Assets/Editor/` so it compiles into the editor assembly. Then run `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod Reebles2D.Editor.ProjectBootstrap.Setup -quit -logFile unity-urp.log`. Do NOT create scenes or gameplay assets.
- **Exit criteria**: Unity run exits 0; `ls game/Assets/Settings/` shows the pipeline asset + renderer data; `grep 'm_CustomRenderPipeline' game/ProjectSettings/GraphicsSettings.asset` and `game/ProjectSettings/QualitySettings.asset` show a non-`{fileID: 0}` reference; no `error CS` in the log.
- **Max new lines**: ~70
- **Dependencies**: TASK-4 (done)
- **Status**: in progress
- **Git**: `feat(TASK-5): add URP 2D pipeline bootstrap and settings assets`
