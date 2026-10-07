# Task management configuration

Specifies whether this project uses local file-based task tracking or Jira via MCP.

## Mode

Set to one of: `local` | `jira`

```
Mode: jira
Jira Cloud ID: stan-butler.atlassian.net
Jira Project Key: REEB
```

## Local mode (default)

When `Mode: local`, all task and bug tracking uses markdown files in `.ai/`:
- `.ai/tasks/backlog.md` — source of truth for pending tasks
- `.ai/tasks/active.md` — source of truth for current task
- `.ai/tasks/done.md` — source of truth for completed tasks
- `.ai/bugs/open.md` — source of truth for open bugs
- `.ai/bugs/resolved.md` — source of truth for resolved bugs

The Architect reads and writes directly to these files. The Developer reads from `active.md` and `bugs/open.md`.

## Jira mode

When `Mode: jira`, task and bug tracking uses Jira via MCP:
- All tasks and bugs are created/updated in Jira
- `.ai/tasks/active.md` is a **read-only snapshot** synced from Jira (updated by Architect after each Jira operation)
- `.ai/bugs/open.md` is a **read-only snapshot** synced from Jira (updated by Architect after each Jira operation)
- `.ai/tasks/backlog.md`, `done.md`, and other task files are archived and not used during normal operation
- The Developer reads from `active.md` and `bugs/open.md` (same as local mode)

### Jira configuration

When using Jira mode, provide the following:

```
Mode: jira
Jira Cloud ID: {your-site.atlassian.net or UUID}
Jira Project Key: {e.g., PROJ}
Jira Board ID: {board ID, optional — used for listing cards}
```

The Architect uses the MCP Jira server (via `https://mcp.atlassian.com/v1/mcp`) to:
- Create issues with type "Task" or "Bug"
- Update issue status and fields
- Search for and retrieve issues
- Add comments and worklogs

Primary MCP tools: `mcp0_createJiraIssue`, `mcp0_getJiraIssue`, `mcp0_searchJiraIssuesUsingJql`, `mcp0_editJiraIssue`, `mcp0_addCommentToJiraIssue` (completion reports as comments, `contentFormat: markdown`).

**MCP Configuration**: Add this to `.devin/config.json` (committed) or `.devin/mcp_config.local.json` (machine-local):

```json
{
  "mcpServers": {
    "atlassian-mcp-server": {
      "url": "https://mcp.atlassian.com/v1/mcp",
      "transport": "http"
    }
  }
}
```

**Authentication**: The first time you use a Jira MCP tool, a browser tab will open for you to authenticate with your Atlassian credentials. Your session is cached locally.

**Clearing authentication**: If you need to log in with a different Atlassian account or clear cached credentials, delete the contents of:
- **Windows**: `%USERPROFILE%\.mcp-auth`
- **macOS/Linux**: `~/.mcp-auth`

The next Jira MCP call will prompt you to authenticate again.

## Migration

See `.ai/workflows/migrate-to-jira.md` to switch from local to Jira mode.
See `.ai/workflows/migrate-from-jira.md` to switch from Jira back to local mode.

## Snapshot files

Regardless of mode, these files are always maintained:
- `.ai/tasks/active.md` — the current task (source of truth in local mode, snapshot in Jira mode)
- `.ai/bugs/open.md` — open bugs (source of truth in local mode, snapshot in Jira mode)

These files ensure the Developer never needs to know which mode is active. The Architect keeps them in sync.
