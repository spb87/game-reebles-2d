# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-152: I3 — Villager NPC placed in scene + wired to dialogue

- **Feature**: REEB-145 (M2: Interaction + dialogue card + first NPC)
- **Role**: Developer
- **Files allowed**: `game/Assets/Editor/VillageSceneBuilder.cs`, `game/Assets/Prefabs/Npc.prefab` (new generated), generated `Village.unity`, `game/Assets/Tests/PlayMode/*.cs`
- **Input**: Extend the builder: generate `Npc.prefab` — `npc_villager.png` sprite (EnsureSpriteImport if needed) + CircleCollider2D footprint blocker + `NpcComponent` ('Marla the Baker' + 2-3 cozy lines) + `NpcInteractable` (promptVerb 'Talk', radius ~1.5-2) + shared shadow child at feet. Place ONE instance in `Village.unity` near the bakery door facing the plaza. NPC is static (wander = M4). PlayMode test: player in range → prompt; Interact → card opens with 'Marla' name + line; advance → closes. Also verify the scene YAML references the prefab instance.
- **Exit criteria**: scene has the NPC at the bakery wired to dialogue; PlayMode green incl. talk test; builder exits 0.
- **Max new lines**: ~150
- **Status**: in progress
- **Git**: `feat(REEB-152): add villager NPC wired to dialogue`
