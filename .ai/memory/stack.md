# Tech stack

The agreed-upon technology stack for this project. Updated by the Architect when stack decisions are made.

## Language / Runtime

- C# 9.0 — Unity 6 scripting (`.NET Standard 2.1` API level; file-scoped namespaces are NOT available — C# 9 max)
- Python 3.10+ — art-pipeline tooling only; scripts must be **stdlib-only** (no pip installs) so they run on stock Windows Python and WSL Python alike

## Framework

- **Unity 6000.3.24f1 (Unity 6.3 LTS)** — changeset `4e7b9b5b6244`; same install as the archived 3D project, do NOT use the `6000.6.3f1` editor also present on the machine
  - Editor: `C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe` (Windows binary; from WSL invoke via `/mnt/c/...` path with Windows-style `-projectPath`)
  - WebGL build module: **installed** (`Editor/Data/PlaybackEngines/WebGLSupport`)
  - URP **2D Renderer** — top-down/oblique 2D; no 3D assets, no lighting stack beyond URP 2D defaults
- **RouteLLM image API** (`https://routellm.abacus.ai/v1`) — all game art; `art/tools/gen_image.py` helper (stdlib-only, reads `ROUTELLM_API_KEY` from repo `.env`). Default model `seedream`; `nano_banana_pro`/`gpt_image25` for hero shots; `*_edit` variants for iteration.

## Database

- None — no persistence in v1. Quest data ships as JSON text assets inside the build.

## Key dependencies

| Package | Purpose |
|---------|---------|
| `com.unity.render-pipelines.universal` | URP with the 2D Renderer |
| `com.unity.inputsystem` | Input System — keyboard/mouse and touch joystick behind one action map |
| `com.unity.cinemachine` (3.x) | 2D follow camera + confiner to map bounds |
| `com.unity.test-framework` | EditMode + PlayMode tests |
| `com.unity.ugui` 2.0 | HUD/dialogue canvas + mobile on-screen controls (TMP is inside uGUI 2.0 in Unity 6.3 — no separate TMP package) |
| `com.unity.2d.sprite` / `com.unity.feature.2d` | Sprite slicing (asset sheets → sprites), tilemap tooling if needed |

## Dev tools

- Unity Test Framework — EditMode tests (pure logic) and PlayMode tests (movement/interaction/quest behaviors)
- Unity batch mode for scripted verification and builds:
  `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -projectPath 'C:\Source\game-reebles-2d\game' ...`
- `tools/serve-webgl.py` — local static server for Brotli-compressed Web builds (ported from 3D project)
- `tools/run-unity.bat` — Windows-side Unity shim (ported from 3D project)
- `gh.exe` (Windows side, authenticated as `spb87`, `repo`+`workflow` scopes) — GitHub repo + Pages deploy
- `.editorconfig` at repo root for C# style
- No engine MCP servers in v1 — batch-mode tests/builds are the verification path (DEC-8)

## Deployment

- Unity Web build (Brotli) + PWA template → **GitHub Pages** (DEC-3), repo `spb87/game-reebles-2d` (public)
- Builds are produced **locally** by the pinned editor and pushed to the `gh-pages` branch via `tools/deploy-pages` — no CI Unity builds (licensing)

## Environment topology

- Repo lives at `C:\Source\game-reebles-2d`; Devin CLI runs in WSL (`/mnt/c/Source/game-reebles-2d`)
- Unity Editor runs on Windows; Devin's file/exec tools operate via WSL — Unity is invoked through WSL→Windows interop (full `/mnt/c` path to `Unity.exe`, Windows-style args)
- WSL networking: NAT mode (mirrored mode breaks VirtualBox host-only networks — do not retry). Windows host reachable via Tailscale `100.118.197.112` if a socket path is ever needed.

## Repo layout

```
game-reebles-2d/
├── .ai/, .devin/                # agent framework
├── game/                        # Unity project (Assets/, Packages/, ProjectSettings/)
├── art/                         # image-gen pipeline: tools/, prompts/, staging/ (gitignored)
├── tools/                       # build/serve/deploy helpers
├── .env                         # ROUTELLM_API_KEY — gitignored, never committed
└── reebles-2d-project-scope.md  # MVP scope v1 (source of truth)
```
