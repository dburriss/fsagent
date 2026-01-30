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
- `DisableCodeBlockWrapping`: Force raw output even for `import` (default false)
- `RenameMap`: Map for renaming section headings
- `HeadingFormatter`: Optional function to format headings
- `GeneratedFooter`: Optional function to generate footer content
- `IncludeFrontmatter`: Whether to include frontmatter (default true)
- `CustomWriter`: Optional custom writer function

See `knowledge/import-data.md` for an example of generated output with imported data rules from `knowledge/import-data.rules.json`.

## Importing Data

The DSL provides two operations for importing external files:

- `import "path"` - Wraps content in a fenced code block (e.g., ` ```json ... ``` `)
- `importRaw "path"` - Embeds content directly without wrapping

```fsharp
let agent = agent {
    role "Data processor"
    import "config.json"      // Wraps in ```json ... ```
    importRaw "inline.txt"    // Embeds directly
}

// Default: imports are resolved with code blocks respected
let markdown = MarkdownWriter.writeMarkdown agent (fun _ -> ())

// Force all imports to raw (no code blocks)
let rawMarkdown = MarkdownWriter.writeMarkdown agent (fun opts ->
    opts.DisableCodeBlockWrapping <- true)
```

## Toon import example

The script `examples/toon.fsx` demonstrates building an agent that pulls structured lore from `examples/toon-data.toon` and writes `examples/toon-agent.md`. It runs the DSL-defined agent through `MarkdownWriter.writeMarkdown` so the generated sheet includes the catalog contents directly. This shows how to keep TOON catalogs external while still serializing them into final Markdown on demand.
