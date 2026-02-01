## Why

FsAgent currently combines prompt and agent concerns in a single type and builder, limiting reusability and composability. Prompts (role, objective, instructions) are tied to agents, preventing them from being defined once and reused across multiple agents or workflows. This change introduces Prompt as a first-class type with its own builder, enabling modular prompt design, template support with variable substitution, and cleaner separation between prompt content and agent configuration.

## What Changes

- Add `Prompt` type as a first-class entity (similar structure to Agent: Frontmatter + Sections)
- Add `prompt { ... }` computation expression builder for creating prompts
- Extend `Node` discriminated union with `Template` and `TemplateFile` cases for template support
- Integrate Fue template library for variable substitution in prompts
- **BREAKING**: Remove `role`, `objective`, `instructions`, `context`, `output`, `examples` operations from `AgentBuilder`
- Add `prompt` operation to `AgentBuilder` to reference Prompt instances
- Split `Library.fs` into domain-focused files: `AST.fs`, `Prompt.fs`, `Agent.fs`, `Writers.fs`
- Organize code into namespaces: `FsAgent.AST`, `FsAgent.Prompts`, `FsAgent.Agents`, `FsAgent.Writers`
- Add `writePrompt` function to MarkdownWriter (prompts don't output frontmatter blocks)
- Move prompt-specific constructors (`role`, `objective`, etc.) from AST module to Prompt module

## Capabilities

### New Capabilities
- `prompt-builder`: Computation expression for building reusable prompts with role, objective, instructions, context, output, examples, and template support
- `template-rendering`: Template/TemplateFile node types with Fue-based variable substitution at write time
- `prompt-writer`: Serialization of prompts to markdown/JSON/YAML without frontmatter blocks
- `namespace-organization`: Domain-focused file and namespace structure with AutoOpen builders

### Modified Capabilities
- `agent-builder`: Agent builder now references prompts instead of defining prompt content inline; breaking change removes prompt-related operations
- `ast-nodes`: Node DU extended with Template and TemplateFile cases; prompt constructors moved to Prompt module
- `markdown-writer`: Extended to handle Template/TemplateFile nodes and writePrompt function

## Impact

**Code:**
- `src/FsAgent/Library.fs` - Split into 4 separate files (AST.fs, Prompt.fs, Agent.fs, Writers.fs)
- `src/FsAgent/FsAgent.fsproj` - Add Fue NuGet dependency, update compilation order

**APIs:**
- **BREAKING**: `agent { role "..." objective "..." }` no longer valid; must use `prompt { ... }` then `agent { prompt promptInstance }`
- New: `FsAgent.Prompts` namespace with `prompt { ... }` builder
- New: `FsAgent.Agents` namespace (agents moved from `FsAgent.DSL`)
- New: `FsAgent.Writers.MarkdownWriter.writePrompt` function
- Backward compatibility layer via `FsAgent.DSL` re-exports

**Dependencies:**
- Add: Fue 2.2.0 for template rendering

**Tests:**
- New test files: PromptTests.fs, TemplateTests.fs, AgentPromptIntegrationTests.fs
- Update existing tests: AstTests.fs, DslTests.fs, MarkdownWriterTests.fs
- Tests follow A/B/C organization (Acceptance/Building/Communication)
