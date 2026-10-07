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

```
# Build a WebGL build into game/Builds/Web (build task pending — see TASK-11)
# Then serve the Brotli-compressed build:
python3 tools/serve-webgl.py            # serves game/Builds/Web on :8080
```

## Deployment

The game deploys to **GitHub Pages** (repo `spb87/game-reebles-2d`, public). Builds are produced locally by the pinned editor and pushed to the `gh-pages` branch by a deploy script — no CI Unity builds. See DEC-3 in `.ai/memory/decisions.md`.

## Repo layout

```
game/    Unity project (Assets/, Packages/, ProjectSettings/)
art/     image-gen pipeline: tools/, prompts/, staging/ (gitignored)
tools/   build/serve/deploy helpers (run-unity.bat, serve-webgl.py, ...)
.env     ROUTELLM_API_KEY — gitignored, never committed
```
