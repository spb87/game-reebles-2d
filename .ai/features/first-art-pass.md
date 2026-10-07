# Feature: First generated art pass — replace greybox

- **Milestone**: unscheduled pull-forward (M3 subset, user-requested after M1 review)
- **Jira**: REEB-139
- **Status**: active

## Goal

Replace every procedural flat-color sprite in `Village.unity` with RouteLLM-generated painterly art matching the style anchor, then redeploy so the live URL shows real art instead of greybox. Settle the alpha/transparency approach (deferred experiment) as part of this pass.

## Why now

M1 shipped and is live but greybox reads as "terrible" on first view. The art pipeline was already scaffolded for exactly this; pulling the first asset batch forward de-risks M3's core question (can the pipeline produce coherent, usable sprites with alpha) before M2 quest work builds scenes on top.

## Scope

In scope:
- Alpha/transparency experiment (the deferred 3-call test): `recraft` direct-alpha vs flat-background + chroma-key vs alternate model — pick the winner, document in `art/README.md`
- Asset batch (~8-10 generations): tileable ground texture, 5 building sprites (bakery/smithy/herbalist/store/inn — 3/4 oblique front view, alpha), fountain (alpha), fence segment (tileable horizontally, alpha), player Reeble (small cute creature — sprite sheet 4-dir × 2-frame if it comes out clean, else single facing sprite), optional small props sheet
- Prompt records committed per asset (`art/prompts/<asset>.md`: model + full prompt incl. anchor)
- `VillageSceneBuilder` loads promoted sprites instead of procedural Texture2D; importer sets sprite settings (Sprite mode, alpha, PPU, bilinear)
- Colliders match the **footprint** of buildings (base/walkable edge), not the full sprite incl. roof overhang
- Rebuild WebGL + redeploy via `tools/deploy-pages.py` → live URL shows the art pass

Out of scope: NPC sprites (M3/M4), walk animation polish, props placement, interiors, any prompt-heroics beyond one or two re-rolls per asset.

## Acceptance criteria

- Every greybox sprite replaced by a generated asset with correct alpha
- Style coherence: all assets visibly share the anchor's palette/hand-painted look
- Colliders still block correctly (PlayMode suite stays green; wall test intact)
- Live URL serves the art-pass build
- `art/README.md` records the settled alpha approach; `art/prompts/` has a record per promoted asset

## Tasks

| Jira | Summary | Status | Depends on |
|------|---------|--------|------------|
| REEB-140 | Art pass 1A: alpha experiment + first asset batch | ⏳ | — |
| REEB-141 | Art pass 1B: integrate art into scene builder + rebuild + redeploy | ⏳ | REEB-140 |
