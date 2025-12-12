## Context
Introduce an immutable, flavour-agnostic AST for agent files that cleanly separates construction from any rendering concerns. The AST must carry frontmatter and import references (path + declared format) without parsed objects or format-specific logic. Resolution of imports and output formatting is explicitly out of scope for this change. DSL builders are also out of scope for this change.

## Goals / Non-Goals
- Goals: stable core model; deterministic construction and traversal; path-only import references
- Non-Goals: implement or modify writers; implement or modify DSL builders; commit to specific output formats; introduce IO

## Decisions
- AST nodes: Text, Block, Section, List, Imported, Agent (root)
- Frontmatter stored as `Map<string, obj>` attached to root Agent
- Imported node retains `sourcePath` + declared `DataFormat` (yaml|json|toon); no parsed object inside AST
- AST is immutable; nodes are not mutated during traversal
- Node kinds `CodeFence` and `Table` are explicitly out of scope for v1
- Frontmatter supports nested maps/arrays (not flat-only)
- Expose value constructors in `AST` module: `role: string -> Section`, `objective: string -> Section`, `instructions: string -> Section`, `context: string -> Section`, `output: string -> Section`, `example: Section list -> Section`, `examples: Section list -> Section` (pure, non-mutating)


## Risks / Trade-offs
- Risk: Overly complex AST
  - Mitigation: keep node set minimal and immutable; avoid nested polymorphism unless needed

## Migration Plan
- Implement AST types
- Add tests for AST construction invariants and deterministic traversal

## Open Questions
- None at this time
