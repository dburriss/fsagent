# fsagent
A small DSL and library for generating custom agent files for popular agent tools

## Features

- **Prompt-first design**: Define reusable prompts with `prompt { ... }` builder
- **Template support**: Dynamic content generation with Fue templating (`{{{variable}}}` syntax)
- **Agent composition**: Reference prompts in agents for modular, reusable configurations
- **Multiple output formats**: Markdown, JSON, YAML
- **Type-safe builders**: F# computation expressions for ergonomic authoring

## Quick Start

### Creating Reusable Prompts

```fsharp
open FsAgent.Prompts

let assistantPrompt = prompt {
    role "You are a helpful coding assistant"
    objective "Help users with F# development tasks"
    instructions "Provide clear, concise code examples"
    examples [
        Prompt.example "How to build?" "Run `dotnet build` from the project root"
        Prompt.example "How to test?" "Run `dotnet test` to execute all tests"
    ]
}
```

### Building Agents with Prompts

```fsharp
open FsAgent.Agents
open FsAgent.Writers
open FsAgent.Tools

let codingAgent = agent {
    name "FSharp-Assistant"
    description "An AI assistant for F# development"
    model "gpt-4"
    temperature 0.7
    tools [Read; Custom "search"; Edit]

    prompt assistantPrompt  // Reference the prompt
}

let markdown = MarkdownWriter.writeAgent codingAgent (fun _ -> ())
```

### Using Templates

```fsharp
open FsAgent.Prompts
open FsAgent.Writers

let greetingPrompt = prompt {
    role "You are a friendly greeter"
    template "Hello {{{userName}}}, welcome to {{{appName}}}!"
}

let output = MarkdownWriter.writePrompt greetingPrompt (fun opts ->
    opts.TemplateVariables <- Map.ofList [
        ("userName", "Alice" :> obj)
        ("appName", "FsAgent" :> obj)
    ])
// Output: Hello Alice, welcome to FsAgent!
```

For lower-level usage using the AST directly, see [Using the AST](docs/using-ast.md).

## API Overview

### Prompt Builder

Create reusable prompt content:

```fsharp
prompt {
    // Metadata (stored but not output)
    name "my-prompt"
    description "A helpful prompt"
    author "Your Name"

    // Content sections
    role "You are..."
    objective "Your goal is..."
    instructions "Follow these steps..."
    context "In this scenario..."
    output "Format responses as..."

    // Templates
    template "Dynamic content: {{{variable}}}"
    templateFile "path/to/template.txt"

    // Imports
    import "config.json"      // Wrapped in code block
    importRaw "inline.txt"    // Raw content

    // Custom sections
    section "custom-section" "Custom content"
}
```

### Agent Builder

Build agents referencing prompts:

```fsharp
agent {
    // Metadata (output in frontmatter)
    name "my-agent"
    description "Agent description"
    model "gpt-4"
    temperature 0.7
    maxTokens 2000.0

    // Tool configuration
    tools [Write; Edit; Read]                   // Type-safe tool list
    disallowedTools [Bash; Write]               // Disable specific tools

    // Reference prompts
    prompt myPrompt1
    prompt myPrompt2  // Sections are merged

    // Direct sections
    section "notes" "Additional information"
    import "data.yaml"

    // Or use meta builder for complex frontmatter
    meta (meta {
        kv "key" "value"
        kvList "items" ["a"; "b"; "c"]
    })
}
```

### Writer Options

- `OutputFormat`: `Opencode` (default) or `Copilot`
- `OutputType`: `Md` (default), `Json`, or `Yaml`
- `ToolFormat`: `Auto` (default), `ToolsList`, or `ToolsMap` - Controls tool configuration output format
- `TemplateVariables`: Map of variable name → value for template rendering
- `DisableCodeBlockWrapping`: Force raw output even for `import` (default false)
- `RenameMap`: Map for renaming section headings
- `HeadingFormatter`: Optional function to format headings
- `GeneratedFooter`: Optional function to generate footer content
- `IncludeFrontmatter`: Whether to include frontmatter (default true)
- `CustomWriter`: Optional custom writer function

