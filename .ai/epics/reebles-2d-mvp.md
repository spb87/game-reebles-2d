# Epic: Reebles 2D — Cozy Village Errands (v1)

**Jira Issue**: [REEB-123](https://stan-butler.atlassian.net/browse/REEB-123) (Epic)

An epic is a project-level goal that is too large to be a single feature. It gets decomposed into milestones, and each milestone gets decomposed into features.

**Scope source of truth**: `reebles-2d-project-scope.md` (MVP Scope v1).

## Vision

A cozy top-down 2D errand game running on a public URL in any browser: the player is a Reeble, the village's delivery sprite, wandering a warm painterly village — talking to villagers, fetching items, delivering them, and collecting hearts. All art is AI-generated; the game is small, finished, and shipped.

## Milestones

### Milestone 1: Walking skeleton

- **Goal**: A public URL serves a Web build where a greybox Reeble walks/runs around a greybox village on desktop and mobile, with camera follow and collision against placeholder geometry.
- **Proves**: Unity-on-WSL batch pipeline, URP 2D setup, Input System desktop+mobile, Web build → GitHub Pages deploy path. Hosting is proven before any art is spent (scope pillar 1: a live URL early, not deferred).
- **Features**: `m1-repo-toolchain-scaffold.md`, `m1-greybox-scene-movement.md`, `m1-web-smoke-deploy.md`
- **Estimated cost**: see `.ai/costs/estimates.md`
- **Status**: in progress

### Milestone 2: Core loop vertical slice

- **Goal**: ONE quest works end-to-end — talk to NPC → dialogue card → fetch marked item → deliver → hearts counter ticks. HUD shows objective + counter + toast.
- **Proves**: The loop is actually playable; quest-data-as-JSON authoring works (DEC-6).
- **Features**: TBD — decomposed after M1 go/no-go
- **Status**: not started

### Milestone 3: Art pipeline + village art pass

- **Goal**: Generated painterly art replaces all greybox: style anchor locked (DEC-9), backdrop map, Reeble 4-direction sprite set, 4 NPC sprites, prop sheet sliced and placed.
- **Proves**: RouteLLM pipeline produces coherent in-scene art; alpha/transparency approach settled (3-call experiment per scope).
- **Features**: TBD
- **Status**: not started

### Milestone 4: Content complete

- **Goal**: ≥5 quests, ≥4 NPCs with posts + idle wander, colliders match painted geometry, visible world-edge boundary, all quests playable end-to-end.
- **Proves**: Full acceptance criteria minus polish.
- **Features**: TBD
- **Status**: not started

### Milestone 5: Ship v1

- **Goal**: PWA installable, performance budgets verified (≤50 MB, 60/30 fps, ≤5 s load), public URL final, acceptance checklist passed.
- **Proves**: v1 is done; v1.1+ backlog starts from player feedback.
- **Features**: TBD
- **Status**: not started

## Scope boundaries

- **In scope for this epic**: the errand loop, one village map, 5 quests, 4 NPCs, generated 2D art, Web/PWA hosting, desktop + mobile input.
- **Out of scope**: 3D/Blender/FBX, saves, accounts, multiplayer, interiors, day/night, weather, economy, inventory UI beyond one carry slot, audio (v1), quest branching/dialogue trees.
- **Deferred**: audio pass, more quests, map icons (v1.1); itch.io as an additional channel; engine MCP servers.

## Technical risks

1. **Generated-art coherence fails**: painterly style + fixed anchor + asset sheets mitigates; if a set reads wrong, regenerate (~50 generation budget, pennies). Mitigation: style anchor decided in M3 before mass generation; alpha approach settled by a 3-call experiment.
2. **GitHub Pages can't serve Unity Brotli with correct headers**: Pages does not emit `Content-Encoding: br`. Mitigation: M1 smoke build uses `Compression Format: Disabled` (or decompression fallback) — size budget has huge headroom; compression revisit is a v1 polish task if needed.
3. **Unity batch invocation from WSL is brittle**: proven in the 3D project (full-path `Unity.exe`, Windows-style args, `-batchmode -quit`, check exit code + log tail). Mitigation: `tools/run-unity.bat` shim + every Unity task's exit criteria checks the log for compile errors.
4. **Scope creep kills the project** (the 3D project's actual cause of death — polish-first sequencing): any feature request outside the core loop or hosting goes to v1.1+, no exceptions — including better art.

## Go / No-go checkpoint (M1 → M2)

Before decomposing M2, review:

- Does the public URL serve a build where the Reeble moves on desktop AND mobile?
- Are all M1 tasks in `done.md` with green exit criteria?
- Is cost tracking within estimate?
- Any recurring drift patterns in `bugs/patterns.md` to adjust for?
- Does the generated-scene convention hold (no hand-edited YAML)?
