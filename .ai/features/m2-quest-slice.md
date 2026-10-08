# Feature: M2 — One quest end-to-end (the vertical slice)

- **Milestone**: M2 — Core loop vertical slice
- **Jira**: REEB-146
- **Status**: active

## Goal

Talk to the baker → dialogue assigns the errand → fetch the marked item (berry bush in the outskirts) → deliver → hearts counter ticks. If this isn't fun, nothing else matters (scope).

## Scope

In scope:
- Quest JSON schema + `QuestLibrary` loading `TextAsset`s from `Assets/Data/Quests/` (DEC-6) + EditMode schema tests
- One quest: `errand_berries.json` — giver NPC, fetch item, deliver, reward hearts
- Carry slot (one item — scope): pickup via interact at the berry bush, carry indicator, deliver via interact at the quest NPC
- HUD per design brief: objective line top-left, hearts counter top-right, toast bottom-center
- Berry bush becomes a quest interactable in the outskirts (uses F1 bush art)
- End-to-end PlayMode test of the loop + rebuild + redeploy

Out of scope: multiple quests (M4), quest chains, dialogue trees, economy, persistence/save.

## Acceptance criteria

- The loop plays end-to-end in the built game: prompt → card → marker → carry → deliver → heart + toast
- Quest is pure data (adding another quest = new JSON, no code — validated by EditMode test)
- Hearts counter visible and increments; toast fires on reward
- Live URL updated

## Tasks

| Jira | Summary | Status | Depends on |
|------|---------|--------|------------|
| REEB-153 | Quest schema + QuestLibrary + errand JSON | ⏳ | F2 |
| REEB-154 | Carry slot + pickup/deliver + hearts HUD | ⏳ | schema |
| REEB-155 | E2E test + scene wiring + rebuild + redeploy | ⏳ | carry |
