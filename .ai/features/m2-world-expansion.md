# Feature: M2 — World expansion (outskirts beyond the village)

- **Milestone**: M2 — Core loop vertical slice (user feedback: "reeble can explore more than just the initial village")
- **Jira**: REEB-144
- **Status**: active

## Goal

The map grows from a 30×20 fenced village (~2-3 screens) to a ~60×40 world with a village core surrounded by explorable outskirts. Terrain backdrop stays a single generated image; the world edge is a natural **treeline** (tree sprites + collider strip) instead of picket fence everywhere.

## Scope

In scope:
- Regenerated terrain backdrop sized for the larger map (empty terrain: grass + dirt paths radiating from the village plaza outward to the edges; NO buildings/objects/characters/text)
- Prop art batch (recraft, alpha): 3 tree variants, bush, berry bush (quest-usable later), flower patch (walk-through), rock
- `VillageSceneBuilder`: map bounds → ~60×40; village core keeps current layout at center; treeline perimeter (repeating tree sprites + continuous collider strips); scattered props with colliders (trees/rocks) + pass-through flowers; camera confiner to new bounds
- Pickets only around the village plaza core IF it reads well, else drop fence entirely (treeline is the edge)
- Rebuild + PlayMode green + WebGL rebuild + redeploy

Out of scope: interiors, additional named locations, NPCs (F2), quest items logic (F3), streaming/chunking.

## Acceptance criteria

- Player can walk well beyond the village core in all directions; camera follows to map bounds
- World edge is a visible treeline that blocks movement (PlayMode collision test still green)
- Village core layout/buildings/fountain unchanged; backdrop has no baked-in objects
- Live URL serves the expanded world

## Tasks

| Jira | Summary | Status | Depends on |
|------|---------|--------|------------|
| REEB-147 | Art batch 2: terrain backdrop + prop set | ⏳ | — |
| REEB-148 | Builder: expanded map + treeline + prop scatter | ⏳ | art batch |
| REEB-149 | Verify + rebuild + redeploy | ⏳ | builder |
