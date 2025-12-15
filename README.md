# fsagent
A small DSL and library for generating custom agent files for popular agent tools

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

For lower-level usage using the AST directly, see [Using the AST](docs/using-ast.md).

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
