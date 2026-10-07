# .ai/ — Multi-agent development framework

A project overlay that enables structured multi-agent workflows for AI-assisted coding. Two agent roles (Architect, Developer) work together with shared memory, task management, and drift detection — all stored as markdown files in this directory.

This boilerplate is built for **Devin** (Devin CLI / Devin Desktop). The `.ai/` directory and all its contents are runtime-agnostic markdown; the `.devin/` directory holds the Devin-specific wiring. The active workflow is declared in `.ai/config/agent-mode.md` (`autonomous` or `supervised`).

---

## Choose your workflow

### Autonomous mode (default)

A single agent session orchestrates the whole framework: it plans features, decomposes into Jira issues, executes each task via a `developer` subagent, and maintains all memory files. The user reviews at feature boundaries and answers clarification requests.

**Agent files:** `.devin/rules/agent-workflow.md` (always-on mode router), `.ai/roles/autonomous.md` (orchestrator role), `.devin/agents/developer.md` (task-execution subagent profile — `model:` pinned, e.g. `swe-2-medium`)

**How it works:**
```
User describes work → Orchestrator writes spec, creates Jira issues, decomposes tasks
  → For each task: sync to active.md → dispatch `developer` subagent
  → Subagent executes, commits code, returns completion report
  → Orchestrator reviews report, posts to Jira, updates memory, commits .ai/
  → Repeat until the feature is complete, then pause for user review
```

The subagent keeps each task's context small and isolated — same principle as the supervised "one task per session" rule, automated. Subagents return error reports instead of guessing; the orchestrator escalates to the user when needed.

**Subagent fallback:** if subagents hang or go unresponsive, set `Subagents: false` in `.ai/config/agent-mode.md` — the orchestrator then executes tasks directly in-session (still honoring the Developer contract). `"subagents_enabled": false` in `.devin/config.json` is the hard kill-switch that removes the subagent tools entirely.

**Quick start:**
1. Copy the `.ai/` and `.devin/` directories into a new repo.
2. Confirm `.ai/config/agent-mode.md` says `Mode: autonomous`.
3. Start a Devin session. The always-on `agent-workflow.md` rule routes the session to the orchestrator role automatically.
4. Describe your project. The orchestrator runs a bootstrap conversation — filling in `project-brief.md`, `stack.md`, and `conventions.md` — then decomposes your first feature into tasks.

---

### Supervised mode (fallback)

Manual, human-in-the-loop workflow. The user pastes a role file into each chat session and moves tasks between sessions. Works in Devin or any other agent runtime (Windsurf, Claude Code, etc.) — the role files are plain markdown.

**Agent files:** `.ai/roles/architect.md`, `.ai/roles/developer.md`

**Quick start:**
1. Set `Mode: supervised` in `.ai/config/agent-mode.md`.
2. Start a chat. Paste `.ai/roles/architect.md` as context.
3. Describe your project. The Architect runs a bootstrap conversation — filling in `project-brief.md`, `stack.md`, and `conventions.md`.
4. The Architect decomposes your first feature into tasks.

**How role switching works:**
- Start a chat, paste a role file, do the work, end the session.
- Context boundaries are enforced by starting fresh chat sessions.
- Use the most capable model for Architect sessions and a mid-tier model for Developer sessions.

**Typical workflow:**
1. Architect session (new chat)
   → Create feature spec + decompose into tasks + cost estimate
   → End session

2. Architect session (new chat)
   → Review any prior completion reports (if continuing work)
   → Process completed tasks: move to done.md, update context.md, track bugs/patterns, update costs
   → Commit all .ai/ changes to git
   → Move next task from backlog.md to active.md
   → End session

3. Developer session (new chat)
   → Paste .ai/roles/developer.md
   → Execute the task in active.md
   → Commit code changes to git
   → Write completion report in active.md
   → End session

4. Repeat steps 2-3 until all tasks complete

5. Architect session (new chat)
   → Review feature against acceptance criteria
   → Create next feature or declare milestone complete

**Key principle:** Each session is a fresh chat with a role file pasted as context. This enforces clean context boundaries and prevents drift.

---

## Task management modes

This framework supports two task tracking backends:

### Local mode (default)

