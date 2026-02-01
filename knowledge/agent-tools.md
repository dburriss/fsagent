# Using Tools in Markdown Agent Files

## Overview

Tools control what actions agents can perform in OpenCode. They determine which capabilities are available to an agent during execution. Tools are configured in the agent frontmatter and can be enabled or disabled using boolean values.

## Tool Configuration Syntax

### Markdown Format

Tools are specified in the YAML frontmatter of markdown agent files:

```markdown
---
tools:
  bash: false
  write: false
  edit: true
  webfetch: true
---

# Agent Role

Your agent content here...
```

### JSON Format

In JSON agent configuration files, tools are nested within the agent object:

```json
{
  "agent": {
    "plan": {
      "tools": {
        "write": false,
        "bash": false
      }
    }
  }
}
```

## Available Tools

### Core Tools

- **`write`** - File creation and writing operations. Allows the agent to create new files or overwrite existing ones.
- **`edit`** - Code modification and patching operations. Enables the agent to make surgical edits to existing files.
- **`bash`** - System command execution. Grants access to shell commands and scripts.
- **`webfetch`** - Web content retrieval. Allows the agent to fetch and process web resources.
- **`todo`** - Task management capabilities. Available with full access on General subagents.

### Tool Values

- `true` - Enable the tool for this agent
- `false` - Disable the tool for this agent
- Omitting a tool inherits the default/global setting

## Common Patterns

### Read-Only Agent

For analysis or review agents that should not modify code:

```markdown
---
tools:
  write: false
  edit: false
  bash: false
---
```

### Research Agent

For agents that need to fetch information but not execute code:

```markdown
---
tools:
  webfetch: true
  bash: false
  write: false
---
```

### Full-Access Implementation Agent

For agents that need all capabilities:

```markdown
---
tools:
  write: true
  edit: true
  bash: true
  webfetch: true
---
```

## Advanced Configuration

### Wildcard Patterns

Use glob patterns to manage groups of tools, particularly useful for MCP server tools:

```markdown
---
tools:
  mymcp_*: false  # Disable all tools from 'mymcp' server
  othermcp_*: true  # Enable all tools from 'othermcp' server
---
```

## Tools vs Permissions

**Important distinction:**

- **Tools** determine what capabilities are *available* to the agent
- **Permissions** (`ask`, `allow`, `deny`) control *approval requirements* before tool execution

Both can be combined for fine-grained control:

```markdown
---
tools:
  bash: true  # Tool is available
permissions:
  bash: ask   # But requires user approval
---
```

## Best Practices

1. **Principle of Least Privilege**: Only enable tools that the agent actually needs for its specific role
2. **Specialize Agents**: Use tool restrictions to create focused, specialized agents
3. **Override Defaults**: Agent-level tool settings override global configurations
4. **Document Restrictions**: Comment why certain tools are disabled for clarity
5. **Combine with Permissions**: Use both tools and permissions for security-sensitive operations

## Example: Code Review Agent

```markdown
---
name: code-reviewer
description: Reviews code for quality and best practices
tools:
  write: false      # Cannot modify code
  edit: false       # Cannot make changes
  bash: false       # Cannot run tests
  webfetch: true    # Can look up documentation
---

# Role

You are a code review specialist who analyzes code for quality,
maintainability, and adherence to best practices.

# Instructions

- Read and analyze code thoroughly
- Identify potential issues and improvements
- Provide constructive feedback
- Reference external documentation when helpful
- Never modify the code directly
```

## Example: Implementation Agent

```markdown
---
name: feature-implementor
description: Implements new features and fixes bugs
tools:
  write: true
  edit: true
  bash: true
  webfetch: true
---

# Role

You are a software engineer who implements features and fixes bugs.

# Instructions

- Read requirements carefully
- Write clean, tested code
- Run tests to verify changes
- Follow project conventions
```

## References

- [OpenCode Agent Documentation](https://opencode.ai/docs/agents/)
- Related: See `agents.md` for general agent structure
- Related: See `copilot-agent-config.md` for Copilot-specific configuration
