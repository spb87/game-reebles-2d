# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-151: I2 — Dialogue card UI + DialogueController

- **Feature**: REEB-145 (M2: Interaction + dialogue card + first NPC)
- **Role**: Developer
- **Files allowed**: `game/Assets/Scripts/UI/*` (DialogueCard.cs, DialogueController.cs in Reebles2D.UI asmdef — may need asmdef ref to Reebles2D.Interaction), `game/Assets/Editor/VillageSceneBuilder.cs`, `game/Assets/Tests/PlayMode/*.cs`
- **Input**: Dialogue card per design brief: bottom-center uGUI panel on a screen-space-overlay canvas (dark translucent rounded rect — procedural sprite like the PromptBubble routine), name header text + body text + continue affordance. `DialogueCard` view: Show(name, line), Hide; `DialogueController`: OpenDialogue(name, string[] lines), advance on Interact action / pointer click, close at last line. While open, suppress player movement (simplest: `DialogueController.IsOpen` flag that PlayerMovement respects, or disable the Move action map — pick the cleaner). `NpcComponent` MonoBehaviour: displayName + string[] lines; NpcInteractable : Interactable whose Interact() opens the dialogue. Fountain 'Admire' shows a one-line card (name 'Fountain'). Fonts: uGUI built-in LegacyRuntime font is fine for v1 (no TMP). Builder: generate canvas + card (starts hidden) + wire into scene.
- **Exit criteria**: builder exits 0; scene regenerated; PlayMode green incl. NEW tests (open shows name+line; advance cycles lines; close at end; movement suppressed while open).
- **Max new lines**: ~350
- **Status**: in progress
- **Git**: `feat(REEB-151): add dialogue card UI + controller`
