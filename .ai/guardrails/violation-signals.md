# Violation signals

This document catalogs observable patterns that indicate an agent has broken the rules of its role. These signals can be checked by the agent itself (self-monitoring), by the Architect (during completion report processing), or by the user (manual review).

---

## Task constraint violations

These are checked against the structured task header in `tasks/active.md`.

### V1: File scope violation

**Signal**: The completion report lists files in "Files modified" that are not in the task's "Files allowed" field.

**How to detect**: Compare the two lists. Any file in "modified" but not in "allowed" is a violation.

**Severity**: High. This means the agent took unsanctioned action on the codebase.

**What to do**: Flag for Architect review. The changes may need to be reverted, or the task scope may need retroactive expansion.

---

### V2: Excess growth

**Signal**: The implementation added significantly more lines than the task's "Max new lines" estimate (more than 50% over).

**How to detect**: Count the actual lines added (from the diff or completion report) and compare to the estimate.

**Severity**: Medium. The task may be incorrectly scoped, or the agent may have over-engineered.

**What to do**: Note in the completion review. If this happens repeatedly, the Architect should adjust their estimation approach.

---

### V3: Missing exit criteria verification

**Signal**: The completion report does not include verification results, or says "not tested."

**How to detect**: Check the "How to verify" field in the completion report. It should contain a concrete result (test output, screenshot description, or confirmed behavior).

**Severity**: High. Unverified work may be broken.

**What to do**: Do not move to `done.md`. Flag for the user to verify manually or return to a Developer session.

---

## Role boundary violations

These indicate an agent operated outside its designated role.

### V4: Developer made architectural decisions

**Signal**: The completion report describes choosing between approaches, introducing a new pattern, or creating an abstraction not specified in the task.

**How to detect**: Look for language like "I decided to...", "I chose to...", "I introduced a pattern where...", or "I created a new module/class/service for..."

**Severity**: High. Architectural decisions made without Architect review can introduce inconsistency.

**What to do**: Flag for Architect review. The decision may be fine, but it needs to be recorded in `decisions.md` and validated against existing patterns.

---

### V5: Task or decision files modified outside an Architect session

**Signal**: New tasks appear in `backlog.md` that weren't created by the Architect. Or `decisions.md` is updated during a non-Architect session.

**How to detect**: Check the session that produced the changes. If a non-Architect session modified `backlog.md` or `decisions.md` beyond simple record-keeping, it's a violation.

**Severity**: Medium. Task creation and decision-making are the Architect's job.

**What to do**: Review the content. If valid, have the Architect ratify it. If not, remove it.

---

### V6: Architect wrote production code

**Signal**: Files outside `.ai/` were modified during an Architect session.

**How to detect**: Check the diff from the session. Architect sessions should only modify files in `.ai/`.

**Severity**: Low-Medium. The Architect may have been demonstrating something or providing pseudocode that accidentally became real code.

**What to do**: Review the changes. If they're good, have a Developer create a task to formalize them. If not, revert.

---

## Process violations

These indicate the workflow itself was not followed.

### V7: Task started without acceptance echo

**Signal**: A Developer began writing code without first stating the task acceptance block (task, files, goal, exit criteria, exclusions).

**How to detect**: Check the beginning of the Developer session. The first substantive output should be the `TASK ACCEPTANCE` block.

**Severity**: Medium. Without explicit acceptance, the agent may be working from an incorrect understanding.

**What to do**: Note the pattern. If the work is correct, proceed. If issues arose, this may be the root cause.

---

### V8: Checkpoint not run

**Signal**: The completion report's "Clarity checkpoint" field is missing or says "not run."

**How to detect**: Check the completion report.

**Severity**: Medium. The checkpoints are the primary defense against drift.

**What to do**: Note the pattern in `bugs/patterns.md`. The task result may still be fine, but the safety net wasn't active.

---

### V9: Multiple tasks without fresh acceptance

**Signal**: A Developer executed a second task in the same session without re-reading `active.md` and producing a new task acceptance block.

**How to detect**: Check the session for a second `TASK ACCEPTANCE` block between the first completion report and the start of new work.

**Severity**: Medium-High. The second task is operating on stale context from the first task.

**What to do**: Review the second task's work carefully. If issues exist, they likely stem from context pollution.

---

### V10: Memory not updated after task completion

**Signal**: A task was moved to `done.md` but `context.md` was not updated to reflect the changes.

**How to detect**: Compare the timestamps or content of `done.md` and `context.md`. If `context.md` doesn't reflect the most recent completed task, the completion-processing step was skipped or incomplete.

**Severity**: Medium. Stale context means the next Developer or Architect session starts with an incorrect picture of the project.

**What to do**: Run an Architect session to catch up.

---

## Using this document

### Mode applicability

- **Supervised mode**: all signals apply as written.
- **Autonomous mode** (`.ai/roles/autonomous.md`): the `developer` subagent is still bound by V1-V3 and V7-V8 on every task. V4 is relaxed — the orchestrator may make architectural decisions, but they must be recorded in `decisions.md`, never hidden in code. V5 and V6 still apply conceptually: only the orchestrator writes `.ai/` files, and non-trivial decision records are its job. The orchestrator checks all signals when processing a subagent's report.

### For self-monitoring (Developer agent)

Before writing your completion report, scan V1-V4 and V7-V8. If any apply, note them honestly in your report. Do not try to hide violations — they will be caught during review, and hiding them wastes more tokens than reporting them.

### For the Architect

When processing a completion report, check V1-V3 and V7-V10. Record anything you find — you don't need to fix violations, just flag them. During completion reviews, check V4-V6. When reviewing `bugs/patterns.md`, look for recurring violations — they indicate systemic issues with task scoping or role definitions.

### For the user

If you see the same violation pattern appearing across multiple tasks, the system needs tuning. Common fixes:

- **Recurring V1 (scope)**: Tasks are too tightly scoped, or file dependencies aren't being identified during decomposition.
- **Recurring V2 (growth)**: The Architect is underestimating complexity. Review and adjust the estimation model.
- **Recurring V4 (decisions)**: Tasks need more specific guidance. The Architect should front-load decisions rather than leaving ambiguity.
- **Recurring V9 (context pollution)**: Sessions are running too long. Enforce the one-task-per-session default more strictly.
