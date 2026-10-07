# Active tasks

Tasks currently being worked on by a Developer agent. Maximum one task active at a time.

---

### TASK-3: Repo tools + README

- **Feature**: m1-repo-toolchain-scaffold.md
- **Role**: Developer
- **Files allowed**: `tools/run-unity.bat`, `tools/serve-webgl.py`, `README.md`
- **Input**: Port `../game-reebles/tools/run-unity.bat` verbatim (already points at the pinned editor). Port `../game-reebles/tools/serve-webgl.py` but change the default `--dir` to `game/Builds/Web`. Root `README.md`: project name + one-liner, prerequisites (Unity 6000.3.24f1 on Windows, Python 3.10+), setup (`.env` with `ROUTELLM_API_KEY` — see `.env.example`), how to open the project (`game/` in Unity), test command (batch `-runTests`), build/serve commands (deferred links are fine — the exact build task is TASK-11), and the deploy note (GitHub Pages, DEC-3 in `.ai/memory/decisions.md`).
- **Exit criteria**: `python3 tools/serve-webgl.py --help` exits 0; `README.md` exists and names all required commands; `tools/run-unity.bat` contains `6000.3.24f1`.
- **Max new lines**: ~160
- **Dependencies**: TASK-1 (done)
- **Status**: in progress
- **Git**: `chore(TASK-3): add repo tools (run-unity, serve-webgl) and README`
