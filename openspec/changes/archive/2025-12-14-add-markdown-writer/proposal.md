# Change: Add Markdown Writer with Configurable Options

## Why
A first writer is needed to convert the Agent AST to a Markdown format for distribution and documentation. Configuration is required to control how imported data is embedded and how headings are rendered/renamed.

## What Changes
- Add a `markdown-writer` capability to generate Markdown from the Agent AST
- Support output format selection for agent files: `opencode` and `copilot` (initially)
- Enforce ATX headings for all output (no Setext)
- Imported data inclusion: embed raw transformed content (no automatic codeblocks); authors can wrap content in codeblocks explicitly if desired
- Provide options to control heading naming/formatting transformations (rename map + optional global formatter with sensible default)
- Provide a .NET-style configuration API using a function that mutates an options object (familiar pattern similar to ASP.NET/JsonSerializer configuration)
- Provide an optional generated footer function with context that returns a string to append (default: none)
- Establish sensible defaults and ensure deterministic output; writer returns a string
- Default behavior includes frontmatter emission for supported formats
- Add extensibility: developers can register a custom agent writer (e.g., Claude) via options
- Support output type selection: `md | json | yaml`

## Impact
- Affected specs: `agent-ast` (consumed, not modified), new `markdown-writer`
- Affected code: Writers module (new), options types, tests for writer behavior

## References
- Imported data example pattern: `knowledge/import-data.md` (generated) uses rules from `knowledge/import-data.rules.json`
- Copilot minimum frontmatter: `name` and `description` (see GitHub Copilot Agents docs)
- OpenCode frontmatter: `description` only (see opencode docs)

## Open Questions
1. Copilot format specifics beyond `name` and `description`: confirm any additional required sections or ordering.
2. Version markers or metadata: clarify desired default footer content if any (option provided as function; default none).
