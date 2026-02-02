# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FsAgent is an F# library for generating agent configuration files for AI tools (Opencode, Copilot, Claude Code). It provides:
- An immutable AST (`Node`, `Agent`, `Tool` types) for representing agent structures
- A computation expression DSL (`agent { ... }`, `meta { ... }`) for ergonomic authoring
- Type-safe tool configuration with harness-specific name mapping
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

### Core Types

The library is organized across multiple files:

**src/FsAgent/AST.fs**
- **`Node`** - Discriminated union representing AST nodes: `Text`, `Section`, `List`, `Imported`, `Template`, `TemplateFile`
- **`Tool`** - Discriminated union for type-safe tool references: `Write`, `Edit`, `Bash`, `WebFetch`, `Todo`, `Custom of string`
- **`AST` module** - Constructor functions (`role`, `objective`, `instructions`, etc.) and frontmatter helpers (`fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap`)

**src/FsAgent/Prompts.fs**
- **`Prompt`** - Record with `Sections: Node list`
- **`PromptBuilder`** - Computation expression for building prompts

**src/FsAgent/Agents.fs**
- **`Agent`** - Record with `Frontmatter: Map<string, obj>` and `Sections: Node list`
- **`AgentBuilder`** and **`MetaBuilder`** - Computation expressions for ergonomic agent authoring

**src/FsAgent/Writers.fs**
- **`AgentHarness`** - Target execution platform: `Opencode`, `Copilot`, `ClaudeCode`
- **`MarkdownWriter` module** - Converts `Agent` to string output with configurable `Options`
- Harness-specific tool name mapping (e.g., `Write` → "write" for Opencode, "Write" for ClaudeCode)

**src/FsAgent/Library.fs**
- Public API re-exports for convenient access to all types

### Data Flow

```
DSL (agent { ... }) → Agent record → MarkdownWriter.writeMarkdown → String output
```

### Output Formats

- **AgentHarness**: Execution platform - `Opencode` (default), `Copilot` (requires `name` and `description` in frontmatter), `ClaudeCode`
- **OutputType**: `Md` (markdown), `Json`, `Yaml`
- **ToolFormat**: `ToolsList` (array), `ToolsMap` (boolean map), `Auto` (default, based on harness)

### Tool Configuration

The `Tool` discriminated union provides type-safe tool references with IDE autocomplete:

```fsharp
type Tool =
    | Write      // File writing capability
    | Edit       // File editing capability
    | Bash       // Shell command execution
    | WebFetch   // HTTP fetching capability
    | Todo       // Task management
    | Custom of string  // MCP tools or platform-specific tools
```

**Usage examples:**

```fsharp
// Basic tool configuration
let agent = agent {
    tools [Write; Edit; Bash]
}

// Disable specific tools
let agent = agent {
    tools [Write; Edit; Bash; WebFetch]
    disallowedTools [Bash]  // Bash will be disabled
}

// Custom tools (MCP, platform-specific)
let agent = agent {
    tools [Write; Custom "mcp_database"; Custom "github_api"]
}
```

**Harness-specific tool names:**

Tool names are automatically mapped based on the target harness:

| Tool | Opencode | Copilot | ClaudeCode |
|------|----------|---------|------------|
| Write | write | write | Write |
| Edit | edit | edit | Edit |
| Bash | bash | bash | Bash |
| WebFetch | webfetch | webfetch | WebFetch |
| Todo | todo | todo | Todo |
| Custom "x" | x | x | x |

This allows you to write harness-agnostic agent definitions and generate output for any platform.

### Writer Options

Options are configured via a mutable record passed to a configuration function:

```fsharp
// Basic configuration
MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- Opencode  // AgentHarness: Opencode, Copilot, or ClaudeCode
    opts.RenameMap <- Map.ofList ["role", "Agent Role"]
    opts.HeadingFormatter <- Some (fun s -> s.ToUpper()))

// Generate output for different harnesses
let opencodeOutput = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- Opencode
    opts.ToolFormat <- ToolsList)

let claudeOutput = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- ClaudeCode
    opts.ToolFormat <- ToolsList)

// The same agent definition produces harness-specific tool names
```

## Test Organization

Tests use xUnit with naming convention: `[Category]: Description`
- **A (Acceptance)**: End-to-end DSL → AST → Writer pipeline tests
- **B (Building)**: Internal scaffolding tests
- **C (Communication)**: External boundary/validation tests
