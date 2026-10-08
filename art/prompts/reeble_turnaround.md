# reeble_turnaround

- Model: `recraft`
- Aspect ratio: 1:1 (default)
- REEB-143: 4-view turnaround for directional facing. gen_image.py has no
  edit/input-image flag, so per-view edits of `reeble.png` were not possible;
  a single 2x2 turnaround sheet was generated instead.
- Post-processing: alpha-component analysis found 4 large components in a 2x2
  arrangement (cells NOT evenly split at midline; stray text/fragment blobs at
  edges were discarded). View identification heuristics: TR = back (highest
  alpha symmetry, ~zero dark face-feature pixels), BL = front (symmetric,
  heavy dark features top and bottom), BR = left-facing (dark centroid far
  left of body centroid), TL = right-facing (dark centroid right of center).
  Each cell was cropped to its alpha bbox + 8px padding and saved as
  `reeble_{front,back,left,right}.png` in `game/Assets/Art/Sprites/`.

## Prompt

```
flat 2D game sprite character turnaround sheet, small round fuzzy cream-colored fantasy creature like a hamster blob with tiny feet, same creature shown in a 2x2 grid: front view, back view, left side view, right side view, consistent design, evenly spaced grid cells, stylized cartoon, NOT photorealistic, hand-painted watercolor style, transparent background, alpha channel, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text
```