See `knowledge/import-data.md` for an example of generated output with imported data rules from `knowledge/import-data.rules.json`.

## Tool Configuration Formats

FsAgent supports type-safe tool configuration with automatic harness-specific name mapping:

### List Format (Allowlist)
Used by Copilot, Claude, and OpenCode:

```fsharp
open FsAgent.Tools

let agent = agent {
    name "my-agent"
    tools [Glob; Bash; Read]
}

let markdown = MarkdownWriter.writeAgent agent (fun _ -> ())
// Output (Opencode):
// tools:
//   - grep
//   - bash
//   - read
```

### Map Format (Enable/Disable)
Used by OpenCode for fine-grained control:

```fsharp
open FsAgent.Tools

let agent = agent {
    name "my-agent"
    tools [Edit; WebFetch]
    disallowedTools [Bash; Write]
}

let markdown = MarkdownWriter.writeAgent agent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
// Output:
// tools:
//   bash: false
//   write: false
//   edit: true
//   webfetch: true
```

### Allow/Disallow Pattern
Combine allowed and disallowed tools for convenience:

```fsharp
open FsAgent.Tools

let agent = agent {
    name "my-agent"
    tools [Glob; Bash; Read; Edit]
    disallowedTools [Bash; Write]  // Disable specific tools
}

let markdown = MarkdownWriter.writeAgent agent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
// Output:
// tools:
//   grep: true
//   bash: false   (overridden by disallowedTools)
//   read: true
//   edit: true
//   write: false  (from disallowedTools)
```

### Format Conversion

The writer can convert between formats:

```fsharp
open FsAgent.Tools

// List → Map (all tools enabled)
let agent = agent {
    tools [Glob; Bash]
}
let mapOutput = MarkdownWriter.writeAgent agent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
// Output: tools:\n  grep: true\n  bash: true

// Map → List (only enabled tools)
let agent2 = agent {
    tools [Edit; Read]
    disallowedTools [Bash]
}
let listOutput = MarkdownWriter.writeAgent agent2 (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
// Output: tools:\n  - edit\n  - read
// (bash excluded because it's disabled)
```

### Auto Format Selection

By default, `ToolFormat = Auto` selects the format based on `OutputFormat`:
- **Copilot**: Uses `ToolsList` (list format)
- **Opencode**: Uses `ToolsList` (list format, can override with explicit `ToolsMap`)

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

## Example: Toon Mission Agent

The script `examples/toon.fsx` demonstrates the prompt-first approach:

1. **Create a prompt** with role, objective, instructions, and imported TOON data
2. **Build an agent** that references the prompt with configuration metadata
3. **Generate output** with `MarkdownWriter.writeAgent`

```fsharp
// 1. Define reusable prompt
let toonPrompt = prompt {
    role "You are a narrative strategist for animated missions"
    objective "Map each catalog character to a mission brief"
    instructions "Use the imported TOON catalog..."
    import "examples/toon-data.toon"
}

// 2. Create agent with configuration
let toonAgent = agent {
    name "toon-importer"
    model "gpt-4.1"
    tools [Read; Custom "search"]
    prompt toonPrompt
}

// 3. Write to file
let markdown = MarkdownWriter.writeAgent toonAgent (fun _ -> ())
```

This shows how prompts can be defined once and reused across multiple agents while keeping configuration separate from content.

## Backward Compatibility

Existing code using `open FsAgent.DSL` continues to work via re-exports in `Library.fs`. However, the new modular approach with separate `Prompt` and `Agent` types is recommended for new code:

**Old style (still works):**
```fsharp
open FsAgent.DSL
// Uses backward compatibility layer
```

**New style (recommended):**
```fsharp
open FsAgent.Prompts
open FsAgent.Agents
open FsAgent.Writers
```

## Migration Guide

See [CHANGELOG.md](CHANGELOG.md) for detailed migration instructions from v0.1.0 to v0.2.0.
