# Architectural decisions

Decisions made by the Architect agent. Every agent must read this file before starting work to avoid re-litigating settled questions.

---

### DEC-1: Unity 6000.3.24f1 as the engine pin

- **Date**: 2026-10-07
- **Context**: Scope requires "Unity 6 LTS (6000.3.24f1 — same install as the 3D project)". The editor is already installed on Windows with the WebGL module.
- **Decision**: Pin **Unity 6000.3.24f1** (changeset `4e7b9b5b6244`) at `C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe`. The `6000.6.3f1` editor also installed on the machine is NOT to be used.
- **Alternatives considered**: 6000.6.x tech stream (newer, shorter support); Linux editor in WSL (rejected in the 3D project — unsupported GUI, `/mnt/c` 9P filesystem cripples `Library/` I/O).
- **Consequences**: `game/ProjectSettings/ProjectVersion.txt` locks the version; upgrades require a new DEC. C# 9 max — no file-scoped namespaces.

### DEC-2: Repo layout — Unity project in `game/`, art pipeline in `art/`

- **Date**: 2026-10-07
- **Context**: Repo root already holds `.ai/`, `.devin/`, and the scope doc. Same constraint the 3D project solved.
- **Decision**: Unity project lives in `game/` (Assets/, Packages/, ProjectSettings/ under it). Art pipeline lives in `art/`: `art/tools/` (scripts), `art/prompts/` (committed prompt records), `art/staging/` (gitignored generation outputs). Promoted, game-ready sprites are committed under `game/Assets/Art/`. Repo helpers in `tools/`.
- **Alternatives considered**: Unity project at repo root — rejected (keeps root clean, room for non-Unity tooling, matches proven 3D layout).
- **Consequences**: Task "Files allowed" paths are prefixed `game/`, `art/`, or `tools/`; `.gitignore` covers `game/Library/` etc. and `art/staging/`.

### DEC-3: Hosting — GitHub Pages (public repo)

- **Date**: 2026-10-07
- **Context**: Scope requires "hosted publicly (GitHub Pages or itch.io — decide in the first task, don't defer)". `gh.exe` on the Windows side is authenticated as `spb87` with `repo` scope.
- **Decision**: **GitHub Pages**. Repo `spb87/game-reebles-2d` will be **public** (Pages is free on public repos; HTTPS is a PWA requirement). Builds are produced locally by the pinned editor and pushed to a `gh-pages` branch by a deploy script — no CI Unity builds (license risk).
- **Alternatives considered**: itch.io — remains a valid *additional* distribution channel later (v1.1+), but uploads are manual/butler-driven while Pages deploys are a git push. Vercel/Netlify — extra accounts for zero gain over Pages.
- **Consequences**: Public repo means no secrets may ever be committed — `.env` stays gitignored (enforced by task exit criteria). A `.nojekyll` file ships with the Pages site.

### DEC-4: 2D movement — kinematic Rigidbody2D + MovePosition

- **Date**: 2026-10-07
- **Context**: Top-down 2D needs collision that matches painted geometry "with no physics surprises" (scope). The 3D project's `CharacterController` choice doesn't apply — it's a 3D component.
- **Decision**: Player and NPCs use kinematic `Rigidbody2D` + `MovePosition` in `FixedUpdate`. Blocking geometry gets `PolygonCollider2D`/`BoxCollider2D`; pass-through props get none. No dynamic physics (no force-pushed objects, no mass).
- **Alternatives considered**: `transform.Translate` + manual overlap checks — reinvents collision; dynamic Rigidbody2D — physics jitter against traced colliders is exactly the "physics surprise" the scope warns about.
- **Consequences**: Deterministic movement, easy PlayMode tests. Tunables (walk/run speed, interact radius) stay `[SerializeField]`.

### DEC-5: Sprite animation minimal — 4-direction, 2-4 frame swaps

- **Date**: 2026-10-07
- **Context**: Scope permits "2-4 frame walk swap + idle bob"; skeletal rigs only if a parts-sheet lands well — "do not bet the schedule on it."
- **Decision**: 4-directional sprite sets with frame-swap animation driven by a simple `SpriteAnimator` (or Animator with sprite states). No Unity 2D skeletal rigging in v1.
- **Alternatives considered**: 8-direction turnaround — doubles art cost for marginal gain; skeletal rig — generation-fragile.
- **Consequences**: Art prompts target 4-direction sheets. If a generated turnaround lands poorly, fall back to a single facing + flip.

### DEC-6: Quest data as JSON text assets

- **Date**: 2026-10-07
- **Context**: Scope requires "5 quests as data (ScriptableObjects or JSON) — adding a quest must not touch code."
- **Decision**: **JSON** files under `game/Assets/Data/Quests/` (imported as TextAssets), loaded by a `QuestLibrary` at startup.
- **Alternatives considered**: ScriptableObjects — Unity-idiomatic but `.asset` YAML is fragile to hand-edit, hostile to diffs, and EditMode tests for data validation are cleaner against JSON.
- **Consequences**: Quest authoring is "drop in a JSON file" — validated by an EditMode test that parses every file in the folder and checks required fields (npc, item, location, dialogue lines, reward).

### DEC-7: Reward currency — village hearts

- **Date**: 2026-10-07
- **Context**: Scope says "village hearts / gold — pick one, expose tuning."
- **Decision**: **Hearts**. Fits the cozy fantasy (thanks, not commerce); counter labelled "Hearts" on the HUD; reward amount per quest is a JSON field.
- **Alternatives considered**: Gold — implies economy, which is an explicit non-goal.
- **Consequences**: `RewardCounter`/`Hearts` naming in code and HUD.

### DEC-8: No engine MCP servers in v1 — batch mode is the verification path

- **Date**: 2026-10-07
- **Context**: The 3D project planned `unity-mcp`/`blender-mcp` for editor verification; neither was ever installed. 2D has no Blender at all.
- **Decision**: Verification = Unity Test Framework in batch mode (`-runTests`) + scripted scene builders (`-executeMethod`) + local Web serving for browser checks. No MCP servers are project dependencies.
- **Alternatives considered**: Install unity-mcp for play-mode screenshots — adds a Windows↔WSL socket dependency for marginal gain; revisit only if batch-mode verification proves insufficient.
- **Consequences**: Tasks verify via `Unity.exe -batchmode` commands and file artifacts. User-facing visual verification happens on the served/public build.

### DEC-9: Style anchor recorded as the single art-direction constant

- **Date**: 2026-10-07
- **Context**: Scope requires one fixed prompt fragment appended verbatim to every generation.
- **Decision**: The style anchor lives in `art/style-anchor.txt` (committed) and `art/README.md` documents the pipeline (anchor + per-asset prompt → `gen_image.py` → `art/staging/` → promote to `game/Assets/Art/` + prompt record in `art/prompts/`).
- **Consequences**: Changing the anchor restarts art coherence — treat as a DEC-level change.
