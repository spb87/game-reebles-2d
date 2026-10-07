# Project context

Current state of the project. Updated by the Architect after each completed task. This is a snapshot, not a log — stale information should be overwritten, not appended to.

## Current state

Bootstrapped 2026-10-07. Framework docs filled (project-brief, stack, conventions, decisions DEC-1..9), epic `reebles-2d-mvp` created with 5 milestones, M1 decomposed into 3 features / 12 tasks. **M1 Feature 1 (repo + toolchain scaffold) is complete** — awaiting user check-in at the feature boundary.

## What works

- `.ai/` + `.devin/` framework complete and committed (baseline `4a20165`)
- Repo hygiene: `.gitignore` (Unity + `art/staging/` + `.env`), `.env` copied from `../game-reebles` (ROUTELLM_API_KEY present, untracked), `.env.example`, `.editorconfig`
- Art pipeline: `art/tools/gen_image.py` (verbatim port, `--help` works), `art/style-anchor.txt` committed, `art/README.md` documents staging→promotion flow
- Tools: `tools/run-unity.bat`, `tools/serve-webgl.py` (Brotli-aware static server, default dir `game/Builds/Web`)
- Unity project `game/`: Unity 6000.3.24f1, URP 17.3.0 + InputSystem 1.14.0 + Cinemachine 3.1.2 + 2D sprite/tilemap packages; `activeInputHandler: 1`; URP 2D pipeline assets in `Assets/Settings/` assigned to Graphics + all Quality levels; EditMode+PlayMode test asmdefs; EditMode tests green (2/2)
- Batch pipeline proven: `Unity.exe -batchmode -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod ... -quit -logFile X.log` works from WSL

## What's in progress

- M1 walking skeleton — Feature 2 (greybox scene + player movement, TASK-7…10) next

## What's blocked

- Nothing

## Open questions

- Alpha/transparency approach for generated sprites — deferred to first M3 asset task (3-call experiment per scope)
- GitHub Pages propagation timing — first deploy may take minutes after `gh api .../pages` enable (TASK-12)
- TASK-12 will create public repo `spb87/game-reebles-2d` and push — flag for user awareness when reached

## Pipeline gotchas discovered

- `-runTests` + `-quit` on Unity 6000.3.24f1: `-quit` exits before tests run — omit it for test runs (recorded in conventions.md)
- `run-unity.bat` echoes `UNITY_EXIT=` but doesn't propagate it — callers parse output or invoke `Unity.exe` directly
