# GitHub Copilot — Custom Agents Configuration (Summary)

## YAML Frontmatter Properties
- `name` (string, optional) — display name.
- `description` (string, required) — agent purpose/capabilities.
- `target` (string, optional) — `vscode` or `github-copilot`; defaults to both.
- `tools` (string or list of strings, optional) — tool allowlist; omit or set `"*"` for all.
- `infer` (boolean, optional) — allow auto‑selection by Copilot coding agent; defaults `true`.
- `mcp-servers` (object, optional) — additional MCP servers and tool namespaces.
- `metadata` (object of string→string, optional) — arbitrary key/value annotations.

Notes
- Frontmatter is YAML at top of `.md` or `.agent.md` files; Markdown prompt body follows (max ~30k chars).
- `model`, `argument-hint`, and `handoffs` may be ignored by GitHub.com coding agent.
- Deduplication and precedence: repository overrides organization; organization overrides enterprise.

## Tools
- Omit `tools` or set `tools: ["*"]` to enable all available tools (built‑in + allowed MCP).
- Provide specific tools: `tools: ["read", "edit", "search"]`.
- Disable all tools: `tools: []`.
- Namespacing: `server/tool` or `server/*` (e.g., `github/*`, `playwright/*`).
- Unknown tool names are ignored safely.

### Tool Aliases (common)
- `execute` — aliases: `shell`, `bash`, `powershell` — runs commands.
- `read` — read file contents.
- `edit` — edit files (exact args vary by environment).
- `search` — search files and contents.
- `agent` — invoke another custom agent.
- `web` — `WebSearch`, `WebFetch` (not applicable for coding agent today).
- `todo` — `TodoWrite` (VS Code only; not coding agent today).

### Out‑of‑the‑box MCP servers
- `github` — `github/*` read‑only tools scoped to repo token.
- `playwright` — `playwright/*` tools with localhost access; token scoped to source repo.

## MCP Servers (Configuration)
- Levels: org/enterprise profiles can embed `mcp-servers`; repository profiles cannot embed but can use tools configured in repository settings.
- Type mapping: `stdio` maps to `local` for compatibility.
- Env/secrets: values must be configured in the Copilot environment (repository settings). Supported references:
  - `$NAME`, `${NAME}` (Claude/VS Code styles) — env + header.
  - `${{ secrets.NAME }}`, `${{ var.NAME }}` — env + header via Copilot environment.

### Example (org‑level agent with MCP)
```yaml
---
name: my-custom-agent-with-mcp
description: Custom agent description
tools: ["tool-a", "tool-b", "custom-mcp/tool-1"]
mcp-servers:
  custom-mcp:
    type: local
    command: some-command
    args: ["--arg1", "--arg2"]
    tools: ["*"]
    env:
      ENV_VAR_NAME: ${{ secrets.MY_SECRET }}
---

You are a helpful agent...
```

## Processing Rules
- Tools:
  - Omitted → all enabled
  - Empty list → none enabled
  - Specific list → only listed tools enabled
- MCP override order: built‑in MCP → custom agent MCP (org/enterprise) → repository MCP JSON; later layers override earlier.

## Quick Examples
Enable all tools
```yaml
---
name: testing-specialist
description: Focused on tests
---
```

Enable specific tools
```yaml
---
name: implementation-planner
description: Creates implementation plans
tools: ["read", "search", "edit"]
---
```

Reference MCP server tools
```yaml
---
name: repo-helper
description: Uses GitHub MCP tools
tools: ["github/*", "github/issues-read"]
---
```

## Practical Tips
- Use namespacing to selectively expose MCP tools: `server/tool`.
- Keep `metadata` for annotations and routing; writers or orchestration can use it.
- Keep frontmatter minimal; rely on repository/org/enterprise MCP configuration where possible.
