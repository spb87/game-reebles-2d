# Feature: Visual spike — living-painting web demo

- **Milestone**: unscheduled (pre-M3 direction-setting, user-requested after M2 visual review)
- **Jira**: REEB-157 (story) → REEB-158 (build subtask)
- **Status**: active

## Goal

Determine whether a "living painting" presentation — AI-generated full-scene painterly art animated by code (parallax, light, particles) — reaches the visual quality bar before deciding whether the project stays in Unity or moves web-native.

## Why now

Post-M2 user review: visuals are primitive — flat, no shadows, sprites read as decals on a backdrop. Research showed the reference-quality AI game demos (Claude Fable / GPT-6 Astra + Higgsfield-class asset pipelines) run browser-native stacks where the agent can screenshot and iterate on its own renders — a loop Unity batch mode lacks. User's redirected priority: **beautiful first, fit the game to it**. Three candidate archetypes identified; this spike tests the cheapest one (living painting) as a web demo that also doubles as a web-native proof-of-concept.

## Acceptance criteria

1. Single-page web demo renders a painterly village-square scene generated via the existing RouteLLM art pipeline.
2. ≥3 independent motion layers/effects (depth parallax, drifting fog, lantern flicker / god rays, ambient particles).
3. A reeble walks the scene under keyboard control with sprite-sheet animation and a soft grounding shadow; scale conveys depth.
4. Orchestrator can view/iterate on the render via browser preview tooling (closes the blind-agent gap).
5. Result presented to user for a beauty verdict — go/no-go on the visual direction.

## Known constraints

- Throwaway spike — no Unity dependency; code need not port into `game/`.
- Art via existing pipeline (`art/tools/gen_image.py`, style anchor verbatim); prompts recorded in `art/prompts/`.
- Self-contained static files, servable by `python3 -m http.server`.
- Executed in-session by the orchestrator (Subagents fallback clause) — the visual see→edit loop wants one context.

## Out of scope

- Gameplay systems (quests, dialogue, NPCs, collision) beyond a walkable character.
- The engine decision itself — this spike informs it.
- Mobile/touch controls, persistence, audio.

## Tasks

- REEB-158: Build living-painting web demo

## Status

- [x] Spec complete
- [x] Tasks decomposed
- [x] Cost estimated
- [ ] All tasks done
