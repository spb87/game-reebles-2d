# Bootstrap checklist

Read this when creating tasks for a project's first milestone. These items are easy to forget but painful to bolt on later. Each one should be an explicit task or included in the first scaffolding task.

## Must-have (include in first milestone, first task)

- [ ] `.gitignore` appropriate for the stack (node_modules, __pycache__, .env, .venv, build artifacts, etc.)
- [ ] `README.md` at the project root with: project name, one-line description, prerequisites, how to install dependencies, how to run the dev server, how to run tests
- [ ] `.env.example` with placeholder values for any secrets or config (API keys, database URLs, etc.)
- [ ] Formatter and linter configured (Prettier/ESLint, Black/Ruff, etc.) — match what's specified in `config/conventions.md`
- [ ] Dependency environment initialized:
  - Python: virtual environment created (`python -m venv .venv`), `pyproject.toml` or `requirements.txt` with initial dependencies, `.venv` added to `.gitignore`
  - Node: `package.json` initialized, lock file committed
  - Other: language-appropriate dependency isolation set up

## Should-have (include in first milestone, can be a separate task)

- [ ] `config/conventions.md` fully filled in — not left with placeholder values
- [ ] `memory/stack.md` updated with exact versions (not "latest")
- [ ] CI-friendly test command that works with zero configuration (e.g., `npm test`, `pytest`)
- [ ] Editor config (`.editorconfig` or equivalent) so contributors get consistent formatting

## Nice-to-have (can wait for later milestones)

- [ ] CI/CD pipeline config (GitHub Actions, etc.)
- [ ] Docker / container setup
- [ ] Contributing guide
- [ ] License file
- [ ] Changelog

## How to use this

When you write the first task for a new project (typically "Scaffold project"), include the must-have items in that task's exit criteria. For example:

```
- **Exit criteria**: `npm run dev` starts the dev server. `.gitignore` excludes node_modules and .env. `README.md` exists with install and run instructions. ESLint runs with no errors on `npm run lint`.
```

For Python projects:
```
- **Exit criteria**: `.venv` exists and is in `.gitignore`. `pip install -r requirements.txt` (or `pip install -e .`) succeeds inside the venv. `pytest` runs with no errors. `README.md` exists with setup instructions including venv activation.
```

The should-have items can be a separate TASK-2 if the first task would get too big, but they should be done before any feature work begins.
