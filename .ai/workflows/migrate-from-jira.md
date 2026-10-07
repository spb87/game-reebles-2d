---
description: Migrate from Jira back to local task management
---

# Migrate from Jira to local task management

This workflow converts a project from Jira task tracking back to local file-based management (`.ai/tasks/backlog.md`, etc.).

Use this if:
- Your team is too small to benefit from Jira
- You prefer the simplicity of local files
- You want to reduce external dependencies
- You're switching to a different task management system

## Prerequisites

- Project is currently in `Mode: jira` (see `.ai/config/task-management.md`)
- You have access to the Jira project to export issues
- All tasks are in a stable state

## Steps

### 1. Export tasks from Jira

In an Architect session:

1. Use `mcp0_searchJiraIssuesUsingJql` to fetch all tasks:
   ```
   JQL: project = {PROJECT_KEY} AND type = Task
   ```

2. For each issue, use `mcp0_getJiraIssue` to fetch full details
3. Convert each Jira issue back to the local task format and collect them, preserving the "Feature" field by:
   - If the task has a "Parent" field set to a feature issue, set the "Feature" field to the feature filename or ID
   - If no Parent field exists, set "Feature" to "none"

### 2. Rebuild local task files

Create or update the local task files:

#### `.ai/tasks/backlog.md`

```
# Backlog

### TASK-{number}: {title}

- **Feature**: {feature filename}
- **Role**: Developer
- **Files allowed**: {list}
- **Input**: {context}
- **Exit criteria**: {verification steps}
- **Max new lines**: {estimate}
- **Dependencies**: {TASK-numbers or "none"}
- **Status**: backlog

[Repeat for all backlog tasks from Jira]
```

#### `.ai/tasks/active.md`

If there is a task currently "In Progress" in Jira:

```
# Active task

### TASK-{number}: {title}

- **Feature**: {feature filename}
- **Role**: Developer
- **Files allowed**: {list}
- **Input**: {context}
- **Exit criteria**: {verification steps}
- **Max new lines**: {estimate}
- **Dependencies**: {TASK-numbers or "none"}
- **Status**: active
```

If no active task, leave empty:

```
# Active task
```

#### `.ai/tasks/done.md`

```
# Completed tasks

### TASK-{number}: {title}

- **Feature**: {feature filename}
- **Role**: Developer
- **Files allowed**: {list}
- **Input**: {context}
- **Exit criteria**: {verification steps}
- **Max new lines**: {estimate}
- **Dependencies**: {TASK-numbers or "none"}
- **Status**: done

COMPLETION REPORT:
- Task: TASK-{number}
- Status: complete
- Files modified: {list}
- What was done: {summary}
- How to verify: {steps}
- Observations: {notes}
- Follow-up tasks suggested: {list or "none"}
- Clarity checkpoint: all 5 passed
- Git commit: {commit hash or message}

[Repeat for all completed tasks from Jira]
```

### 3. Export bugs from Jira

Use `mcp0_searchJiraIssuesUsingJql` to fetch all bugs:

```
JQL: project = {PROJECT_KEY} AND type = Bug
```

### 4. Export epics from Jira (if migrated)

If you previously migrated epics to Jira (using Option B in migrate-to-jira.md):

1. Use `mcp0_searchJiraIssuesUsingJql` to fetch all epics:
   ```
   JQL: project = {PROJECT_KEY} AND type = Epic
   ```
   (Or use `type = Story` if you used Story for epics)

2. For each epic, use `mcp0_getJiraIssue` to fetch full details

3. Convert each Jira epic back to the local epic format and create files in `.ai/epics/`:
   ```
   # EPIC-{number}: {title}

   ## Vision

   {vision text}

   ## Milestones

   {milestones}

   ## Scope boundaries

   {scope}

   ## Technical risks

   {risks}

   ## Go/no-go checkpoints

   {checkpoints}
   ```

### 5. Export features from Jira (if migrated)

If you previously migrated features to Jira (using Option B in migrate-to-jira.md):

1. Use `mcp0_searchJiraIssuesUsingJql` to fetch all features:
   ```
   JQL: project = {PROJECT_KEY} AND type = Story
   ```
   (Or use `type = Epic` if you used Epic for features)

2. For each feature, use `mcp0_getJiraIssue` to fetch full details
3. Convert each Jira feature back to the local feature format and create files in `.ai/features/`:
   ```
   # FEATURE-{number}: {title}

   ## Goal

   {goal text}

   ## Acceptance criteria

   1. {criterion 1}
   2. {criterion 2}
   ...

   ## Known constraints

   {constraints}

   ## Out of scope

   {out of scope items}
   ```

4. **Restore epic-feature relationships**: If the feature has an "Epic Link" field set to an epic issue, add an `## Epic` section to the feature file with the epic name or ID:
   ```
   ## Epic

   EPIC-{number}: {epic title}
   ```

### 6. Rebuild local bug files

#### `.ai/bugs/open.md`

```
# Open bugs

### BUG-{number}: {title}

- **Found in**: TASK-{number}
- **Date**: {YYYY-MM-DD}
- **Description**: {what's wrong}
- **Severity**: low | medium | high | critical
- **Reproduction**: {steps or "not yet confirmed"}

[Repeat for all open bugs from Jira]
```

#### `.ai/bugs/resolved.md`

```
# Resolved bugs

### BUG-{number}: {title}

- **Found in**: TASK-{number}
- **Date**: {YYYY-MM-DD}
- **Description**: {what's wrong}
- **Severity**: low | medium | high | critical
- **Reproduction**: {steps or "not yet confirmed"}

BUG FIX REPORT:
- Bug: BUG-{number}
- Status: fixed
- Files modified: {list}
- Root cause confirmed: yes
- What was done: {summary}
- Regression test: {test file and name}
- How to verify: {steps}
- Side effects: none
- Git commit: {commit hash}

[Repeat for all resolved bugs from Jira]
```

### 7. Update task-management.md

Edit `.ai/config/task-management.md`:

```
Mode: local
```

Remove the Jira configuration lines.

### 8. Archive Jira mapping (optional)

If you created a Jira mapping file during migration-to-jira, you can keep it for reference:

```bash
mkdir -p .ai/tasks/archive/jira-migration
cp .ai/tasks/jira-mapping.txt .ai/tasks/archive/jira-migration/
```

This helps with auditing if you ever need to cross-reference Jira issue keys with local TASK-{n} numbers.

### 9. Verify and commit

1. Verify that all task and bug files are properly populated
2. Verify that `task-management.md` is set to `Mode: local`
3. Verify that `.ai/tasks/active.md` and `.ai/bugs/open.md` are correct
4. Commit all changes:
   ```bash
   git add .ai/
   git commit -m "docs(ai): migrate from Jira to local task management"
   ```

### 10. Update context.md

Add a note to `.ai/memory/context.md`:

```
## Task management

As of {date}, this project uses local file-based task tracking. See `.ai/config/task-management.md` for configuration.

All tasks and bugs are tracked in `.ai/tasks/` and `.ai/bugs/` directories.

Previous Jira integration is archived in `.ai/tasks/archive/jira-migration/` for reference.
```

## Rollback

If you decide to go back to Jira:

1. Follow the `.ai/workflows/migrate-to-jira.md` workflow
2. Note that any work done locally after this migration will need to be manually re-created in Jira

## Considerations

- **Data loss**: Jira comments and custom field data not captured in the task description will be lost
- **History**: Jira issue history (status changes, comments) is not preserved in local files
- **Collaboration**: Local files are less suitable for distributed teams; consider keeping Jira for large projects
