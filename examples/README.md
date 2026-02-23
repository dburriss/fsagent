# FsAgent Examples

This directory contains example scripts demonstrating FsAgent features with the typed tools system and DSL computation expressions.

## Running Examples

All examples are F# scripts (.fsx) that can be run with:

```bash
# From the repository root
dotnet fsi examples/<example-folder>/<script-name>.fsx
```

## Examples

### [agents-md](./agents-md/)

Demonstrates creating a minimal `AGENTS.md` file template using the `command` computation expression.

**Features:** Reusable prompts, command definitions, project orientation templates.

**Run it:**
```bash
dotnet fsi examples/agents-md/agents-md.fsx
```

### [ck-skill](./ck-skill/)

Demonstrates creating a comprehensive skill that teaches an agent how to use `ck` (semantic code search).

**Features:** Detailed skill sections, setup instructions, usage examples, workflow guidance, template variables.

**Run it:**
```bash
dotnet fsi examples/ck-skill/ck-skill.fsx
```

### [opencode-skill-factory](./opencode-skill-factory/)

Demonstrates the FileWriter convenience API for generating and writing multiple artifacts to `.opencode`.

**Features:** FileWriter API, batch artifact generation, skills orchestration, sub-agent patterns.

**Run it:**
```bash
dotnet fsi examples/opencode-skill-factory/opencode-skill-factory.fsx
```

### [toon](./toon/)

Demonstrates a complete agent definition with imports, metadata, custom tools, and custom footers.

**Features:** File imports, custom metadata, custom tools, footer generation, complete agent configuration.

**Run it:**
```bash
dotnet fsi examples/toon/toon.fsx
```

### [typed-tools](./typed-tools/)

Demonstrates type-safe tool configuration and harness-specific output mapping.

**Features:** Typed tools with autocomplete, disallowed tools, harness-specific mapping (Opencode, ClaudeCode, Copilot), tool name conventions.

**Run it:**
```bash
dotnet fsi examples/typed-tools/typed-tools.fsx
```

### [xml-sections](./xml-sections/)

Demonstrates rendering prompts with XML section tags instead of Markdown headings.

**Features:** SectionStyle.Xml, XML tag rendering, custom tag name mapping for structured prompts.

**Run it:**
```bash
dotnet fsi examples/xml-sections/xml-sections.fsx
```

## Quick Reference

### Computation Expressions

```fsharp
open FsAgent.Agents
open FsAgent.Commands
open FsAgent.Skills

// Agent: combine metadata and prompt
let myAgent = agent { ... }

// Command: slash command with prompt
let myCommand = command { ... }

// Skill: reusable domain knowledge
let mySkill = skill { ... }
```

### Built-in Tools

```fsharp
open FsAgent.Tools

tools [
    Write      // File writing
    Edit       // File editing
    Bash       // Shell commands
    Read       // File reading
    Glob       // File pattern matching
    WebFetch   // HTTP fetching
    Todo       // Task management
    Custom "x" // Custom tool
]
```

### Harness-Specific Mapping

| Tool | Opencode | Copilot | ClaudeCode |
|------|----------|---------|------------|
| Write | write | write | Write |
| Edit | edit | edit | Edit |
| Bash | bash | bash | Bash |

## More Information

- [CLAUDE.md](../CLAUDE.md) - Project architecture and API reference
- [MIGRATION.md](../MIGRATION.md) - Migration guide from v1.x
- [CHANGELOG.md](../CHANGELOG.md) - Version history and breaking changes
