# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-156 (Bug): Camera never follows the Reeble — missing PositionComposer + Brain

- **Feature**: none — Bug directly under epic REEB-123 (DEC-10)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs`, generated `Village.unity`, `game/Assets/Tests/PlayMode/*.cs`
- **Input**: In `Build()` (~line 310-322): the Main Camera GameObject gets a `Camera` but no `CinemachineBrain`, and `PlayerCamera`'s `CinemachineCamera` has Follow set but no `CinemachinePositionComposer` — so nothing drives camera position. Fix: add `Unity.Cinemachine.CinemachineBrain` to the Main Camera object and `Unity.Cinemachine.CinemachinePositionComposer` to the vcam object (zero or tiny dead zone — user wants the reeble centered while walking; keep CinemachineConfiner2D so the camera still rests at world edges). Regenerate scene. PlayMode test: instantiate prefab player + vcam w/ composer + brain'd camera, move the player N units → camera.x/y tracks (player's WorldToScreenPoint stays near screen center). Then WebGL rebuild + `python3 tools/deploy-pages.py` + live URL 200.
- **Exit criteria**: camera follows player within confiner bounds; PlayMode green incl. new follow test; deployed (gh-pages commit + live 200).
- **Max new lines**: ~120
- **Status**: in progress
- **Git**: `fix(REEB-156): wire Cinemachine brain + position composer so camera follows player`
