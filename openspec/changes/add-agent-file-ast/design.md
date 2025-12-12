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

## Risks / Trade-offs
- Risk: Overly complex AST
  - Mitigation: keep node set minimal and immutable; avoid nested polymorphism unless needed

## Migration Plan
- Implement AST types
- Add tests for AST construction invariants and deterministic traversal

## Open Questions
- Do we need additional node kinds (e.g., CodeFence, Table) in v1?
- Should frontmatter allow nested maps/arrays or remain flat initially?
