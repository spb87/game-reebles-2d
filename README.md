# Reebles 2D

A top-down 2D village game built in Unity 6 — port of the archived 3D prototype, rebuilt for fast tuning loops and a public web deploy.

## Prerequisites

- **Unity 6000.3.24f1** (Unity 6.3 LTS) on Windows, with the WebGL build module installed. Do **not** use any other editor version — the project is pinned via `game/ProjectSettings/ProjectVersion.txt`.
- **Python 3.10+** for the art pipeline and repo tools (all scripts are stdlib-only — no pip installs).

## Setup

1. Clone the repo.
2. Copy `.env.example` to `.env` and fill in `ROUTELLM_API_KEY` (needed only for the art-generation pipeline; `.env` is gitignored — never commit it).

## Opening the project

Open the `game/` folder in Unity Hub / the Unity Editor. Or from Windows cmd:

```
tools\run-unity.bat -projectPath C:\Source\game-reebles-2d\game
```

## Running tests

EditMode + PlayMode tests run in the Unity Test Runner, or in batch mode:

```
tools\run-unity.bat -batchmode -projectPath C:\Source\game-reebles-2d\game -runTests -testPlatform EditMode -testResults results.xml
```

From WSL, invoke the editor via the full `/mnt/c/...` path with Windows-style `-projectPath` (see `.ai/memory/stack.md`).

## Build and serve locally

Build a WebGL player into `game/Builds/Web` (gitignored) in batch mode:

```
# Windows cmd
tools\run-unity.bat -batchmode -nographics -buildTarget WebGL -projectPath C:\Source\game-reebles-2d\game -executeMethod Reebles2D.Editor.WebBuild.Build -quit

# WSL
"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -nographics -buildTarget WebGL -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod Reebles2D.Editor.WebBuild.Build -quit
```

The first build runs IL2CPP codegen and can take 10–30+ minutes. Compression is disabled (`WebGLCompressionFormat.Disabled`) because GitHub Pages' CDN applies transport compression itself.

Then serve the build and verify it responds:

```
python3 tools/serve-webgl.py --dir game/Builds/Web --port 8080
curl -sI http://localhost:8080/        # expect HTTP/1.0 200
```

## Deployment

The game deploys to **GitHub Pages** (repo `spb87/game-reebles-2d`, public). Builds are produced locally by the pinned editor and pushed to the `gh-pages` branch by a deploy script — no CI Unity builds. See DEC-3 in `.ai/memory/decisions.md`.

Live site: <https://spb87.github.io/game-reebles-2d/>

```
python3 tools/deploy-pages.py
```

The script is idempotent: it creates the repo/`origin` remote if missing, pushes `main`, stages `game/Builds/Web/*` onto an orphan `gh-pages` branch in a temp clone (with `.nojekyll`), pushes it, and enables Pages. On WSL it uses `gh.exe` (Windows GitHub CLI) for auth — no stored git credentials are required. First deploys take a few minutes to propagate; recheck with `curl -sI https://spb87.github.io/game-reebles-2d/ | head -1`.

## Repo layout

```
game/    Unity project (Assets/, Packages/, ProjectSettings/)
art/     image-gen pipeline: tools/, prompts/, staging/ (gitignored)
tools/   build/serve/deploy helpers (run-unity.bat, serve-webgl.py, ...)
.env     ROUTELLM_API_KEY — gitignored, never committed
```
