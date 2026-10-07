# reeble

- Model: `recraft`
- Aspect ratio: 1:1 (default)
- REEB-142 style re-roll #2: previous sheet came back semi-realistic; this attempt
  asked for a single flat cartoon mascot sprite instead of a sheet.
- Post-processing: the returned image contained one large creature (770x740px,
  on-model) plus small stray fragments at the edges; the largest connected alpha
  component was extracted and cropped (803x772 with 16px padding) to produce the
  promoted `game/Assets/Art/Sprites/reeble.png`.

## Prompt

```
single small round fuzzy fantasy village creature, cute cartoon mascot, flat 2D game sprite, bold simple shapes, stylized painted look, front-facing, centered, transparent background, hand-painted cozy fantasy village, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text
```
