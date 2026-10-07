# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-136: Mobile on-screen controls

- **Feature**: REEB-125 (`m1-greybox-scene-movement.md`)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs` (extend — adds controls canvas), `game/Assets/Scripts/UI/MobileControlsHud.cs`, `game/Assets/Input/ReeblesInput.inputactions` (touch/on-screen bindings), `game/Assets/Tests/PlayMode/*.cs` (test additions), `game/Assets/Scripts/UI/Reebles2D.UI.asmdef` (new — required so PlayMode tests can reference MobileControlsHud; follow the Reebles2D.Player.asmdef pattern with `autoReferenced: true`)
- **Input**: Extend `VillageSceneBuilder.Build` to generate a `MobileControls` uGUI canvas (screen-space overlay; add an EventSystem if the generated scene lacks one): `OnScreenStick` bound to `Move` (bottom-left), `OnScreenButton` bound to `Interact` (bottom-right) — both feeding the SAME `Player` action map. On-screen controls emit input on a `controlPath` that must have a matching binding on the action — for `OnScreenStick`/`OnScreenButton` in Input System 1.14 the canonical control paths are the gamepad-style paths (`<Gamepad>/leftStick` for stick, `<Gamepad>/buttonEast` or a `*/{Pressed}`-style path for button), which the existing gamepad bindings may already satisfy; verify against the OnScreenControls sample/docs in `game/Library/PackageCache/com.unity.inputsystem@1.14.0/` (authoritative for this version) and add any missing binding to the `.inputactions` file. `MobileControlsHud.cs` (namespace `Reebles2D.UI`): show controls only when `Input.touchSupported` at runtime (NO `#if` — WebGL reports touch at runtime); `[SerializeField]` override toggle for desktop testing. Safe-area aware: use `Screen.safeArea` or anchored margins — keep it simple.
- **Exit criteria**: `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -nographics -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod Reebles2D.Editor.VillageSceneBuilder.Build -quit -logFile unity-scene.log` exits 0 (scene regen includes canvas); PlayMode run `-runTests -testPlatform PlayMode -testResults 'C:\Source\game-reebles-2d\pm.xml'` (**NO `-quit`**) stays green AND gains ≥1 test asserting the on-screen controls exist and feed the `Player` map (assert via Input System APIs — e.g. control paths resolve to actions in the Player map); no `error CS`.
- **Max new lines**: ~220
- **Dependencies**: REEB-135 (done)
- **Status**: in progress
- **Git**: `feat(REEB-136): add mobile on-screen controls to scene builder`
