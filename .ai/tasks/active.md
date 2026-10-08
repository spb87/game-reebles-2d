# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-154: Q2 — CarrySlot + QuestService + HUD (objective/hearts/toast)

- **Feature**: REEB-146 (M2: One quest end-to-end — vertical slice)
- **Role**: Developer
- **Files allowed**: `game/Assets/Scripts/Interaction/CarrySlot.cs`, `game/Assets/Scripts/Quests/QuestService.cs` + `QuestNpcInteractable.cs`/`QuestItemInteractable.cs` (asmdef refs as needed — Quests may reference Interaction+UI), `game/Assets/Scripts/UI/Hud*.cs`, `game/Assets/Editor/VillageSceneBuilder.cs`, `game/Assets/Tests/PlayMode/*.cs`
- **Input**:
  1. `CarrySlot` on the player prefab: `string CurrentItemId` (null=empty), `bool TryPickup(string itemId)` (fails if occupied), `bool TryDeliver(string itemId)` (clears + returns true only if matching). One item max — scope's carry slot.
  2. `QuestService` (scene-generated singleton): `ActiveQuest` (QuestDef), `Hearts` int, `event Action`/`UnityEvent` for state changes the HUD listens to. `StartQuest(id)`, pickup gate (`CanPickup(targetId)` → quest active + fetchTargetId match + slot empty), `Deliver(npcId)` → hearts += reward, quest done.
  3. Interactable subclasses: `QuestItemInteractable : Interactable` (berry bush — Interact() → CarrySlot.TryPickup(quest.fetchItemId) when QuestService allows; fail path: brief log/toast 'hands full' or 'not now'); `QuestGiverInteractable : NpcInteractable` (Marla — dialogue lines switch by phase: not started→offerLines + StartQuest; active→activeLines; carrying item→completeLines + Deliver + hearts).
  4. HUD on the existing DialogueUI canvas or a sibling `Hud` canvas: objective text top-left ('Bring berries to Marla' while quest active, empty otherwise), hearts counter top-right ('♥ 0' text + optional heart glyph sprite — procedural ok), toast bottom-center-above-card that fades after ~2.5s ('+1 heart! Marla thanks you!'). uGUI Text (LegacyRuntime font), wire via builder.
  5. Builder: generate QuestService + CarrySlot on player + swap Marla's NpcInteractable→QuestGiverInteractable with npcId `marla_baker` + swap one berry bush's Interactable→QuestItemInteractable targetId `berry_bush` (leave the other 2 as flavor 'Pick berries' or make all 3 quest items — your call, simplest consistent).
- **Exit criteria**: builder exits 0; PlayMode green incl. NEW tests (pickup gates on quest+empty slot; deliver awards hearts + completes; HUD text reflects state); EditMode still green.
- **Max new lines**: ~400
- **Status**: in progress
- **Git**: `feat(REEB-154): add carry slot + hearts HUD + quest service`
