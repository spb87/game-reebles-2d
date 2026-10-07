# Project context

Current state of the project. Updated by the Architect after each completed task. This is a snapshot, not a log — stale information should be overwritten, not appended to.

## Current state

Bootstrapped 2026-10-07. Framework docs filled (project-brief, stack, conventions, decisions DEC-1..10), epic `reebles-2d-mvp` created with 5 milestones. **MILESTONE 1 (walking skeleton) is COMPLETE** — greybox game live at https://spb87.github.io/game-reebles-2d/ (public repo `spb87/game-reebles-2d`). Pausing at the M1 boundary for user go/no-go; next is M2 (vertical slice: one quest end-to-end) — decompose only after review.

**Task management: Jira mode** (DEC-10). Epic REEB-123 → Stories REEB-124/125/126 all Done → Subtasks REEB-127…138 all Done (TASK-1…12). `active.md`/`bugs/open.md` are Jira-synced snapshots; `backlog.md`/`done.md` archived to `.ai/tasks/archive/pre-jira-migration/`. Atlassian MCP at local scope (`.devin/mcp_config.local.json`, gitignored).

## What works

- `.ai/` + `.devin/` framework complete and committed (baseline `4a20165`)
- Repo hygiene: `.gitignore` (Unity + `art/staging/` + `.env`), `.env` copied from `../game-reebles` (ROUTELLM_API_KEY present, untracked), `.env.example`, `.editorconfig`
- Art pipeline: `art/tools/gen_image.py` (verbatim port, `--help` works), `art/style-anchor.txt` committed, `art/README.md` documents staging→promotion flow
- Tools: `tools/run-unity.bat`, `tools/serve-webgl.py` (Brotli-aware static server, default dir `game/Builds/Web`)
- Unity project `game/`: Unity 6000.3.24f1, URP 17.3.0 + InputSystem 1.14.0 + Cinemachine 3.1.2 + 2D sprite/tilemap packages; `activeInputHandler: 1`; URP 2D pipeline assets in `Assets/Settings/` assigned to Graphics + all Quality levels
- Game code: `Reebles2D.Player` asmdef — `MovementMath.ComputeVelocity` (deadzone, diagonal-normalized) + `PlayerMovement` (kinematic `Rigidbody2D.Slide`, `[SerializeField]` walkSpeed/runMultiplier, reads actions from serialized `InputActionAsset`); `Reebles2D.UI` asmdef — `MobileControlsHud` (touch-gated via `Input.touchSupported`, safe-area pinned)
- `ReeblesInput.inputactions`: Player map — Move (WASD/arrows/gamepad leftStick), Run (shift×2/gamepadSouth, Hold), Interact (E/gamepadEast); gamepad paths double as the on-screen control paths
- `VillageSceneBuilder.Build` (idempotent, `-executeMethod`): generates greybox sprites → `Player.prefab` (SpriteRenderer + kinematic RB + CircleCollider2D + PlayerMovement wired) → `Village.unity` (30×20 ground, 5 blocking buildings, fountain, 4-sided fence colliders, CinemachineCamera + Confiner2D on trigger CameraBounds, MobileControls canvas + EventSystem)
- Tests green: EditMode 7/7 (`MovementMathTests` + sanity), PlayMode 4/4 (movement, wall-blocking via Slide, run, on-screen controls bound to Player map)
- Batch pipeline proven: `Unity.exe -batchmode -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod ... -quit -logFile X.log` works from WSL
- WebGL pipeline: `WebBuild.Build` → `game/Builds/Web` (compression Disabled — Pages CDN compresses in transit); `tools/deploy-pages.py` — idempotent deploy to `gh-pages` branch + Pages enable + propagation poll
- **DEPLOYED**: https://spb87.github.io/game-reebles-2d/ — public, HTTP 200, `.nojekyll` on gh-pages

## What's in progress

- Nothing — M1 boundary pause; M2 vertical-slice decomposition awaits go/no-go

## What's blocked

- Nothing

## Open questions

- Alpha/transparency approach for generated sprites — deferred to first M3 asset task (3-call experiment per scope)
- GitHub Pages propagation timing — first deploy may take minutes after `gh api .../pages` enable (TASK-12)
- TASK-12 will create public repo `spb87/game-reebles-2d` and push — flag for user awareness when reached

## Pipeline gotchas discovered

- `-runTests` + `-quit` on Unity 6000.3.24f1: `-quit` exits before tests run — omit it for test runs (recorded in conventions.md)
- `run-unity.bat` echoes `UNITY_EXIT=` but doesn't propagate it — callers parse output or invoke `Unity.exe` directly
- Kinematic `MovePosition` does NOT collide with static colliders — use `Rigidbody2D.Slide` (DEC-4 amended, REEB-135)
- `InputTestFixture.Press()` throws `ArgumentNullException` on Input System 1.14 — queue `KeyboardState` events instead
- PlayMode tests that `LoadSceneInPlayMode(Village)` must restore scene state — colliders at origin pin subsequently-spawned test players
