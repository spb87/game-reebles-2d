# demo_fog_wisp (living-painting spike, REEB-158)

- Model: `recraft` — returned opaque RGB WebP (wisp on white background, no alpha)
- Post-processing: luminance -> alpha (`alpha = clip((252 - L) * 2.4, 0, 200)`),
  RGB filled white, crop to bbox, resize 700px wide.
  Promoted to `demo/living-painting/assets/fog.png`.
- Lesson: for white-on-white subjects (fog, mist, glow), luminance-keyed alpha
  is a reliable salvage when recraft drops transparency.

## Prompt

```
single soft wispy horizontal fog cloud, gentle translucent white mist, dreamy painterly edges, isolated fog wisp only, no background, no ground, transparent background, alpha channel PNG, soft watercolor brush strokes
```
