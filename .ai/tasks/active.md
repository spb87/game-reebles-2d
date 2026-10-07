# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10). The Architect writes the current task here after transitioning its Jira issue to In Progress.

---

### REEB-138: GitHub repo + Pages deploy

- **Feature**: REEB-126 (`m1-web-smoke-deploy.md`)
- **Role**: Developer
- **Files allowed**: `tools/deploy-pages.py`, `README.md` (deploy section) — plus authorized remote side effects: `gh.exe repo create`, `git remote add`, `git push` to `spb87/game-reebles-2d`
- **Input**: `tools/deploy-pages.py` (stdlib Python; `gh` is Windows-side → invoke `gh.exe` via WSL interop). Steps: (1) `gh.exe repo create spb87/game-reebles-2d --public --source . --remote origin --push` (pushes `main`); (2) stage `game/Builds/Web/*` into orphan `gh-pages` branch (git worktree or temp clone), add `.nojekyll`, commit, push; (3) `gh.exe api repos/spb87/game-reebles-2d/pages -X POST -f "source[branch]=gh-pages" -f "source[path]=/"` enables Pages; (4) poll `curl -sI https://spb87.github.io/game-reebles-2d/` for 200 (propagation may take minutes — report URL even if pending, with the exact check command).
- **Exit criteria**: `git ls-remote origin gh-pages` lists a commit; `curl -sI https://spb87.github.io/game-reebles-2d/ | head -1` → 200 (or pending-propagation note + check command); repo public; `.nojekyll` on gh-pages.
- **Max new lines**: ~160
- **Dependencies**: REEB-137 (done — `game/Builds/Web` already built)
- **Status**: in progress
- **Git**: `feat(REEB-138): add GitHub Pages deploy tooling`, plus first live deploy.
- **Pre-flight check**: before pushing, `git ls-files | grep -iE '\.env$|secret|key'` must return nothing sensitive — the repo is going PUBLIC. `.env` is gitignored but verify anyway.
