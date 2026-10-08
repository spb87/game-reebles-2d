# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-150: I1 — Interactable + Interactor + prompt UI

- **Feature**: REEB-145 (M2: Interaction + dialogue card + first NPC)
- **Role**: Developer
- **Files allowed**: `game/Assets/Scripts/Interaction/*` (new asmdef `Reebles2D.Interaction` following the Player/UI asmdef pattern + update test asmdef refs), `game/Assets/Editor/VillageSceneBuilder.cs`, `game/Assets/Tests/PlayMode/*.cs`
- **Input**: `Interactable` MonoBehaviour: `[SerializeField] interactRadius`, `[SerializeField] string promptVerb` ('Talk'/'Pick up'), UnityEvent or virtual `Interact()`. `Interactor` on the player prefab: finds nearest Interactable within its radius (Physics2D.OverlapCircle against a dedicated interactable trigger collider or tagged objects — pick the simpler), exposes CurrentTarget, fires `target.Interact()` when the `Interact` action triggers (same Input System action already bound to E/gamepad/mobile button). Prompt UI: floating indicator above the current target (sprite-based prompt — a small '!'/'E' bubble; sprite import or procedural texture, more reliable than TMP in WebGL). Builder: add Interactor + prompt wiring to player prefab; add a test interactable on the fountain (flavor line, promptVerb 'Admire').
- **Exit criteria**: builder exits 0; scene regenerated; PlayMode suite green incl. NEW tests (nearest-in-range detection; Interact action fires target).
- **Max new lines**: ~300
- **Status**: in progress
- **Git**: `feat(REEB-150): add interactable system + prompt UI`
