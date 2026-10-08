# Project context

Current state of the project. Updated by the Architect after each completed task. This is a snapshot, not a log — stale information should be overwritten, not appended to.

## Current state

Bootstrapped 2026-10-07. Framework docs filled (project-brief, stack, conventions, decisions DEC-1..10), epic `reebles-2d-mvp` created with 5 milestones. **MILESTONE 1 (walking skeleton) COMPLETE + first art pass done** — painterly art live at https://spb87.github.io/game-reebles-2d/ (public repo `spb87/game-reebles-2d`). Next: M2 (vertical slice: one quest end-to-end) — decompose only after review.

**Task management: Jira mode** (DEC-10). Epic REEB-123 → Stories REEB-124/125/126/139 all Done → Subtasks REEB-127…138, REEB-140/141 all Done. `active.md`/`bugs/open.md` are Jira-synced snapshots; `backlog.md`/`done.md` archived to `.ai/tasks/archive/pre-jira-migration/`. Atlassian MCP at local scope (`.devin/mcp_config.local.json`, gitignored).

## What works

- `.ai/` + `.devin/` framework complete and committed (baseline `4a20165`)
- Repo hygiene: `.gitignore` (Unity + `art/staging/` + `.env`), `.env` copied from `../game-reebles` (ROUTELLM_API_KEY present, untracked), `.env.example`, `.editorconfig`
- Art pipeline: `art/tools/gen_image.py` (verbatim port, `--help` works), `art/style-anchor.txt` committed, `art/README.md` documents staging→promotion flow
- Tools: `tools/run-unity.bat`, `tools/serve-webgl.py` (Brotli-aware static server, default dir `game/Builds/Web`)
- Unity project `game/`: Unity 6000.3.24f1, URP 17.3.0 + InputSystem 1.14.0 + Cinemachine 3.1.2 + 2D sprite/tilemap packages; `activeInputHandler: 1`; URP 2D pipeline assets in `Assets/Settings/` assigned to Graphics + all Quality levels
- Game code: `Reebles2D.Player` asmdef — `MovementMath.ComputeVelocity` (deadzone, diagonal-normalized) + `PlayerMovement` (kinematic `Rigidbody2D.Slide`, `[SerializeField]` walkSpeed/runMultiplier, reads actions from serialized `InputActionAsset`); `Reebles2D.UI` asmdef — `MobileControlsHud` (touch-gated via `Input.touchSupported`, safe-area pinned)
- `ReeblesInput.inputactions`: Player map — Move (WASD/arrows/gamepad leftStick), Run (shift×2/gamepadSouth, Hold), Interact (E/gamepadEast); gamepad paths double as the on-screen control paths
- `VillageSceneBuilder.Build` (idempotent, `-executeMethod`): sets sprite imports + slices `reeble_sheet` → `Player.prefab` (SpriteRenderer + kinematic RB + CircleCollider2D + PlayerMovement wired) → `Village.unity` (30×20 tiled grass backdrop, 5 building sprites w/ footprint colliders, fountain, tiled fence, lanterns, CinemachineCamera + Confiner2D on trigger CameraBounds, MobileControls canvas + EventSystem)
- Tests green: EditMode 7/7 (`MovementMathTests` + sanity), PlayMode 4/4 (movement, wall-blocking via Slide, run, on-screen controls bound to Player map)
- Batch pipeline proven: `Unity.exe -batchmode -projectPath 'C:\Source\game-reebles-2d\game' -executeMethod ... -quit -logFile X.log` works from WSL
- WebGL pipeline: `WebBuild.Build` → `game/Builds/Web` (compression Disabled — Pages CDN compresses in transit); `tools/deploy-pages.py` — idempotent deploy to `gh-pages` branch + Pages enable + propagation poll
- **Art**: RouteLLM pipeline live — `recraft` = the alpha model (omit `--image-config` for it/gpt_image25, HTTP 400); assets promoted in `Assets/Art/` (5 buildings, fountain, fence, lantern, reeble_sheet 4×2, grass backdrop); VillageSceneBuilder loads real art + sets import settings; prompt records in `art/prompts/`
- **DEPLOYED**: https://spb87.github.io/game-reebles-2d/ — public, HTTP 200, `.nojekyll` on gh-pages

## What's in progress

- Nothing — M1 + art pass boundary pause; M2 vertical-slice decomposition awaits go/no-go

## What's blocked

- Nothing

## Open questions

- Alpha/transparency approach for generated sprites — deferred to first M3 asset task (3-call experiment per scope)
- GitHub Pages propagation timing — first deploy may take minutes after `gh api .../pages` enable (TASK-12)
- TASK-12 will create public repo `spb87/game-reebles-2d` and push — flag for user awareness when reached

## Pipeline gotchas discovered

- `-runTests` + `-quit` on Unity 6000.3.24f1: `-quit` exits before tests run — omit it for test runs (recorded in conventions.md)
- `run-unity.bat` echoes `UNITY_EXIT=` but doesn't propagate it — callers parse output or invoke `Unity.exe` directly
- Kinematic `MovePosition` does NOT collide with static colliders — use `Rigidbody2D.Slide` (DEC-4 amended, REEB-135)
- `InputTestFixture.Press()` throws `ArgumentNullException` on Input System 1.14 — queue `KeyboardState` events instead
- PlayMode tests that `LoadSceneInPlayMode(Village)` must restore scene state — colliders at origin pin subsequently-spawned test players

## Current state (post-M2)

M2 vertical slice COMPLETE + deployed: world expanded 30x20→60x40 w/ treeline perimeter + outskirts props (REEB-144); Interactable/Interactor + prompt bubble (REEB-150); dialogue card + controller + movement suppression via Move-action disable (REEB-151); Marla the Baker NPC at bakery (REEB-152); quest schema in Resources/Quests + QuestService + CarrySlot + HUD objective/hearts/toast (REEB-153/154); E2E loop test + redeploy (REEB-155). PlayMode 30/30, EditMode 12/12. Live: https://spb87.github.io/game-reebles-2d/

## Test-infra notes

- PlayMode tests: NEVER reference `Assets/Input/ReeblesInput.inputactions` — under InputTestFixture the imported asset can report enabled with zero bound controls (passes isolated, fails in full suite). Use per-test synthetic InputActionAsset (convention in InteractionTests/DialogueTests/NpcVillagerTests/QuestLoopTests).
- `yield return null` in UnityTest may resume within the same Time.frameCount — use a NextFrame() helper that waits for the counter to advance (needed for LastClosedFrame guards).
- Tests loading Village.unity MUST restore scene state afterward.

## Architecture notes (M2)

- Quest JSON lives in `Assets/Resources/Quests/` (Resources.LoadAll) — non-Resources folders don't ship in WebGL. DEC-6 amended.
- asmdef dep graph: Player ← Interaction (CarrySlot, IHudView/HudViewLocator); Quests → Interaction+UI; UI → Interaction. Player has no UI/Quests deps; QuestService pushes state.
- Interactables found via OverlapCircleAll on existing solid colliders, filtered by GetComponentInParent<Interactable> — no separate trigger layer.
- DialogueController disables the Move action while open (no Player→UI dep).
- NpcInteractable has LastClosedFrame guard — needed when interactables trigger on the same Interact press that closes a card.
