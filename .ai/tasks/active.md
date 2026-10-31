# Active tasks

Jira-synced snapshot (Mode: jira — DEC-10).

## REEB-158 — Build living-painting web demo (In Progress)

- **Parent**: REEB-157 (story: Visual spike — living-painting web demo), under epic REEB-123
- **Spec**: `.ai/features/visual-spike-living-painting.md`
- **Executor**: orchestrator in-session (Subagents fallback clause — single-context visual see→edit loop)
- **Scope**: `demo/living-painting/**`, `art/staging/**`, `art/prompts/**`
- **Exit criteria**: demo renders via local static server, ≥3 motion layers + walkable reeble + depth cues, no console errors, prompts recorded in `art/prompts/`
- **Then**: present to user for beauty verdict (go/no-go on living-painting direction); post completion comment + transition REEB-158/157
