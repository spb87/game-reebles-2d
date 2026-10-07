# Active tasks

Tasks currently being worked on by a Developer agent. Maximum one task active at a time.

---

### TASK-2: Art pipeline port

- **Feature**: m1-repo-toolchain-scaffold.md
- **Role**: Developer
- **Files allowed**: `art/tools/gen_image.py`, `art/style-anchor.txt`, `art/README.md`, `art/prompts/.gitkeep`
- **Input**: Copy `../game-reebles/art/tools/gen_image.py` **verbatim** (stdlib-only RouteLLM helper — verified working). Style anchor (from scope doc): `hand-painted cozy fantasy village, warm golden palette, soft watercolor brush strokes, flat game art, clean silhouettes, no text` — write it to `art/style-anchor.txt` as a single line. `art/README.md` documents: the pipeline (anchor + per-asset prompt → `gen_image.py` → `art/staging/` → promote to `game/Assets/Art/` + prompt record in `art/prompts/<asset>.md`), the model menu (seedream default; nano_banana_pro/gpt_image25 hero; *_edit iteration), and the alpha/transparency open question (3-call experiment deferred to first M3 asset task — do NOT run generations now).
- **Exit criteria**: `python3 art/tools/gen_image.py --help` exits 0; `test -s art/style-anchor.txt`; `git check-ignore art/staging/x.png` exits 0.
- **Max new lines**: ~150 (mostly copied file)
- **Dependencies**: TASK-1 (done)
- **Status**: in progress
- **Git**: `chore(TASK-2): port RouteLLM art pipeline (gen_image.py, style anchor)`
