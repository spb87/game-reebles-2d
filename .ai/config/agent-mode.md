# Agent mode

Declares which workflow this project uses. Every agent session reads this file first (via `.devin/rules/agent-workflow.md`, which is always-on).

```
Mode: autonomous
Subagents: true
```

## Modes

### `autonomous` (default)

A single agent session performs Architect and Developer responsibilities end-to-end:

- Plans with the user, writes feature specs, decomposes into Jira issues
- Executes tasks by dispatching the `developer` subagent (`.devin/agents/developer.md`) — one task per subagent, small focused context
- Maintains Jira and the `.ai/` snapshot files (`active.md`, `bugs/open.md`) directly
- Pauses for user review at **feature boundaries**; mid-work it only stops for clarification (ambiguous requirements, security-sensitive changes, destructive operations, conflicts with recorded decisions)

Role file: `.ai/roles/autonomous.md`

#### `Subagents:` flag (autonomous mode only)

- `true` (default): tasks execute via the `developer` subagent — small isolated context per task, cheaper pinned model.
- `false`: the orchestrator executes tasks **directly in-session** — fallback for hung or unresponsive subagents. It still follows the Developer contract (declared scope, exit criteria, completion reports, clarity checkpoints) but may update a task's declared scope before proceeding, noting the change in its report.

Hard kill-switch for Devin CLI: `"subagents_enabled": false` in `.devin/config.json` removes the subagent tools entirely — set it when subagents are misbehaving. The flag above governs the workflow contract for any runtime.

### `supervised` (human-in-the-loop)

The original manual workflow:

- Architect and Developer run as separate chat sessions; the user pastes a role file into each
- User moves tasks between sessions and confirms acceptance echoes
- Role files: `.ai/roles/architect.md`, `.ai/roles/developer.md`
- Trigger words (`.windsurfrules`, legacy): `architect` → `architect.md`, `developer` → `developer.md`

## Switching modes

Change `Mode:` above. Both modes share the same memory files, task/bug formats, Jira conventions, and commit discipline — switching is safe mid-project. Completed work and in-flight tasks carry over; only the execution mechanics differ.

`Subagents:` can be flipped at any time, including mid-feature — it only changes who executes the next task.
