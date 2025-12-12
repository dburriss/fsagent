## Context
Introduce an immutable, flavour-agnostic AST for agent files that cleanly separates construction (DSL) from rendering (writers). The AST must carry frontmatter and import references (path + declared format) without parsed objects or format-specific logic; writers resolve imports via lower-level IO/parsing modules.

## Goals / Non-Goals
- Goals: stable core model; deterministic rendering; late-bound import serialization; writer independence
- Non-Goals: commit to a single output format; introduce IO inside writers or DSL

## Decisions
- AST nodes: Text, Block, Section, List, Imported, Agent (root)
- Frontmatter stored as `Map<string, obj>` attached to root Agent
- Imported node retains parsed object + declared `DataFormat` (yaml|json|toon)
- Writers (Markdown, JSON) read the AST and choose import serialization at write time
- Flavour writers wrap base writers to adjust frontmatter and layout only

## Risks / Trade-offs
- Risk: Overly complex AST
  - Mitigation: keep node set minimal and immutable; avoid nested polymorphism unless needed
- Risk: Writer divergence across formats
  - Mitigation: define common writer interface and invariants against AST traversal

## Migration Plan
- Implement AST and baseline writers behind existing DSL functions
- Update tests to assert deterministic shapes and outputs
- Keep DSL signatures stable; only enhance internals to target the AST

## Open Questions
- Do we need additional node kinds (e.g., CodeFence, Table) in v1?
- Should frontmatter allow nested maps/arrays or remain flat initially?
