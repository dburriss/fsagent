# Change: Add Agent AST for Creating Agent Files

## Why
Introduce a flavour-agnostic AST to model agent files so writers can render Markdown/JSON variants consistently without leaking format concerns into the DSL.

## What Changes
- Add a new capability spec: `agent-ast`
- Define immutable AST nodes for agent metadata, sections, lists, and imports
- Specify frontmatter as a `Map<string, obj>` carried by the AST
- Establish invariants for deterministic construction and traversal
- Clarify import representation (parsed object + original format)
- Align writer expectations: writers operate on AST only

## Impact
- Affected specs: `agent-ast`
- Affected code: `DSL`, `AST`, `Writers`, `Import/Serialization`
- No external API breakage anticipated; adds core model used by writers
