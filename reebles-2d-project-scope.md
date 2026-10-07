# Reebles 2D — Cozy Village Errands — MVP Scope v1

**Status:** Draft for architect handoff — new repo, new project
**Pipeline:** Unity 6 (2D/URP, Web platform / PWA) + RouteLLM image generation for all art
**Origin:** Pivot from the 3D Reebles project (`game-reebles`, archived on branch). Carries the village fiction forward but deliberately rebuilds thin.

## Elevator pitch

A cozy top-down 2D errand game for the browser: you are a **Reeble**, the village's helpful delivery sprite — talk to villagers, fetch what they need, bring it back, earn their thanks. The entire art set is AI-generated painterly 2D. The point of v1 is a complete, playable game loop on a public URL, not a tech demo.

## Design pillars

1. **Ship and learn beats polish and guess.** A live URL in week one; iteration driven by players, not critic passes. Hosting is a v1 requirement, not an open question.
2. **AI-native art.** Generated images are the art direction, not a placeholder — painterly style hides generation seams. Consistency comes from asset-sheet prompts (grid layouts sliced to sprites) and a fixed style anchor, not from fighting the generator for precision.
3. **One core loop, finished.** Talk → fetch → deliver → reward. Five quests, four NPCs, one map. Nothing else earns its place in v1.
4. **Small and finished beats big and broken.** (Unchanged from the 3D project — it was right then too.)

## Target platform & tech stack

- **Engine:** Unity 6 LTS (6000.3.24f1 — same install as the 3D project), URP **2D Renderer**
- **Distribution:** Unity Web build + PWA template → desktop + mobile browsers; **hosted publicly** (GitHub Pages or itch.io — decide in the first task, don't defer)
- **Art pipeline:** Abacus.AI RouteLLM API (`https://routellm.abacus.ai/v1`) — `gen_image.py` helper script (port from `game-reebles/art/tools/`, stdlib-only, reads `ROUTELLM_API_KEY` from repo `.env`, gitignored). Models: `seedream` default, `nano_banana_pro`/`gpt_image25` for hero shots, `*_edit` variants for iteration.
- **Input:** Keyboard/mouse desktop; touch joystick + interact button mobile (reuse the proven MobileControls pattern conceptually — same Input System package)
- **No Blender required.** The heaviest dependency from the 3D project is gone; sprites are the asset.

## Art direction

- **Look:** warm painterly cozy — Fantasy Life i / Lil Gator adjacent. Generated art carries its own palette; no vertex-color pipeline.
- **Style anchor:** one fixed prompt fragment reused verbatim across all generations (e.g. "hand-painted cozy fantasy village, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text"). Record it in the repo; every generation call appends it.
- **Asset sheets:** generate grids ("16 village props, 4x4 sheet, flat style, consistent scale") and slice to sprites — one call buys style coherence across a set.
- **Alpha:** prefer models with transparent-background output (`recraft`, or `gpt_image25` transparency if supported); fallback is chroma-key on a flat background color in the pipeline script. Decide during the first asset task with a 3-call experiment.

## Player character: the Reeble (2D)

- Top-down/slight-oblique sprite — generated once in a turnaround pose set (4 or 8 directions) or kept to 4-directional to minimize frame count
- Walk, run (shift), interact (E / tap). Animation minimal by design: 2-4 frame walk swap + idle bob is acceptable; Unity 2D skeletal rig only if a generated parts-sheet lands well — do not bet the schedule on it
- No customization

## Core loop & systems (v1)

- **Talk:** walk to an NPC, prompt appears, interact opens a 2-3 line dialogue card (name, want, thanks)
- **Fetch:** quest item spawns at a marked spot (or was always there — simpler); pickup adds it to a one-slot carry ("you're carrying: bread")
- **Deliver:** return to the NPC, dialogue card completes, reward ticks a counter (village hearts / gold — pick one, expose tuning)
- **Quest data:** 5 quests as data (ScriptableObjects or JSON) — NPC, item, location, line of dialogue. Adding a quest must not touch code
- **HUD:** current objective line + reward counter + quest-complete toast. Nothing else
- **NPCs:** 4 villagers with fixed posts + idle wander in a small radius; distinct generated portraits/sprites

## World: the village

- One painted map, larger than one screen (camera follows, ~2-3 screens of scroll): painterly backdrop image(s) at 2048px+ under a collider layer — simple polygon colliders traced on buildings/props, no physics surprises
- ~5 buildings with door/interact points (bakery, smithy, herbalist, general store, inn); plaza with fountain as orientation anchor
- Prop dressing: generated sheet — barrels, crates, lamps, flower boxes, stalls — scattered with collider-free pass-through vs blocking flag
- The fence principle still holds: the world edge is a visible boundary, not an invisible wall

## Performance budgets (Web platform)

- Initial download ≤ 50 MB compressed — expect ≤ 15 MB at this scope; PNG budgets: backdrop ≤ 3 MB each, sprite sheets ≤ 1 MB each
- 60 fps desktop browsers / ≥30 fps iOS Safari 15+ — trivially achievable in 2D, keep it that way (no per-frame allocations, sprite atlasing via Unity's packer)
- Brotli compression; load to interactive ≤ 5 s on broadband

## Explicit non-goals (v1)

- No 3D, no Blender, no FBX anywhere
- No save system, accounts, multiplayer, leaderboards
- No interiors — NPCs stand at their posts outdoors
- No day/night cycle, weather, economy, inventory UI beyond the one carry slot
- No audio in v1 (v1.1 candidate — generated music is a natural fit later)
- No quest chains/branching, no dialogue trees — fixed two-card conversations

## Roadmap

- **v1 (this scope):** playable errand loop on a public URL
- **v1.1:** audio pass (generated music/ambience), more quests, quest-giver map icons
- **v2:** whatever players actually ask for — that is the point of shipping v1

## Acceptance criteria

*Architect: verify via play-mode tests + a live URL the user can open on a phone.*

- [ ] Web build runs in browser at a **public URL** and installs as a PWA
- [ ] Reeble moves (walk/run/interact) on desktop and mobile
- [ ] One full quest loop works end-to-end: talk → fetch → deliver → reward counter
- [ ] ≥5 quests, ≥4 distinct NPCs, village is visually coherent (generated art, consistent style anchor)
- [ ] World edge is a visible boundary; colliders match the painted geometry
- [ ] Build ≤ 50 MB compressed; 60 fps desktop / 30 fps mobile

## Handoff notes for the architect

- **Build order:** greybox scene + player movement → ONE quest end-to-end (the vertical slice — if this isn't fun, nothing else matters) → asset-generation pipeline + style anchor → full village art pass → remaining quests + NPCs → PWA + public hosting → polish within scope only
- **Copy these into the new repo:** `gen_image.py` (stdlib RouteLLM helper — verified working), the `.ai/` framework skeleton (agent-mode, roles, conventions — update paths), `.env` with `ROUTELLM_API_KEY` (**gitignored, never committed**)
- **Keep the inspector-tunable rule:** movement speeds, interaction radius, camera settings — all `[SerializeField]`; game feel is where AI iteration is weakest, design for fast tuning loops
- **Generated art is not sacred.** If a generated asset reads wrong in-scene, regenerate with a new prompt — that's the whole advantage of this pipeline. Budget ~50 generations for v1 (~pennies).
- **Scope discipline is the whole bet.** The 3D project died of polish-first sequencing, not technical failure. Any feature request that isn't the core loop or hosting goes to v1.1+, no exceptions — including better art.
