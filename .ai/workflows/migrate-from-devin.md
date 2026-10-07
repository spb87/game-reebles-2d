# Migrate from Devin

This guide explains how to move a Devin-based project back to the manual supervised workflow (paste-a-role-file sessions, in any runtime).

## Prerequisites

- Existing project with `.devin/` directory and `.ai/` directory
- `.ai/roles/architect.md` and `.ai/roles/developer.md` present (the supervised role contracts — they are shared by both modes and should never have been deleted)

## Migration steps

### 1. Switch to supervised mode

Set `Mode: supervised` in `.ai/config/agent-mode.md`. That's the only change required — the always-on `.devin/rules/agent-workflow.md` will route new sessions to the manual role workflow automatically.

With `Mode: supervised`:
- The user starts a session and pastes `.ai/roles/architect.md` or `.ai/roles/developer.md` as context (type `architect` or `developer` plus the file).
- The Developer contract applies verbatim; completion reports are written to `active.md` and processed by the Architect.

### 2. (Optional) Disable or remove subagents

If you want to keep autonomous mode available but stop using subagents, set `Subagents: false` in `agent-mode.md` — the orchestrator executes tasks in-session. For a hard removal of the tools, set `"subagents_enabled": false` in `.devin/config.json`.

To fully detach from Devin, delete the `.devin/` directory:

```
rm -rf .devin/
```

If the project is moving to a different runtime permanently, you can also remove `.ai/config/agent-mode.md` and `.ai/roles/autonomous.md` — they are Devin-oriented scaffolding. The shared contracts (`architect.md`, `developer.md`) and all memory/task files must stay.

### 3. Revert Devin-specific doc sections (optional)

Remove or slim down Devin sections in `.ai/config/models.md`, `.ai/config/conventions.md`, and `.ai/README.md` if they no longer apply:

```
git checkout HEAD~ -- .ai/config/models.md .ai/config/conventions.md .ai/README.md
```

Or edit manually if you have other changes you want to preserve.

### 4. Test the rollback

1. Start a new chat session in your runtime of choice.
2. Paste `.ai/roles/architect.md` as context.
3. Verify the architect can read `.ai/` files correctly.
4. Start a new chat session, paste `.ai/roles/developer.md`, and verify the developer can execute a task from `active.md`.

### 5. Commit the rollback

```
git add -A
git commit -m "chore: revert from Devin to supervised workflow"
```

## What you lose

- Automatic task dispatch — you manually start fresh sessions and move tasks between them
- Per-task subagent cost isolation (everything runs on the session model)
- DeepWiki auto-generated architecture docs and Fast Context (if leaving Devin entirely)

## What you keep

- All `.ai/` memory files (tasks, decisions, context, bugs, costs)
- The Architect and Developer contracts — identical files, identical formats
- Task tracking mode (local or Jira) is unchanged
- All feature specs and epics are preserved

## Data portability

The `.ai/` memory layer is identical regardless of orchestration mechanism. You can switch back and forth between autonomous and supervised without data loss — `agent-mode.md` is a declaration, not a migration.

## Re-migrating to Devin

Follow `migrate-to-devin.md` again. Your `.ai/` data will still be intact.
