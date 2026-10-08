# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-155: Q3 — E2E quest loop PlayMode test + final rebuild + redeploy

- **Feature**: REEB-146 (M2: One quest end-to-end — vertical slice)
- **Role**: Developer
- **Files allowed**: `game/Assets/Tests/PlayMode/QuestLoopTests.cs` + wiring-only fixes in `game/Assets/Editor/VillageSceneBuilder.cs` if a test exposes a gap
- **Input**: End-to-end PlayMode test of the full loop: player next to Marla → Interact → card opens w/ offer lines → quest active (objective text set) → move to berry bush → Interact → slot holds 'berries' → back to Marla → Interact → complete lines → hearts = 1 → toast/objective updated. Use the harness pattern from existing QuestLoopTests/DialogueTests (prefab instantiation or scene load — if Village.unity is loaded, restore state after). Then: full EditMode + PlayMode runs green → WebGL rebuild (`-executeMethod Reebles2D.Editor.WebBuild.Build -quit`, Windows log path `C:\Source\game-reebles-2d\webgl-build.log`) → `python3 tools/deploy-pages.py` → verify new gh-pages commit + `curl -sI https://spb87.github.io/game-reebles-2d/ | head -1` → 200.
- **Exit criteria**: E2E loop test green; full suites green; new gh-pages commit; live URL 200.
- **Max new lines**: ~150
- **Status**: in progress
- **Git**: `test(REEB-155): e2e quest loop test + redeploy`
