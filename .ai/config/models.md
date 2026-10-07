# Model configuration

Maps roles to models. Two modes are supported — see `.ai/config/agent-mode.md`.

## Autonomous mode (Devin — default)

| Role | Agent | Model | Reasoning |
|------|-------|-------|-----------|
| Orchestrator | Devin session per `.ai/roles/autonomous.md` | Frontier (SWE-2 High / Opus) | Planning, decomposition, Jira, review, all `.ai/` updates — requires frontier reasoning |
| Developer | `.devin/agents/developer.md` subagent | `swe-2-medium` (pinned via `model:` in profile) | Focused single-task execution — small context, cheaper model |

- One subagent per task; sequential by default.
- Fallback: `Subagents: false` in `.ai/config/agent-mode.md` makes the orchestrator execute tasks in-session on the frontier model — slower and costlier, but immune to subagent hangs. (`"subagents_enabled": false` in `.devin/config.json` is the hard kill-switch that removes the tools.)
- The `model:` pin is what keeps task execution off the orchestrator's expensive model — `subagent_general` would inherit the parent model, so the custom profile is required.
- Verify the `model:` identifier in `.devin/agents/developer.md` against the `/model` picker — thinking-level suffixes may not be pinnable; use the closest family name (e.g., `swe-2` or `swe`) if it errors.

## Supervised mode (human-in-the-loop)

| Role | Agent | Model | Reasoning |
|------|-------|-------|-----------|
| Architect | Pasted `.ai/roles/architect.md` (main session) | Frontier (Opus-class) | Decision-making, decomposition, review, and all `.ai/` file updates |
| Developer | Pasted `.ai/roles/developer.md` (separate session) | Mid-tier (Sonnet-class) | Code execution, testing — balances capability with cost |

## Notes

- The Architect/orchestrator runs on whatever model the user selects for their session. For best results, use frontier models for Architect work.
- Post-completion `.ai/` file updates (moving tasks, updating context, logging bugs) are performed by the Architect/orchestrator — there is no separate Scribe role.
- Monitor `costs/actuals.md` to track whether the delegation model is achieving the expected cost distribution.

## Cost optimization targets

- Orchestrator / Architect (frontier): ~25-35% of total token spend — decisions, orchestration, review, and `.ai/` record-keeping
- Developer (mid-tier / swe-2-medium): ~60-70% of total token spend — this is where most work happens
