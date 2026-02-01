## Context

FsAgent is currently a single-file F# library (Library.fs, ~350 lines) that provides an immutable AST and DSL for building agent configuration files. The `Agent` type contains `Frontmatter: Map<string, obj>` and `Sections: Node list`, where sections like role, objective, and instructions are created through the `AgentBuilder` computation expression. This design conflates prompt content with agent configuration, preventing prompt reuse across multiple agents.

The current architecture has:
- All code in a single namespace (`FsAgent`) with modules: AST, DSL, MarkdownWriter
- Agent builder includes operations: `role`, `objective`, `instructions`, `context`, `output`, `examples`
- Node types: Text, Section, List, Imported (for data file imports)
- Writers support Opencode/Copilot formats with configurable options

This refactoring introduces a first-class Prompt type to enable composition and adds template support for dynamic content generation.

## Goals / Non-Goals

**Goals:**
- Separate prompt and agent concerns into distinct types with their own builders
- Enable prompt reusability: define once, reference in multiple agents
- Add template support with variable substitution using Fue library
- Organize code into domain-focused files and namespaces for better maintainability
- Maintain backward compatibility through re-export layer in Library.fs
- Follow existing patterns (Node types, computation expressions, writer configurability)

**Non-Goals:**
- Template rendering in builders (rendering happens at write time, not when loading prompt)
- Prompt registries or external prompt loading (prompts are F# values passed by reference)
- Skills, SlashCommands, or other artifact types (future work)
- Changing the immutable AST pattern or core Node structure
- Reader implementations (parsing markdown back to AST)

## Decisions

### Decision 1: Templates as Node Types (not special fields)

**Choice**: Add `Template of text: string` and `TemplateFile of path: string` as new Node cases, following the same pattern as `Imported` nodes.

**Alternatives Considered**:
- Special `Template` and `TemplateFile` fields on Prompt type → Rejected: Inconsistent with existing design; import already uses nodes
- Template as a Section with special handling → Rejected: Less type-safe, harder to distinguish from regular sections

**Rationale**: The existing `Imported` node demonstrates that file references and dynamic content belong in the Node type. Templates can be called multiple times, order matters (just like imports), and they're appended to the Sections list. This maintains consistency with the existing architecture.

### Decision 2: Prompt Structure Mirrors Agent

**Choice**: `Prompt = { Frontmatter: Map<string, obj>; Sections: Node list }` - identical structure to Agent.

**Alternatives Considered**:
- Explicit fields (Name, Description, Author, etc.) → Rejected: Verbose, breaks from existing Agent pattern
- Sections only (no frontmatter) → Rejected: Need metadata for identification and documentation

**Rationale**: Using the same structure simplifies the writer implementation (both can share serialization logic) and maintains consistency. Frontmatter stores metadata but won't be written as a YAML frontmatter block in prompt output - the writer handles this distinction.

### Decision 3: Domain-Focused File Organization

**Choice**: Split into `AST.fs` → `Prompt.fs` → `Agent.fs` → `Writers.fs` with compilation order dependency.

**Alternatives Considered**:
- Keep single Library.fs → Rejected: Growing complexity, poor separation of concerns
- Layer-focused (Types.fs, Builders.fs, Writers.fs) → Rejected: Splits related domain logic
- Fine-grained per-component files → Rejected: Over-engineering for current size

**Rationale**: Domain-focused organization groups related types, builders, and utilities together. AST is foundational (shared types), Prompt has no dependencies beyond AST, Agent depends on Prompt for composition, and Writers depend on all types. This maps cleanly to F# compilation order requirements.

### Decision 4: Namespace Organization with AutoOpen

**Choice**: Namespaces `FsAgent.AST`, `FsAgent.Prompts`, `FsAgent.Agents`, `FsAgent.Writers` with `[<AutoOpen>]` on builder modules.

**Alternatives Considered**:
- Flat namespace with module prefixes → Rejected: Doesn't scale, pollutes global namespace
- Builders in separate .Builder modules → Rejected: More verbose imports required

**Rationale**: Users can `open FsAgent.Prompts` and immediately access `prompt { ... }` builder via AutoOpen, while still being able to access `Prompt.role` function by opening the module. This provides both convenience and explicit access patterns.

### Decision 5: Template Rendering at Write Time

**Choice**: Templates render in the Writer when `writePrompt` or `writeAgent` is called, using `TemplateVariables` from Options.

**Alternatives Considered**:
- Render in builder when template is added → Rejected: Variables not known at build time, makes AST impure
- Separate render function called by user → Rejected: Extra step, error-prone
- Render at Agent creation when prompt is referenced → Rejected: Still too early for runtime variables

**Rationale**: Writers already handle IO (loading imported files), have error handling, and accept runtime configuration through Options. Template variables are runtime concerns (e.g., username, filename), so rendering must happen at serialization time, not AST construction time.

### Decision 6: Breaking Change in AgentBuilder

**Choice**: Remove `role`, `objective`, `instructions`, `context`, `output`, `examples` operations from AgentBuilder. Users must create prompts separately and reference them via `prompt` operation.

**Alternatives Considered**:
- Keep both inline and reference patterns → Rejected: Two ways to do the same thing, confusing API
- Deprecate gradually with warnings → Rejected: Complicates implementation, delays cleanup

**Rationale**: Clean break forces adoption of the new pattern and simplifies the codebase. The backward compatibility layer (FsAgent.DSL re-exports) provides migration path, and this is a pre-1.0 library where breaking changes are acceptable.

### Decision 7: Prompt Writer Behavior

**Choice**: `writePrompt` doesn't output frontmatter YAML blocks; frontmatter is stored internally but not serialized.

**Alternatives Considered**:
- Output frontmatter like agents → Rejected: Prompts are content, not configuration files
- Strip frontmatter entirely (no storage) → Rejected: Lose metadata for programmatic use

**Rationale**: Prompts are embedded in agents, not standalone configuration files. Storing metadata (name, description, author) enables programmatic inspection and composition, but outputting it as YAML would clutter the generated prompt content.

## Risks / Trade-offs

**[Risk] Breaking change disrupts existing users** → Mitigation: Provide clear migration guide, maintain FsAgent.DSL re-exports for backward compatibility, document the change prominently in release notes.

**[Risk] Four-file split increases cognitive overhead** → Mitigation: Each file is ~100-200 lines, focused on single concern. Domain organization is intuitive. Library.fs acts as re-export convenience layer.

**[Risk] Template rendering errors at write time** → Mitigation: Template module wraps Fue calls in try-catch, returns error messages in output (e.g., "[Template error: ...]") rather than throwing exceptions. Users see issues in generated output.

**[Risk] Namespace changes break existing imports** → Mitigation: Keep `FsAgent` namespace with DSL module for re-exports. Existing code using `open FsAgent; open FsAgent.DSL` continues to work unchanged.

**[Trade-off] Prompts and Agents have duplicate metadata operations** → Accepted: Both types support `name`, `description`, `author`, `version`, `license` operations in their builders. Code duplication is minor, and attempting to share would complicate the builder types.

**[Trade-off] Template node ordering matters (like imports)** → Accepted: This is a feature, not a bug. Users can control where templates appear in output, interleave with other sections. Documenting this behavior is sufficient.

**[Trade-off] Fue dependency adds external library** → Accepted: Fue is lightweight, stable, and provides Mustache-compatible syntax. Alternative would be implementing our own template engine (significant scope increase) or using string interpolation (less powerful).
