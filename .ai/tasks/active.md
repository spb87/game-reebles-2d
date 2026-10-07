# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-135: Movement + collision tests (includes collision-strategy fix)

- **Feature**: REEB-125 (`m1-greybox-scene-movement.md`)
- **Role**: Developer
- **Files allowed**: `game/Assets/Tests/EditMode/MovementMathTests.cs`, `game/Assets/Tests/PlayMode/PlayerMovementTests.cs`, `game/Assets/Tests/PlayMode/Reebles2D.Tests.PlayMode.asmdef` (add `UnityEngine.InputSystem.TestFramework` + asmdef refs as needed), `game/Assets/Scripts/Player/PlayerMovement.cs` (collision fix — see below), `game/Assets/Tests/EditMode/Reebles2D.Tests.EditMode.asmdef` (only if refs needed)
- **Input**: EditMode `MovementMathTests`: happy path (right input → +x at walkSpeed), run multiplier applied, diagonal normalized (magnitude = walkSpeed not √2×), deadzone → zero. PlayMode `PlayerMovementTests` (`InputTestFixture`): instantiate player (prefab or minimal GameObject w/ PlayerMovement+Rigidbody2D+CircleCollider2D), simulate Move, assert position changes over physics frames; wall test — player next to a `BoxCollider2D` pushed into it does NOT pass through (assert position clamps before the wall face). Input System test API: `InputSystem.AddDevice<Keyboard>()` + `InputTestFixture.Press(...)` — pick what compiles under `com.unity.inputsystem` test framework.
- **COLLISION FIX REQUIRED FIRST**: REEB-134 review confirmed kinematic `Rigidbody2D.MovePosition` does NOT collide with static colliders — the current `PlayerMovement` walks through walls, so the wall test will fail until this is fixed. DEC-4 (amended): use `Rigidbody2D.Slide(velocity, deltaTime, slideMovement)` — Unity 6's kinematic collide-and-slide API — with `SlideMovement` configured for top-down (zero gravity, sensible layer mask covering default statics). If `Slide` proves problematic, fallback: `body.Cast(direction, results, distance)` pre-check + `MovePosition` clamped to hit distance minus skin. Keep `CurrentSpeed`/`IsRunning` semantics and `[SerializeField]` tunables. After the fix, Player.prefab does NOT need regeneration (Slide uses the body's own collider at runtime) — but re-run VillageSceneBuilder.Build only if you change serialized fields the prefab must carry.
- **Exit criteria**: `"/mnt/c/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe" -batchmode -projectPath 'C:\Source\game-reebles-2d\game' -runTests -testPlatform EditMode -testResults 'C:\Source\game-reebles-2d\em.xml' -logFile unity-em.log` → all pass; same for `-testPlatform PlayMode` → `pm.xml` all pass. **NEVER pass `-quit` with `-runTests`** on Unity 6000.3.24f1 — it exits after import without running tests (conventions.md).
- **Max new lines**: ~260 (includes the movement fix)
- **Dependencies**: REEB-134 (done)
- **Status**: in progress
- **Git**: `test(REEB-135): add movement math and playmode collision tests` (collision fix may ride along or be a `fix(REEB-135):` commit)
