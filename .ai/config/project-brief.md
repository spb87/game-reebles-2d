# Project brief

High-level description of the project. Every agent reads this file first.

**Source of truth for scope**: `reebles-2d-project-scope.md` at repo root (MVP Scope v1). This brief is the summary; defer to that document on conflicts.

## Project name

Reebles 2D — Cozy Village Errands

## One-line description

A cozy top-down 2D errand game for the browser: you are a Reeble, the village's helpful delivery sprite — talk to villagers, fetch what they need, bring it back, earn their thanks.

## Target users

Casual players on desktop and mobile browsers; also a testbed for an AI-native art pipeline (all art generated via RouteLLM image models).

## Key requirements

1. One complete core loop, finished: talk → fetch → deliver → reward. ≥5 quests, ≥4 NPCs, one village map (~2-3 screens of scroll).
2. Playable on a **public URL** as a PWA — shipping and iterating with players is a design pillar, not a stretch goal.
3. All art AI-generated painterly 2D via a fixed style anchor + asset-sheet prompts sliced into sprites.
4. Web platform budget: ≤50 MB compressed (expect ≤15 MB), 60 fps desktop / ≥30 fps iOS Safari 15+, Brotli, interactive ≤5 s on broadband.
5. Fast tuning loops: movement speeds, interaction radius, camera settings all `[SerializeField]` — inspector-tunable.

## Non-goals

- No 3D, no Blender, no FBX anywhere (pivot from the archived 3D `game-reebles` project)
- No save system, accounts, multiplayer, leaderboards
- No interiors, day/night, weather, economy, or inventory UI beyond one carry slot
- No audio in v1 (v1.1 candidate)
- No quest chains/branching or dialogue trees — fixed two-card conversations
- No feature that isn't the core loop or hosting — scope discipline is the whole bet

## Timeline

- **v1**: playable errand loop on a public URL (this scope). Live URL within week one per design pillar 1.
- **v1.1**: audio pass, more quests, quest-giver map icons.
- **v2**: whatever players ask for.

## Origin

Pivot from the 3D Reebles project (`../game-reebles`, archived on branch). Carries the village fiction forward but rebuilds thin. Reusable artifacts ported over: `art/tools/gen_image.py` (RouteLLM helper), the `.ai/` framework, `.env` with `ROUTELLM_API_KEY`.
