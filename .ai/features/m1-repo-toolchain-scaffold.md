# Feature: Repo & Toolchain Scaffold

**Epic**: `reebles-2d-mvp.md` · Milestone: M1 · Tracking: local (`TASK-1` … `TASK-6`)

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

- TASK-1: Repo hygiene + secrets (.gitignore, .env.example, .editorconfig, .env copy)
- TASK-2: Art pipeline port (gen_image.py, style-anchor.txt, art/README.md)
- TASK-3: Repo tools + README (run-unity.bat, serve-webgl.py, README.md)
- TASK-4: Unity project creation + package manifest + Input System setting
- TASK-5: URP 2D pipeline assets via editor bootstrap script
- TASK-6: Test asmdefs + sanity EditMode test

## Status

- [x] Spec complete
- [x] Tasks decomposed
- [x] Cost estimated
- [ ] All tasks done
- [ ] Feature verified
