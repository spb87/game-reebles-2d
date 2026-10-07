# Project context

Current state of the project. Updated by the Architect after each completed task. This is a snapshot, not a log — stale information should be overwritten, not appended to.

## Current state

Bootstrapped 2026-10-07. Framework docs filled (project-brief, stack, conventions, decisions DEC-1..9), epic `reebles-2d-mvp` created with 5 milestones, M1 decomposed into 3 features / 12 tasks in `backlog.md`. Repo initialized with baseline framework commit.

## What works

- `.ai/` + `.devin/` framework complete and committed
- Environment verified: Unity 6000.3.24f1 + WebGL module on Windows; `gh.exe` authenticated as `spb87`; Python 3.10/3.11 both sides; source assets (`gen_image.py`, `.env`) available in `../game-reebles`

## What's in progress

- M1 walking skeleton, Feature 1 (repo + toolchain scaffold), starting at TASK-1

## What's blocked

- Nothing yet

## Open questions

- Alpha/transparency approach for generated sprites — deferred to first M3 asset task (3-call experiment per scope)
- GitHub Pages propagation timing — first deploy may take minutes after `gh api .../pages` enable (TASK-12)
