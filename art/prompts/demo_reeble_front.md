# demo_reeble_front (living-painting spike, REEB-158)

- Model: `recraft` — returned opaque RGB WebP (no alpha; known nondeterminism)
- Post-processing: near-white background flood-filled from border seeds
  (PIL `ImageDraw.floodfill`, thresh 35), soft 2px alpha edge, crop to bbox.
  Promoted to `demo/living-painting/assets/reeble_front.png`.

## Prompt

```
flat 2D game sprite, hand-painted watercolor children's book illustration style, NOT photorealistic, NOT 3D render, NOT plush toy, NOT fuzzy texture, small round cream-colored fantasy creature like a chubby hamster blob with tiny feet, big dark eyes, gentle smile, front view, full body, centered, isolated on transparent background, alpha channel PNG, hand-painted cozy fantasy village, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text
```
