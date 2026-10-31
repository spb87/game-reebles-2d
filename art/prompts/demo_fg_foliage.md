# demo_fg_foliage (living-painting spike, REEB-158)

- Model: `recraft` — returned opaque RGB WebP AND ignored the
  "transparent top three quarters" instruction; produced a full
  photorealistic dusk scene instead.
- Salvage: bottom ~38% of the image is out-of-focus dark grass — exactly the
  foreground-frame look wanted. Cropped that band, resized to scene width,
  feathered a vertical alpha ramp (opaque by ~55% down), darkened 18%.
  Promoted to `demo/living-painting/assets/fg_foliage.png`.
- Lesson: recraft does not honor "empty transparent region" compositional
  asks reliably; band-crop + alpha-ramp salvage is a viable fallback.

## Prompt

```
hand-painted cozy fantasy village, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text, dark out-of-focus foreground foliage framing border: blurry silhouetted grass blades, leaves and small flowers along the bottom edge and lower corners only, top three quarters of image completely empty transparent, deep shadow tones with warm rim light, foreground frame for a game scene, transparent background, alpha channel PNG
```
