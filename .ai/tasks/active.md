# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-149: W3 — Bounds tests + rebuild + redeploy expanded world

- **Feature**: REEB-144 (M2: World expansion — outskirts beyond the village)
- **Role**: Developer
- **Files allowed**: `game/Assets/Tests/PlayMode/*.cs`, `game/Assets/Editor/VillageSceneBuilder.cs` (only if test-driven fixes needed)
- **Input**: Extend PlayMode tests: player cannot leave the new 60x40 bounds via treeline (drive player at an edge, assert position clamps inside bounds); camera confiner bound check if cheap. Then rebuild WebGL (`run-unity.bat ... -executeMethod Reebles2D.Editor.WebBuild.Build -quit`, WINDOWS log path like `C:\Source\game-reebles-2d\webgl-build.log`), `python3 tools/deploy-pages.py`, verify new gh-pages commit + live URL 200.
- **Exit criteria**: PlayMode suite green incl. new bounds test; new gh-pages commit; `curl -sI https://spb87.github.io/game-reebles-2d/` → 200.
- **Max new lines**: ~120
- **Status**: in progress
- **Git**: `test(REEB-149): verify expanded world bounds + redeploy`
