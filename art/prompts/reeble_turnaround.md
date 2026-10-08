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
- Post-deploy review (REEB-143 follow-up): the TL cell saved as
  `reeble_right.png` turned out to be another FRONT view, not a right
  profile — dark-feature centroid ~0.54 (face centered), vs `reeble_left.png`
  centroid ~0.24 (true left profile). The right cell was discarded from use;
  `reeble_right.png` remains on disk but is no longer wired into the player
  prefab. Right-facing is rendered as the left sprite with `flipX = true`
  (PlayerFacing's missing-side-sprite fallback).

## Prompt

```
flat 2D game sprite character turnaround sheet, small round fuzzy cream-colored fantasy creature like a hamster blob with tiny feet, same creature shown in a 2x2 grid: front view, back view, left side view, right side view, consistent design, evenly spaced grid cells, stylized cartoon, NOT photorealistic, hand-painted watercolor style, transparent background, alpha channel, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text
```
