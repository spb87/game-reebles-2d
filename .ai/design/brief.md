# Design brief

Visual guidance for frontend-facing tasks. Frontend features require this file to be filled before decomposition — exit criteria referencing visuals need a palette and component direction to be verifiable.

## Art direction (v1)

- **Look**: warm painterly cozy — Fantasy Life i / Lil Gator adjacent. All art is AI-generated (RouteLLM); consistency comes from the style anchor + asset-sheet prompts, not per-asset heroics.
- **Style anchor** (verbatim fragment appended to every generation — lives in `art/style-anchor.txt`, DEC-9):
  `hand-painted cozy fantasy village, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text`
- **Palette direction**: warm golds/creams, muted greens, terracotta/brown accents. Exact hex values are owned by the generated backdrop (M3) — do not invent a competing palette for HUD chrome.
- **HUD chrome (v1)**: minimal — dark translucent rounded panel, cream text, one accent (heart red for the counter). Font: uGUI default for v1; a generated/hand-lettered font is v1.1+.

## Greybox (M1 only)

- Placeholder sprites: flat colored rounded rects — player = warm orange, buildings = muted terracotta variants, fountain = blue, fence = dark brown, ground = muted green. Greybox visuals carry zero art authority.

## Layout rules

- HUD: screen-space overlay canvas; objective line top-left, hearts counter top-right, toast bottom-center. Anchored corners, safe-area aware for mobile.
- Dialogue card (M2): bottom-center card, name header + 2-3 lines, single "continue" affordance.
- Mobile controls: joystick bottom-left, interact button bottom-right; hidden on non-touch devices.

## References

- `../game-reebles/.ai/design/references/` contains Fantasy Life i captures from the 3D project — palette/mood reference only (different renderer, same target feel).
- `reebles-2d-project-scope.md` §Art direction is the authority.
