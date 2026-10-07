# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-137: Web build pipeline + local serve verify

- **Feature**: REEB-126 (`m1-web-smoke-deploy.md`)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/WebBuild.cs`, `game/ProjectSettings/ProjectSettings.asset` (compression/productName), `README.md` (build/serve section)
- **Input**: `Reebles2D.Editor.WebBuild.Build()` — `BuildPipeline.BuildPlayer` for `BuildTarget.WebGL`; set `PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled` (GitHub Pages' CDN applies transport compression itself; Unity's `.br` precompressed files need `Content-Encoding` Pages won't emit — see feature spec; revisit at M5), `productName = "Reebles 2D"`, scenes = `Assets/Scenes/Village.unity` (run `VillageSceneBuilder.Build` as a prior batch step if the scene is missing — do NOT call it inside Build), output `game/Builds/Web`. Invoke with `-buildTarget WebGL` so platform defines are right. Then serve `python3 tools/serve-webgl.py --dir game/Builds/Web --port 8080` and `curl -sI localhost:8080/` → 200; report `du -sh game/Builds/Web` in the completion report.
- **Exit criteria**: `test -f game/Builds/Web/index.html`; `curl -sI http://localhost:8080/ | head -1` = 200; build size reported; no `error CS` in the build log.
- **Max new lines**: ~90
- **Dependencies**: REEB-136 (done)
- **Status**: in progress
- **Git**: `feat(REEB-137): add Web build pipeline (WebBuild.cs) + build docs`
- **Note**: the WebGL build can take many minutes on first run (il2cpp codegen) — keep the Unity process running until it exits; don't kill it early.
