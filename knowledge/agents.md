# Custom Agent Markdown Files — Claude, OpenCode, Copilot

## Naming model (key difference)

| System   | Agent name source          | Frontmatter | Notes                                   |
| -------- | -------------------------- | ----------- | --------------------------------------- |
| OpenCode | **Markdown file name**     | ✅ Yes       | Frontmatter exists, but no `name` field |
| Copilot  | **`name:` in frontmatter** | ✅ Yes       | File name irrelevant                    |
| Claude   | **`name:` in frontmatter** | ✅ Yes       | File name irrelevant                    |

---

## OpenCode (sst / opencode.ai)

**Naming**

* **Agent name = markdown file name**
* `agents/review.md` → agent name `review`
* Frontmatter **does not define the name**

**Example**

```markdown
---
description: Review code for correctness and risk
model: gpt-4.1
temperature: 0.2
tools:
  - grep
  - bash
---

You are a senior code review agent.
Focus on bugs, edge cases, and architectural risks.
```

Invoked as:

```
@review
```

**Common frontmatter settings**

* `description`: agent purpose
* `model`: model selection
* `temperature`: sampling control
* `tools`: allowed tools
* `max_steps`, `permissions`, etc. (depending on runtime)

**Key constraints**

* No `name:` field
* Renaming the file renames the agent
* File system = identity boundary

**Docs**

* [https://opencode.ai/docs/agents/](https://opencode.ai/docs/agents/)

---

## GitHub Copilot — Custom Agents

**Naming**

* **Agent name comes from `name:` in YAML frontmatter**
* File name is arbitrary

**Location**

```
.github/agents/*.md
```

**Example**

```markdown
---
name: readme-writer
description: Creates and updates README files
tools:
  - grep
  - bash
prompt: |
  You are a technical documentation expert.
---
```

**Common settings**

* `name` (required)
* `description` (required)
* `prompt`
* `tools`
* `mcp-servers`

**Docs**

* [https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-custom-agents](https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-custom-agents)

---

## Claude Code — Subagents

**Naming**

* **Agent name comes from `name:` in YAML frontmatter**
* File name does not affect identity

**Locations**

```
.claude/agents/
~/.claude/agents/
```

**Example**

```markdown
---
name: code-reviewer
description: Reviews code for bugs and design issues
model: claude-sonnet-4
tools:
  - read
  - grep
---
```

**Common settings**

* `name`
* `description`
* `model` (support evolving)
* `tools`
* Optional UI metadata

**Docs**

* [https://code.claude.com/docs/en/sub-agents](https://code.claude.com/docs/en/sub-agents)

---

## Key takeaway (important for tooling)

* **OpenCode**: identity = *file name*
* **Copilot / Claude**: identity = *frontmatter metadata*
* All three use Markdown + YAML, but **identity semantics differ**

If you want, I can:

* Produce **canonical templates** for all three
* Or help design a **portable agent DSL** that compiles to each format correctly