Tasks and bugs are tracked in markdown files within `.ai/`:
- `.ai/tasks/backlog.md`, `active.md`, `done.md`
- `.ai/bugs/open.md`, `resolved.md`

**Best for**: Small teams, simple projects, offline work, minimal external dependencies.

**Configuration**: See `.ai/config/task-management.md` (set `Mode: local`).

### Jira mode

Tasks and bugs are tracked in Jira via MCP:
- All task and bug data lives in Jira
- `.ai/tasks/active.md` and `.ai/bugs/open.md` are synced snapshots (read by Developer)
- Architect uses Jira MCP tools to create, update, and search issues

**Best for**: Large teams, distributed collaboration, integration with other tools, audit trails.

**Configuration**: See `.ai/config/task-management.md` (set `Mode: jira` with Jira Cloud ID and project key).

**Migration**: See `.ai/workflows/migrate-to-jira.md` to switch from local to Jira. See `.ai/workflows/migrate-from-jira.md` to switch back.

---

## Core concepts (both modes)

### Unit of work
The smallest safe task for an AI agent. Touches ≤3 files, has clear input/output, verifiable in ≤3 commands, fits in one context window, requires no architectural decisions from the developer.

### Clarity checkpoint
Five self-monitoring signals the Developer runs mid-task to detect drift: scope creep, role violation, goal drift, complexity spiral, and context pollution. If any signal fires, work stops and the agent produces a structured error report. See `guardrails/clarity-checklist.md`.

### Error report vs. completion report
Failures produce a typed error report with failure category and a `Requires` field routing the next action. Successes produce a completion report with verification steps and observations. See the developer role/agent files for the exact formats.

### Epic → Milestone → Feature → Task
For large projects, the Architect creates an epic with milestones. The first milestone is always a "walking skeleton" — the thinnest vertical slice proving the architecture works. Only the current milestone is decomposed. Go/no-go checkpoints between milestones prevent runaway spending.

### Bootstrap conversation
The Architect/orchestrator doesn't expect the user to fill in template files manually. It asks about the project, writes the brief, decides the stack, and fills in conventions before creating any tasks.

---

## Directory structure

```
.devin/
  rules/
    agent-workflow.md            ← Always-on mode router (autonomous vs supervised)
  agents/
    developer.md                 ← Devin developer subagent profile (model pinned)
  config.json                    ← MCP servers, agent settings (incl. subagents_enabled)
  wiki.json                      ← DeepWiki steering configuration (optional)
.ai/
  roles/                         ← Role prompts
    autonomous.md                ← Combined Architect+Developer orchestrator
    architect.md                 ← Supervised mode / contract reference
    developer.md                 ← Developer contract (both modes)
  config/
    agent-mode.md                ← Mode declaration: autonomous | supervised
    project-brief.md             ← Filled by Architect during bootstrap
    conventions.md               ← Coding standards, testing, DI, dependencies
    models.md                    ← Role-to-model mapping
    bootstrap-checklist.md       ← Scaffolding requirements for first milestone
    task-management.md           ← Task tracking mode (local or jira)
  design/
    brief.md                     ← Colors, fonts, spacing, component patterns
    references/                  ← Screenshots, mockups for frontend work
  epics/
    _template.md                 ← Large project decomposition
    ai-chat-platform.md          ← Sample epic (delete in a real project)
  features/
    _template.md                 ← Feature specs with acceptance criteria
    m1-chat-ui-shell.md          ← Sample feature (delete in a real project)
  tasks/
    backlog.md                   ← Tasks waiting to be picked up
    active.md                    ← Task currently being executed
    done.md                      ← Completed tasks with reports
  memory/
    context.md                   ← Current project state (snapshot, not log)
    decisions.md                 ← Architectural decision log
    stack.md                     ← Tech stack with exact versions
  bugs/
    open.md                      ← Bugs found during development
    resolved.md                  ← Bugs that have been fixed
    patterns.md                  ← Recurring drift/failure patterns
  guardrails/
    clarity-checklist.md         ← 5 drift detection signals
    violation-signals.md         ← 10 observable rule-breaking patterns
  testing/
    test-plan.md                 ← Testing strategy
    playwright.config.md         ← Frontend integration testing
  costs/
    estimates.md                 ← Pre-build cost projections
    actuals.md                   ← Post-build cost tracking
```
