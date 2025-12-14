# fsagent
A small DSL and library for generating custom agent files for popular agent tools

Provides an immutable AST for representing agent files, with constructors for common sections like role, objective, instructions, etc.

## Markdown Writer

The library includes a Markdown writer to convert the Agent AST to Markdown strings with configurable options.

### Example

```fsharp
open FsAgent

let agent = {
    Frontmatter = Map.ofList ["description", "A helpful assistant" :> obj]
    Sections = [
        AST.role "You are a helpful assistant"
        AST.objective "Assist users with coding tasks"
    ]
}

let markdown = MarkdownWriter.writeMarkdown agent id
```

## DSL

The library provides a top-level F# computation expression for authoring agents.

### Example

```fsharp
open FsAgent.DSL

let agent = agent {
    meta (meta {
        kv "description" "A helpful assistant"
        kv "model" "gpt-4"
        kvList "tools" ["grep"; "bash"]
    })
    role "You are a helpful assistant"
    objective "Assist users with coding tasks"
    instructions "Follow these steps..."
    examples [
        example "How to build?" "Run dotnet build"
    ]
}

let markdown = MarkdownWriter.writeMarkdown agent id
```

### Options

- `OutputFormat`: `Opencode` (default) or `Copilot`
- `OutputType`: `Md` (default), `Json`, or `Yaml`
- `ImportInclusion`: `None` (default) or `Raw`
- `RenameMap`: Map for renaming section headings
- `HeadingFormatter`: Optional function to format headings
- `GeneratedFooter`: Optional function to generate footer content
- `IncludeFrontmatter`: Whether to include frontmatter (default true)
- `CustomWriter`: Optional custom writer function

See `knowledge/import-data.md` for an example of generated output with imported data rules from `knowledge/import-data.rules.json`.

## Build & Test

```bash
dotnet build                    # Build project
dotnet test                     # Run unit tests
```

## Using OpenSpec

This project uses OpenSpec for spec-driven development. OpenSpec manages change proposals, specifications, and ensures structured development.

### Essential Commands

```bash
openspec list                  # List active changes
openspec list --specs          # List specifications
openspec show [item]           # Display change or spec details
openspec validate [item]       # Validate changes or specs
openspec archive <change-id> [--yes|-y]   # Archive completed changes
```

For planning changes or new features, create proposals in `openspec/changes/`. See `openspec/AGENTS.md` for detailed instructions.
