# Change: Add Agent AST for Creating Agent Files

## Why
Introduce a flavour-agnostic AST to model agent files, separating construction from output concerns. This change explicitly excludes any writer implementation or changes, and excludes DSL builders.

## What Changes
- Add a new capability spec: `agent-ast`
- Define immutable AST nodes for agent metadata, sections, lists, and imports
- Specify frontmatter as a `Map<string, obj>` carried by the AST
- Establish invariants for deterministic construction and traversal
- Clarify import representation: path-only (sourcePath + declared format); resolution is out of scope

## Impact
- Affected specs: `agent-ast`
- Affected code: `AST`
- Out of scope: writers and DSL builders
