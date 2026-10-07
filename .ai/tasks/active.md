# Active tasks

Tasks currently being worked on by a Developer agent. Maximum one task active at a time.

---

### TASK-6: Test assemblies + sanity test

- **Feature**: m1-repo-toolchain-scaffold.md
- **Role**: Developer
- **Files allowed**: `game/Assets/Tests/EditMode/Reebles2D.Tests.EditMode.asmdef`, `game/Assets/Tests/PlayMode/Reebles2D.Tests.PlayMode.asmdef`, `game/Assets/Tests/EditMode/SanityTests.cs`
- **Input**: EditMode asmdef: `Reebles2D.Tests.EditMode`, references `UnityEngine.TestRunner`/`UnityEditor.TestRunner`, `includePlatforms: ["Editor"]`. PlayMode asmdef: `Reebles2D.Tests.PlayMode`, optional references `UnityEngine.TestRunner`, `UnityEditor.TestRunner` (leave `overrideReferences`/`precompiledReferences` consistent with what `-runTests` needs — a PlayMode asmdef should NOT be editor-only). `SanityTests.cs`: keep it minimal but honest — e.g. `Assert.That(Application.unityVersion, Does.StartWith("6000.3"))` so the sanity test also guards the pinned-version DEC-1. Verify: `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -projectPath 'C:\Source\game-reebles-2d\game' -runTests -testPlatform EditMode -testResults 'C:\Source\game-reebles-2d\editmode-results.xml' -quit -logFile unity-test.log`. (`*-results.xml` is gitignored.)
- **Exit criteria**: Unity run exits 0; `editmode-results.xml` shows ≥1 passed / 0 failed; no `error CS` in `unity-test.log`.
- **Max new lines**: ~60
- **Dependencies**: TASK-5 (done)
- **Status**: in progress
- **Git**: `test(TASK-6): add EditMode/PlayMode test assemblies and sanity test`
