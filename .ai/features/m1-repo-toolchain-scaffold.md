# Feature: Repo & Toolchain Scaffold

**Jira Issue**: [REEB-124](https://stan-butler.atlassian.net/browse/REEB-124) (Story) · Epic: [REEB-123](https://stan-butler.atlassian.net/browse/REEB-123) · Milestone: M1

## Goal

The repo is a working Unity 2D project: `game/` opens in Unity 6000.3.24f1 with URP 2D Renderer, Input System, Cinemachine, and test framework; hygiene files, the ported art pipeline, and repo tools are in place.

## Acceptance criteria

1. `git` repo initialized; `.gitignore` covers Unity noise (`game/Library/`, `game/Temp/`, `game/Obj/`, `game/Builds/`, `game/Logs/`, `*.csproj`, `*.sln`), `.env`, and `art/staging/` — verified via `git check-ignore`
2. `.env` exists at repo root containing `ROUTELLM_API_KEY` (copied from `../game-reebles/.env`) and is gitignored; `.env.example` committed with a placeholder
3. `art/tools/gen_image.py` runs (`--help` exits 0); `art/style-anchor.txt` committed; `art/README.md` documents the pipeline
4. `game/` is a valid Unity 6000.3.24f1 project: `ProjectVersion.txt` pinned; `Packages/manifest.json` includes URP, Input System, Cinemachine, test-framework; `activeInputHandler` = Input System
5. URP 2D pipeline assets exist under `game/Assets/Settings/` and are assigned to Graphics + Quality settings
6. `Unity.exe -batchmode -projectPath game -runTests -testPlatform EditMode` passes (sanity test green); zero compile errors/warnings in the editor log
7. `README.md` documents prerequisites, setup, build, test, and local-serve commands

## Known constraints

- Unity runs on Windows; from WSL invoke `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe"` with **Windows-style** `-projectPath 'C:\Source\game-reebles-2d\game'`. Proven pattern from `../game-reebles`: `tools/run-unity.bat` wraps the binary.
- C# 9 max — no file-scoped namespaces (DEC-1). Layout per DEC-2.
- Repo will be **public** on GitHub (DEC-3) — `.env` must never be committed.
- Package versions pinned by `manifest.json`/`ProjectVersion.txt` — no "latest".
- Unity `-createProject` in batchmode writes directly on `/mnt/c` (9P fs) — slow but workable; generate once, reuse.

## Out of scope

- Any gameplay code, scenes, or generated art
- Real generation calls to RouteLLM (first asset task in M3 does the alpha experiment)
- CI/CD, itch.io, engine MCP servers

## Tasks

| Jira | Summary | Status | Depends on |
|------|---------|--------|------------|
| REEB-127 | TASK-1: Repo hygiene + secrets | ✅ Done | — |
| REEB-128 | TASK-2: Art pipeline port | ✅ Done | REEB-127 |
| REEB-129 | TASK-3: Repo tools + README | ✅ Done | REEB-127 |
| REEB-130 | TASK-4: Unity project creation + packages | ✅ Done | REEB-127 |
| REEB-131 | TASK-5: URP 2D pipeline bootstrap | ✅ Done | REEB-130 |
| REEB-132 | TASK-6: Test assemblies + sanity test | ✅ Done | REEB-131 |

## Status

- [x] Spec complete
- [x] Tasks decomposed
- [x] Cost estimated
- [x] All tasks done (TASK-1…6, commits 78a43f2…4fc3c46)
- [x] Feature verified — acceptance criteria 1-7 independently verified 2026-10-07 (gitignore checks, .env present+untracked, gen_image.py --help, manifest/InputSystem/ProjectVersion pinned, URP 2D pipeline assigned, EditMode 2/2 green, README complete)
