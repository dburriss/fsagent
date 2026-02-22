# FsAgent — Project Overview

## Project Overview

FsAgent is a small, composable F# DSL and library for generating agent definition files for popular AI agent tools. It lets you define agents, prompts, skills, and slash commands once in type-safe F# code, then render them into the formats required by different harnesses (OpenCode, GitHub Copilot, ClaudeCode).

The design is deliberately minimal: a stratified DSL → AST → Writer pipeline similar in spirit to Giraffe.ViewEngine. Agent definitions remain flavour-agnostic; harness-specific details are handled entirely by writers.

**NuGet package:** `FsAgent`  
**Target framework:** .NET 10  
**Language:** F#

---

## Goals

1. **Harness portability** — define an agent once, render it for OpenCode, Copilot, or ClaudeCode without changing the definition.
2. **Composability** — prompts, skills, and commands are first-class values that can be referenced and reused across multiple agents.
3. **Correctness by construction** — type-safe DSL prevents invalid configurations (e.g., unknown tools, malformed frontmatter) at compile time.
4. **Pure construction, late-bound IO** — the DSL and AST are pure; all file I/O, template rendering, and import resolution happen at write time.
5. **Minimal surface area** — keep the API orthogonal and predictable; resist feature creep that bleeds harness-specific concerns into the DSL layer.

**Key constraints:**
- DSL must never depend on output formats.
- Writers must never mutate the AST.
- Rendering must be deterministic.
- No runtime network dependencies.

---

## Key Features

| Feature | Description |
|---|---|
| **`prompt { }` builder** | Define reusable prompt content (role, objective, instructions, examples, imports) independently of any agent |
| **`agent { }` builder** | Compose agents from prompts + configuration metadata (model, temperature, tools, frontmatter) |
| **`skill { }` builder** | Define skills in `SKILL.md` format, referenced by agents |
| **`command { }` builder** | Define slash commands with description frontmatter |
| **Typed tools** | `Tool` discriminated union with compile-time safety; harness-specific name mapping at write time |
| **Template rendering** | Fue-based `{{{variable}}}` interpolation; `{{{tool Name}}}` resolves to the correct harness tool name |
| **Import pipeline** | `import "path"` (code-block wrapped) and `importRaw "path"` (raw embed); YAML/JSON/TOON re-serialization at write time |
| **Multi-harness output** | `OutputFormat`: `Opencode` (struct tools, YAML frontmatter), `Copilot` / `ClaudeCode` (list tools) |
| **Multi-format output** | `OutputType`: `Md` (default), `Json`, `Yaml` |
| **XML section style** | `SectionStyle.Xml` renders sections as XML tags instead of Markdown headings |
| **File writer** | `FileWriter` module resolves harness-correct output paths and writes files to disk |
| **Backward compatibility** | `open FsAgent.DSL` still works via re-exports in `Library.fs` |

---

## Roadmap

### Completed milestones

| Version | Date | Summary |
|---|---|---|
| 0.1.0 | Dec 2025 | Initial release: `agent { }` DSL, `Node` AST, Markdown writer, import inference |
| 0.2.0 | Feb 2026 | `prompt { }` CE, `Template`/`TemplateFile` nodes, Fue templating, namespace split, `renderPrompt` |
| 0.3.0 | Feb 2026 | Typed `Tool` DU, `ClaudeCode` harness, `AgentHarness` rename, `FsAgent.Tools` namespace |
| 0.3.1 | Feb 2026 | Struct/map tool output format for Opencode; alphabetical tool ordering; disabled tools as `false` |
| Unreleased | Feb 2026 | `skill { }` / `command { }` CEs, `renderSkill` / `renderCommand`, `{{{tool X}}}` injection, `SectionStyle.Xml`, `FileWriter`, field validation |

### Near-term direction

- **Readers API** — Parse existing agent files back into FsAgent structures, enabling harness-to-harness conversion (e.g., read an Opencode agent, write it as a Copilot agent).
- **`ConfigPaths` module** — OS-aware, harness-specific path resolution for project-scoped and user-global agent directories (partially addressed by `FileWriter`).
- **Composable writer hierarchy** — A possible `IWriter` interface pattern where harness-specific writers wrap a base format writer, creating a clean (harness × format) matrix without branching. Deferred post-1.0; current `Options`-based dispatch is sufficient.

---

## Architecture

FsAgent uses a strict four-layer stratified design. Full details are in [ARCHITECTURE.md](ARCHITECTURE.md).

```
DSL Layer
  prompt { } / agent { } / skill { } / command { }
  Pure F# computation expressions. No IO. No format knowledge.
        |
        v
Domain Types (AST)
  Prompt, Agent, Skill, SlashCommand
  Node DU: Text, Section, List, Imported, Template, TemplateFile
  Immutable, flavour-agnostic intermediate representation.
        |
        v
Writers (Output Layer)
  AgentWriter: renderAgent, renderPrompt, renderSkill, renderCommand
  Dispatches on OutputType (Md/Json/Yaml) and OutputFormat (Opencode/Copilot/ClaudeCode).
  Performs IO for imports and templates. Never mutates domain types.
        |
        v
Low-Level Utilities
  File reader, YAML/JSON parsers, TOON serializer, Fue template engine.
  No knowledge of writers or domain types.
```

### Key modules

| Module | Namespace | Responsibility |
|---|---|---|
| `AST.fs` | `FsAgent.AST` | `Node` DU, `DataFormat` enum, AST helpers |
| `Prompt.fs` | `FsAgent.Prompts` | `Prompt` type, `PromptBuilder` CE |
| `Agent.fs` | `FsAgent.Agents` | `Agent` type, `AgentBuilder`, `MetaBuilder` CEs |
| `Writers.fs` | `FsAgent.Writers` | `AgentWriter` module (all render functions + `Options`) |
| `Library.fs` | `FsAgent` | Backward-compatibility re-exports |

### Harness support matrix

| Capability | Opencode | Copilot | ClaudeCode |
|---|---|---|---|
| Agents | `~/.config/opencode/agents/` | `.github/` | `~/.claude/agents/` |
| Skills | `~/.config/opencode/skills/` | `.github/` | `~/.claude/commands/` |
| Commands | `~/.opencode/commands/` | `.github/` | `~/.claude/commands/` |
| Tool format | struct (`bash: true`) | list (`- bash`) | list (`- Bash`) |

---

## Development

```bash
dotnet build    # Build
dotnet test     # Run tests
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for extension points (adding node types, output formats, prompt operations).  
See [CHANGELOG.md](CHANGELOG.md) for migration guides and detailed version history.
