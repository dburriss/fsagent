# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FsAgent is an F# library for generating agent configuration files for AI tools (Opencode, Copilot). It provides:
- An immutable AST (`Node`, `Agent` types) for representing agent structures
- A computation expression DSL (`agent { ... }`, `meta { ... }`) for ergonomic authoring
- A configurable Markdown writer with support for multiple output formats

## Build and Test Commands

```bash
# Build the project
dotnet build

# Run all tests
dotnet test

# Run a specific test by name filter
dotnet test --filter "DisplayName~inferFormat"

# Build in release mode
dotnet build -c Release

# Create NuGet package
dotnet pack src/FsAgent/FsAgent.fsproj -c Release
```

## Architecture

### Core Types (Library.fs)

The entire library is in a single file `src/FsAgent/Library.fs`:

- **`Node`** - Discriminated union representing AST nodes: `Text`, `Section`, `List`, `Imported`
- **`Agent`** - Record with `Frontmatter: Map<string, obj>` and `Sections: Node list`
- **`AST` module** - Constructor functions (`role`, `objective`, `instructions`, `context`, `output`, `example`, `examples`) and frontmatter helpers (`fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap`)
- **`DSL` module** - `AgentBuilder` and `MetaBuilder` computation expressions
- **`MarkdownWriter` module** - Converts `Agent` to string output with configurable `Options`

### Data Flow

```
DSL (agent { ... }) → Agent record → MarkdownWriter.writeMarkdown → String output
```

### Output Formats

- **AgentFormat**: `Opencode` (default), `Copilot` (requires `name` and `description` in frontmatter)
- **OutputType**: `Md` (markdown), `Json`, `Yaml`
- **ImportInclusion**: `Exclude` (default), `IncludeRaw` (embeds file content)

### Writer Options

Options are configured via a mutable record passed to a configuration function:
```fsharp
MarkdownWriter.writeMarkdown agent (fun opts ->
    opts.OutputFormat <- Opencode
    opts.RenameMap <- Map.ofList ["role", "Agent Role"]
    opts.HeadingFormatter <- Some (fun s -> s.ToUpper()))
```

## Test Organization

Tests use xUnit with naming convention: `[Category]: Description`
- **A (Acceptance)**: End-to-end DSL → AST → Writer pipeline tests
- **B (Building)**: Internal scaffolding tests
- **C (Communication)**: External boundary/validation tests
