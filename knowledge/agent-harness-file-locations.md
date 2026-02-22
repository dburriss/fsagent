# Agent Harness File Locations

A reference for where [[Claude Code]], [[GitHub Copilot]], and [[OpenCode]] store their agent harness files: slash commands, skills, and custom agents. All three tools converge on the open [Agent Skills standard](https://agentskills.io), making many paths cross-compatible.

## Claude Code

### Subagents
Markdown files with YAML frontmatter defining agent persona, tools, and instructions.

| Scope | Path |
|-------|------|
| User (global) | `~/.claude/agents/<name>.md` |
| Project | `.claude/agents/<name>.md` |

### Skills / Slash Commands
`SKILL.md` files inside a named subdirectory. Skills double as slash commands via the `/<name>` syntax. The legacy `.claude/commands/` path still works for slash commands.

| Scope | Path |
|-------|------|
| User (global) | `~/.claude/skills/<name>/SKILL.md` |
| Project | `.claude/skills/<name>/SKILL.md` |
| Legacy commands | `.claude/commands/<name>.md` |

### Settings
| Scope | Path |
|-------|------|
| User | `~/.claude/settings.json` |
| Project | `.claude/settings.json` |
| Local (not committed) | `.claude/settings.local.json` |
| Managed (macOS) | `/Library/Application Support/ClaudeCode/managed-settings.json` |
| Managed (Linux/WSL) | `/etc/claude-code/managed-settings.json` |

## GitHub Copilot

### Custom Agents
Markdown files with YAML frontmatter (agent profiles).

| Scope | Path |
|-------|------|
| Repository | `.github/agents/<agent-name>.md` |
| Org/Enterprise (global) | `/agents/<agent-name>.md` inside `.github-private` repo |

### Skills
Follows the open Agent Skills spec. Org/enterprise level skills are listed as "coming soon".

| Scope | Path |
|-------|------|
| Project | `.github/skills/<name>/SKILL.md` |
| Project (Claude-compatible) | `.claude/skills/<name>/SKILL.md` |
| Personal (global) | `~/.copilot/skills/<name>/SKILL.md` |
| Personal (Claude-compatible) | `~/.claude/skills/<name>/SKILL.md` |

### Prompt Files (Slash Commands)
Available in VS Code, Visual Studio, and JetBrains as `/<name>` commands.

| Scope | Path |
|-------|------|
| Repository | `.github/prompts/<name>.prompt.md` |

## OpenCode

### Agents
Markdown files with YAML frontmatter.

| Scope | Path |
|-------|------|
| Global | `~/.config/opencode/agents/<name>.md` |
| Project | `.opencode/agents/<name>.md` |

### Skills
`SKILL.md` files inside named subdirectories. OpenCode searches multiple paths in order, including Claude-compatible paths, making it interoperable with Claude Code.

| Priority | Scope | Path |
|----------|-------|------|
| 1 | Project | `.opencode/skills/<name>/SKILL.md` |
| 2 | Global | `~/.config/opencode/skills/<name>/SKILL.md` |
| 3 | Project (Claude-compat) | `.claude/skills/<name>/SKILL.md` |
| 4 | Global (Claude-compat) | `~/.claude/skills/<name>/SKILL.md` |
| 5 | Project (agent-compat) | `.agents/skills/<name>/SKILL.md` |
| 6 | Global (agent-compat) | `~/.agents/skills/<name>/SKILL.md` |

### Commands (Slash Commands)
Markdown files. Also configurable inline via `opencode.json` under the `"command"` key.

| Scope | Path |
|-------|------|
| Global | `~/.config/opencode/commands/<name>.md` |
| Project | `.opencode/commands/<name>.md` |

## Cross-Tool Compatibility

All three tools support the `.claude/skills/` path for skills, and both [[OpenCode]] and [[GitHub Copilot]] also support `~/.claude/skills/`. This means a skill written once can be used across all three tools without modification.

The shared specification is the open [Agent Skills standard](https://agentskills.io).

| Path | Claude Code | Copilot | OpenCode |
|------|-------------|---------|----------|
| `.claude/skills/<name>/SKILL.md` | Yes | Yes | Yes |
| `~/.claude/skills/<name>/SKILL.md` | Yes | Yes (CLI/coding agent) | Yes |
| `.opencode/skills/<name>/SKILL.md` | No | No | Yes |
| `.github/skills/<name>/SKILL.md` | No | Yes | No |
| `.agents/skills/<name>/SKILL.md` | No | No | Yes |

## Resources

- [OpenCode Skills](https://opencode.ai/docs/skills/)
- [OpenCode Commands](https://opencode.ai/docs/commands/)
- [OpenCode Agents](https://opencode.ai/docs/agents/)
- [Claude Code Settings](https://code.claude.com/docs/en/settings)
- [Claude Code Skills](https://code.claude.com/docs/en/skills)
- [Claude Code Sub-Agents](https://code.claude.com/docs/en/sub-agents#create-custom-subagents)
- [GitHub Copilot Custom Agents](https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-custom-agents#where-you-can-configure-custom-agents)
- [GitHub Copilot Agent Skills](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills)
- [GitHub Copilot Prompt Files](https://docs.github.com/en/copilot/tutorials/customization-library/prompt-files/your-first-prompt-file)
