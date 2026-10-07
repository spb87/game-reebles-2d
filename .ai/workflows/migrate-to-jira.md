---
description: Migrate from local task management to Jira
---

# Migrate to Jira task management

This workflow converts a project from local file-based task tracking (`.ai/tasks/backlog.md`, etc.) to Jira via MCP.

## Prerequisites

- A Jira Cloud instance with an existing project
- Access to the Jira Cloud ID and project key
- All local tasks should be in a stable state (no mid-task work)

## Steps

### 1. Prepare Jira project

Ensure your Jira project is set up with:
- **Project type**: Any (Scrum, Kanban, etc.)
- **Issue types**: At minimum, "Task" and "Bug"
  - **Optional**: "Epic" or "Story" for migrating features and epics
- **Custom fields** (optional but recommended):
  - `Task Type` (text) — to store "unit of work" metadata
  - `Files Allowed` (text) — to store file constraints
  - `Max New Lines` (number) — to store size estimate
  - `Dependencies` (text) — to store TASK-{n} dependencies
  - **Epic Link** (for Epic-Story relationships) — standard Jira field for nesting stories under epics
  - **Parent** (for Story-Task relationships) — standard Jira field for nesting tasks under stories

If custom fields don't exist, the Architect will include this metadata in the issue description instead.

**Decision point**: Decide whether to migrate features and epics to Jira:
- **Option A**: Keep features and epics local (`.ai/features/` and `.ai/epics/` remain as design documents)
- **Option B**: Migrate features and epics to Jira as "Epic" or "Story" issues

If choosing Option B, ensure your Jira project has "Epic" or "Story" issue types configured, and verify that the "Epic Link" and "Parent" fields are available for proper visual nesting.

### 2. Update task-management.md

Edit `.ai/config/task-management.md`:

```
Mode: jira
Jira Cloud ID: {your-site.atlassian.net or UUID}
Jira Project Key: {e.g., PROJ}
Jira Board ID: {optional}
```

### 3. Export local tasks

Before deleting local task files, export them for reference:

```bash
# Create a backup directory
mkdir -p .ai/tasks/archive/pre-jira-migration

# Copy all task files
cp .ai/tasks/backlog.md .ai/tasks/archive/pre-jira-migration/
cp .ai/tasks/active.md .ai/tasks/archive/pre-jira-migration/
cp .ai/tasks/done.md .ai/tasks/archive/pre-jira-migration/
```

### 4. Create Jira issues from local tasks

In an Architect session:

1. Read `.ai/tasks/archive/pre-jira-migration/backlog.md`
2. For each task, use `mcp0_createJiraIssue` to create a Jira issue:
   - **Project key**: from `task-management.md`
   - **Issue type**: "Task"
   - **Summary**: `TASK-{number}: {title}` (preserve the TASK-{n} numbering)
   - **Description**: Include all task metadata (Feature, Files allowed, Input, Exit criteria, etc.)
   - **Status**: "Backlog" (or equivalent in your workflow)
   - **Labels**: Add `task-backlog` label
   - **Parent** (custom field): If the task references a feature in its "Feature" field, set this to the Jira feature issue key for proper visual nesting under the story

3. Record the Jira issue key for each TASK-{n} in a temporary mapping file (e.g., `.ai/tasks/jira-mapping.txt`):
   ```
   TASK-1 → PROJ-123
   TASK-2 → PROJ-124
   ...
   ```

4. Repeat for tasks in `active.md` (use "In Progress" status) and `done.md` (use "Done" status)

### 5. Sync active.md from Jira

After creating all Jira issues:

1. If there is a current active task, fetch its details from Jira using `mcp0_getJiraIssue`
2. Populate `.ai/tasks/active.md` with the current task in the standard format
3. If no active task, leave `active.md` empty or with just a header

### 6. Create Jira issues from bugs

If you have bugs tracked locally:

1. Read `.ai/bugs/open.md`
2. For each bug, create a Jira issue with:
   - **Issue type**: "Bug"
   - **Summary**: `BUG-{number}: {title}`
   - **Description**: Include all bug metadata
   - **Status**: "Open" (or equivalent)
   - **Labels**: Add `bug-open` label

