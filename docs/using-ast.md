# Using the AST

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
