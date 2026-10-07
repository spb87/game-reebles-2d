# Feature: Web Smoke Build + Public Deploy

**Epic**: `reebles-2d-mvp.md` · Milestone: M1 · Tracking: local (`TASK-11` … `TASK-12`)

## Goal

The greybox walking skeleton builds to the Web platform in batch mode and is served from a **public GitHub Pages URL** — the hosting path is proven before any content work.

## Acceptance criteria

1. `Reebles2D.Editor.WebBuild.Build` produces `game/Builds/Web/` (gitignored) via batch mode; engine-default template (PWA template is M5)
2. Compression config is GitHub-Pages-safe: `Compression Format: Disabled` (Pages cannot emit `Content-Encoding: br`; decompression fallback is the alternative — Disabled chosen for smoke simplicity; revisit at M5 within the ≤50 MB budget)
3. `tools/deploy-pages.py` pushes the build to the `gh-pages` branch with `.nojekyll`; `README.md` documents the deploy command
4. `https://spb87.github.io/game-reebles-2d/` returns 200 and loads the build (verified by `curl` + user spot-check on phone at feature boundary)
5. Compressed/uncompressed build size reported in the completion notes — baseline for the ≤50 MB budget

## Known constraints

- `gh.exe` is Windows-side and authenticated as `spb87` (`repo`, `workflow` scopes); repo `spb87/game-reebles-2d` created **public** (DEC-3)
- Pages enabled via REST: `gh api repos/spb87/game-reebles-2d/pages -X POST -f "source[branch]=gh-pages" -f "source[path]=/"` — first propagation can take minutes
- WebGL builds can't use threads/ASP.NET APIs — all M1 code is already compliant
- Unity license: local builds only — no CI Unity builds (DEC-3)

## Out of scope

- PWA manifest/template/installability (M5)
- Brotli compression tuning, loading-screen branding, itch.io
- Any gameplay beyond the M1 greybox

## Tasks

- TASK-11: `WebBuild` editor script + Web player settings + local serve verify
- TASK-12: GitHub repo create + `deploy-pages.py` + Pages enable + URL verify

## Status

- [x] Spec complete
- [x] Tasks decomposed
- [x] Cost estimated
- [ ] All tasks done
- [ ] Feature verified