3. Repeat for `.ai/bugs/resolved.md` (use "Done" status, add `bug-resolved` label)

4. Sync `.ai/bugs/open.md` from Jira (only open bugs)

### 7. Migrate features and epics (Option B only)

If you chose Option B in step 1 (migrate features and epics to Jira):

#### Migrate epics

1. Read `.ai/epics/` directory to find all epic files
2. For each epic, create a Jira issue with:
   - **Issue type**: "Epic" (or "Story" if Epic not available)
   - **Summary**: `EPIC-{number}: {title}` (use the epic filename or title)
   - **Description**: Include all epic metadata:
     - Vision
     - Milestones
     - Scope boundaries
     - Technical risks
     - Go/no-go checkpoints
   - **Status**: "Backlog" (or equivalent)
   - **Labels**: Add `epic` label

3. Record the Jira issue key for each epic in a mapping file (e.g., `.ai/epics/jira-mapping.txt`)

#### Migrate features

1. Read `.ai/features/` directory to find all feature files
2. For each feature, create a Jira issue with:
   - **Issue type**: "Story" (or "Epic" if Story not available)
   - **Summary**: `FEATURE-{number}: {title}` (use the feature filename or title)
   - **Description**: Include all feature metadata:
     - Goal
     - Acceptance criteria
     - Known constraints
     - Out of scope
   - **Status**: "Backlog" (or equivalent)
   - **Labels**: Add `feature` label
   - **Epic Link** (custom field): If the feature belongs to an epic, set this to the Jira epic issue key for proper visual nesting

3. Record the Jira issue key for each feature in a mapping file (e.g., `.ai/features/jira-mapping.txt`)

#### Archive local feature and epic files

After migration:

```bash
# Archive local epics
mkdir -p .ai/epics/archive/pre-jira-migration
cp .ai/epics/*.md .ai/epics/archive/pre-jira-migration/

# Archive local features
mkdir -p .ai/features/archive/pre-jira-migration
cp .ai/features/*.md .ai/features/archive/pre-jira-migration/

# Clear the directories (keep only templates if they exist)
# (Optionally keep _template.md files for reference)
```

**If you chose Option A** (keep features and epics local): No action needed. The `.ai/features/` and `.ai/epics/` directories remain as design documents.

### 8. Archive local task files

Once all tasks and bugs are in Jira and synced to snapshots:

```bash
# Clear the active task file (keep header only)
echo "# Active task" > .ai/tasks/active.md

# Clear the backlog (keep header only)
echo "# Backlog" > .ai/tasks/backlog.md

# Clear done tasks (keep header only)
echo "# Completed tasks" > .ai/tasks/done.md

# Clear open bugs (keep header only)
echo "# Open bugs" > .ai/bugs/open.md

# Clear resolved bugs (keep header only)
echo "# Resolved bugs" > .ai/bugs/resolved.md
```

The archive directory `.ai/tasks/archive/pre-jira-migration/` remains for audit purposes.

### 8. Verify and commit

1. Verify that `.ai/tasks/active.md` and `.ai/bugs/open.md` are properly synced from Jira
2. Verify that `task-management.md` is set to `Mode: jira`
3. Commit all changes:
   ```bash
   git add .ai/
   git commit -m "docs(ai): migrate to Jira task management"
   ```

### 9. Update context.md

Add a note to `.ai/memory/context.md`:

```
## Task management

As of {date}, this project uses Jira for task and bug tracking. See `.ai/config/task-management.md` for configuration.

Local snapshot files (`.ai/tasks/active.md`, `.ai/bugs/open.md`) are synced from Jira and should not be edited directly.

Pre-migration local tasks are archived in `.ai/tasks/archive/pre-jira-migration/` for reference.
```

## Rollback

If you need to revert to local task management:

1. Restore the pre-migration backup:
   ```bash
   cp .ai/tasks/archive/pre-jira-migration/* .ai/tasks/
   cp .ai/bugs/archive/pre-jira-migration/* .ai/bugs/
   ```

2. Update `task-management.md` to `Mode: local`

3. Commit:
   ```bash
   git add .ai/
   git commit -m "docs(ai): revert to local task management"
   ```

Note: Any work done in Jira after migration will not be reflected in the restored local files. Use this only if you decide Jira is not the right fit.
