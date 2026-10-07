# Migrate to Devin

This guide explains how to migrate an existing `.ai/` framework project (supervised mode, or coming from another runtime) to Devin.

## Prerequisites

- Devin CLI or Devin Desktop installed and configured
- Existing project with `.ai/` directory and role files (`.ai/roles/`)

## Migration steps

### 1. Create `.devin/` directory structure

Create the following structure in your project root:

```
.devin/
  rules/
    agent-workflow.md       ← always-on mode router
  agents/
    developer.md            ← task-execution subagent profile
  config.json               ← MCP servers, agent settings
  wiki.json                 ← optional DeepWiki steering
```

### 2. Add the mode router rule

Create `.devin/rules/agent-workflow.md`:

```markdown
---
description: Agent workflow mode — routes every session to the correct .ai role
trigger: always_on
---

This project uses the `.ai/` multi-agent task framework. At the start of any session that involves project work:

1. Read `.ai/config/agent-mode.md` for the current mode.
2. **`Mode: autonomous`** (default): operate per `.ai/roles/autonomous.md` — combined Architect + Developer orchestration. Tasks execute via the `developer` subagent (`.devin/agents/developer.md`) when `Subagents: true` in agent-mode.md; when `Subagents: false` the orchestrator executes in-session (fallback for hung/unresponsive subagents).
3. **`Mode: supervised`**: manual role workflow — the user pastes a role file; `architect` → `.ai/roles/architect.md`, `developer` → `.ai/roles/developer.md`.

When in doubt about project state, read `.ai/memory/context.md` and `.ai/tasks/active.md`.
```

### 3. Add the developer subagent profile

Create `.devin/agents/developer.md` — a thin dispatch profile that points at the Developer contract (`.ai/roles/developer.md`) rather than duplicating it. Use `agents/developer.md` from the `.devin` skeleton shipped with this boilerplate as the template; it includes:

- YAML frontmatter: `name: developer`, dispatch-oriented `description`, a `model:` pin (e.g. `swe-2-medium` — verify against the `/model` picker), and `allowed-tools`.
- Setup steps: read the contract, read `active.md`, read only permitted files.
- Dispatch overrides: proceed after TASK ACCEPTANCE without waiting, never write `.ai/` files, return the completion/error report verbatim, stop on clarity checkpoints.
- Boundaries: touch only "Files allowed", no new files/dependencies/decisions, conventional commits.

The Architect has **no** subagent profile — in autonomous mode the orchestrator is the main session; in supervised mode the user pastes `.ai/roles/architect.md` into a session.

### 4. Add or update `.ai/config/agent-mode.md`

Create the mode declaration (see `.ai/config/agent-mode.md` in the boilerplate for the full file):

```
Mode: autonomous
Subagents: true
```

Also add `.ai/roles/autonomous.md` (the orchestrator role) if the project doesn't have it.

### 5. Create `.devin/config.json`

Migrate MCP server configuration from existing sources, and review the agent settings:

```json
{
  "subagents_enabled": true,
  "mcpServers": {
    "your-mcp-server": { "...": "see MCP server docs" }
  }
}
```

- `"subagents_enabled": false` is the hard kill-switch — it removes the subagent tools entirely. The `Subagents:` flag in `agent-mode.md` is the softer, workflow-level equivalent.
- Machine-local or secret-bearing MCP config can live in `.devin/mcp_config.local.json` instead of the committed `config.json`.

### 6. (Optional) Create `.devin/wiki.json`

Create a DeepWiki steering configuration to guide auto-generated documentation. Use `wiki.json` from the `.devin` skeleton shipped with this boilerplate as the template — `repo_notes` describe the `.ai/` memory layer and the orchestrator + developer-subagent model, and `pages` steer the generated docs at project brief, stack, decisions, workflow, and conventions.

### 7. Update `.ai/config/conventions.md`

Ensure the Devin conventions section exists: DeepWiki usage, Spaces usage, Fast Context usage.

### 8. Update `.ai/README.md` and `.ai/config/models.md`

- README: document the autonomous workflow and update the directory tree to include `.devin/`.
- `models.md`: map roles to models for autonomous mode (frontier orchestrator + pinned mid-tier developer subagent) and supervised mode.

### 9. Test the migration

1. Start a Devin session in the project.
2. Verify the `agent-workflow.md` rule routes the session to `.ai/roles/autonomous.md` (autonomous) or prompts for a role file (supervised).
3. In autonomous mode, confirm the orchestrator can read `.ai/` files and dispatch the `developer` subagent.
4. Run one trivial task end-to-end: dispatch → completion report → Jira/snapshot update → `docs(ai)` commit.

### 10. Commit the migration

```
git add .devin/ .ai/config/agent-mode.md .ai/roles/autonomous.md .ai/config/models.md .ai/config/conventions.md .ai/README.md
git commit -m "chore: migrate to Devin workflow"
```

## Rollback

To revert to pure supervised mode:

1. Set `Mode: supervised` in `.ai/config/agent-mode.md` — no other change needed; sessions paste `.ai/roles/` files.
2. To fully detach from Devin, delete `.devin/` and remove `agent-mode.md`/`autonomous.md`.

The `.ai/` memory layer is unchanged by this migration, so no data is lost.

## What doesn't change

- `.ai/` directory contents and formats remain identical
- Task tracking mode (local or Jira) is unchanged
- All existing tasks, decisions, bugs, and costs are preserved
- The Architect and Developer contracts still apply — autonomous mode references them rather than replacing them

## Benefits of migration

- Automatic context isolation: one subagent per task, small focused contexts
- Cost split: cheap pinned model executes tasks; the expensive model orchestrates
- Always-on mode routing — no manual role pasting in autonomous mode
- DeepWiki auto-generates architecture documentation (Devin Desktop)
- Fast Context reduces context retrieval costs
