# Feature: M2 — Interaction system + dialogue card + first NPC

- **Milestone**: M2 — Core loop vertical slice
- **Jira**: REEB-145
- **Status**: active

## Goal

The player can walk up to a villager, see an interact prompt, press E (or tap the mobile interact button), and read a dialogue card. One NPC exists in the world for the vertical slice.

## Scope

In scope:
- `Interactable` component (interact radius, what it triggers) + player-side `Interactor` (nearest-in-range detection, fires `Interact` action)
- Interact prompt UI (small "E"/tap bubble over the target — procedural or generated sprite, hidden when nothing in range)
- Dialogue card UI per design brief: bottom-center card, name header, 1-3 lines, single continue affordance; pauses movement while open (or not — decide, keep simple)
- `NpcComponent` (display name + dialogue lines list); one villager sprite generated (recraft, painterly, alpha) + NPC placed at a post in the village
- Tests: interactable range detection, dialogue open/close, prompt visibility

Out of scope: dialogue trees/branching, multiple NPCs (M4), NPC wander AI, quest wiring (F3 hooks a quest onto the NPC there).

## Acceptance criteria

- E/tap near the NPC opens a dialogue card showing its name + lines; continue dismisses
- Prompt appears only in range; movement/dialogue interaction sane on desktop + mobile
- PlayMode tests green incl. new interaction tests

## Tasks

| Jira | Summary | Status | Depends on |
|------|---------|--------|------------|
| REEB-150 | Interactable + Interactor + prompt UI | ⏳ | F1 |
| REEB-151 | Dialogue card UI + DialogueController | ⏳ | interactable |
| REEB-152 | Villager sprite + NPC prefab in scene | ⏳ | dialogue |
