---
description: Agent workflow mode — routes every session to the correct .ai role
trigger: always_on
---

This project uses the `.ai/` multi-agent task framework. At the start of any session that involves project work:

1. Read `.ai/config/agent-mode.md` for the current mode.
2. **`Mode: autonomous`** (default): operate per `.ai/roles/autonomous.md` — combined Architect + Developer orchestration. Tasks execute via the `developer` subagent (`.devin/agents/developer.md`) when `Subagents: true` in agent-mode.md; when `Subagents: false` the orchestrator executes in-session (fallback for hung/unresponsive subagents).
3. **`Mode: supervised`**: manual role workflow — the user pastes a role file; `architect` → `.ai/roles/architect.md`, `developer` → `.ai/roles/developer.md`.

When in doubt about project state, read `.ai/memory/context.md` and `.ai/tasks/active.md`.
