# Change: Add top-level Agent DSL (F# CE) that outputs Agent AST

## Why
A concise, flavour-agnostic authoring experience is needed to define agents once and render to multiple ecosystems (OpenCode, Copilot, Claude). A top-level F# computation expression (CE) improves ergonomics while preserving stratified design by producing the existing Agent AST. Common frontmatter across ecosystems must be easy to express without coupling DSL to any single writer flavour.

## What Changes
- Add new capability spec `agent-dsl` defining a top-level CE (`agent { ... }`) that yields an Agent AST
- Provide DSL operations for core sections: `role`, `objective`, `instructions`, `context`, `examples`, `output`
- Add flavour-agnostic frontmatter builder in DSL for common keys (e.g., `description`, `model`, `temperature`, `tools`, optional `name`) — optional, no required keys enforced in DSL
- Add typed helpers for lists and objects in frontmatter: `kvList`, `kvObj`, `kvListObj` to express nested configs (e.g., MCP)
- Infer import format from filename extensions in DSL/AST helpers (`.yml/.yaml → yaml`, `.json → json`, `.toon → toon`); DSL `import` takes only `sourcePath`
- Support ad hoc metadata (`metaKv key value`) and ad hoc sections (`section name content`) to avoid restrictiveness (e.g., MCP-related keys)
- Extend `agent-ast` spec with helper constructors for frontmatter and import references to keep DSL pure and stratified
- Ensure deterministic section ordering and immutability are preserved

## Impact
- Affected specs: `agent-dsl` (ADDED), `agent-ast` (MODIFIED: construction helpers, import inference, and signatures)
- Affected code: `src/FsAgent/` (DSL CE surface), AST module helpers, writer compatibility tests
- Non-breaking: additive; existing AST and writers remain valid
