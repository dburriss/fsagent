# Migration Guide: v1.x to v2.0

This guide helps you migrate from FsAgent v1.x to v2.0, which introduces type-safe tool configuration and renames `AgentFormat` to `AgentHarness`.

## Breaking Changes Summary

1. **Tool namespace**: `Tool` type moved from `FsAgent.AST` to `FsAgent.Tools` namespace
2. **AgentFormat → AgentHarness**: Type renamed to better represent execution platform
3. **ClaudeCode harness added**: New harness type for Claude Code platform
4. **tools operation**: Now accepts `Tool list` instead of `obj list`
5. **disallowedTools operation**: Now accepts `Tool list` instead of `string list`
6. **toolMap operation removed**: Use `tools` + `disallowedTools` instead
7. **Frontmatter storage**: Tools stored as `Tool list`, not string maps

## 1. Tool Namespace Change (FsAgent.AST → FsAgent.Tools)

The `Tool` type has been moved to its own dedicated namespace for better organization and discoverability.

**Before (v1.x):**
```fsharp
open FsAgent.AST  // Tool was here
```

**After (v2.0):**
```fsharp
open FsAgent.Tools  // Tool is now here
```

**Impact:** You must update all imports where you use the `Tool` type. If you also use AST types like `Node` or `DataFormat`, you'll need both namespaces:

```fsharp
open FsAgent.AST    // For Node, DataFormat, AST module
open FsAgent.Tools  // For Tool type
```

## 2. AgentFormat → AgentHarness

**Before (v1.x):**
```fsharp
MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.AgentFormat.Opencode)
```

**After (v2.0):**
```fsharp
MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Opencode)
// or explicitly:
    opts.OutputFormat <- MarkdownWriter.AgentHarness.Opencode)
```

The type is renamed from `AgentFormat` to `AgentHarness` throughout the codebase.

## 3. ClaudeCode Harness

A new harness type is available for Claude Code:

```fsharp
MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode)
```

ClaudeCode uses capitalized tool names (e.g., `Write`, `Edit`, `Bash`).

## 4. Typed Tools (tools operation)

**Before (v1.x):**
```fsharp
let agent = agent {
    tools ["write" :> obj; "bash" :> obj; "read" :> obj]
}
```

**After (v2.0):**
```fsharp
open FsAgent.Tools  // for Tool type

let agent = agent {
    tools [Write; Bash; Custom "read"]
}
```

**Benefits:**
- IDE autocomplete for built-in tools
- Compile-time checking (no typos)
- Harness-agnostic (same code generates correct names for each platform)

## 5. Typed disallowedTools

**Before (v1.x):**
```fsharp
let agent = agent {
    disallowedTools ["bash"; "write"]
}
```

**After (v2.0):**
```fsharp
open FsAgent.Tools

let agent = agent {
    disallowedTools [Bash; Write]
}
```

## 6. toolMap Removal

The `toolMap` operation has been removed. Migrate to `tools` + `disallowedTools`:

**Before (v1.x):**
```fsharp
let agent = agent {
    toolMap [("write", true); ("bash", false); ("edit", true)]
}
```

**After (v2.0) - Option 1: Separate lists (recommended)**
```fsharp
open FsAgent.Tools

let agent = agent {
    tools [Write; Edit]
    disallowedTools [Bash]
}
```

**After (v2.0) - Option 2: Only enabled tools**
```fsharp
open FsAgent.Tools

let agent = agent {
    tools [Write; Edit]
}
```

## 7. Custom Tools (MCP, platform-specific)

**Before (v1.x):**
```fsharp
let agent = agent {
    tools ["mcp_database" :> obj; "github_api" :> obj]
}
```

**After (v2.0):**
```fsharp
open FsAgent.Tools

let agent = agent {
    tools [Custom "mcp_database"; Custom "github_api"]
}
```

## 8. Mixed Built-in and Custom Tools

**Before (v1.x):**
```fsharp
let agent = agent {
    tools ["write" :> obj; "bash" :> obj; "mcp_special" :> obj]
}
```

**After (v2.0):**
```fsharp
open FsAgent.Tools

let agent = agent {
    tools [Write; Bash; Custom "mcp_special"]
}
```

## 9. Harness-Specific Tool Names

Tool names are now automatically mapped based on the target harness:

```fsharp
open FsAgent.Tools

let agent = agent {
    name "my-agent"
    description "Example agent"
    tools [Write; Bash]
}

// Generate Opencode output (lowercase)
let opencodeOut = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- Opencode)
// Output: tools:
//           - write
//           - bash

// Generate ClaudeCode output (capitalized)
let claudeOut = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- ClaudeCode)
// Output: tools:
//           - Write
//           - Bash
```

You write the agent definition once and generate platform-specific output.

## 10. Tool Type Reference

```fsharp
type Tool =
    | Write              // File writing
    | Edit               // File editing
    | Bash               // Shell commands
    | WebFetch           // HTTP fetching
    | Todo               // Task management
    | Custom of string   // MCP or platform-specific tools
```

## Complete Migration Example

**Before (v1.x):**
```fsharp
open FsAgent
open FsAgent.Agents
open FsAgent.Writers

let agent = agent {
    name "code-reviewer"
    description "Reviews code for issues"
    tools ["write" :> obj; "bash" :> obj; "read" :> obj]
    disallowedTools ["bash"]
}

let output = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.AgentFormat.Opencode)
```

**After (v2.0):**
```fsharp
open FsAgent
open FsAgent.Agents
open FsAgent.Writers
open FsAgent.Tools  // for Tool type

let agent = agent {
    name "code-reviewer"
    description "Reviews code for issues"
    tools [Write; Custom "read"]
    disallowedTools [Bash]
}

let output = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- Opencode)
```

## Troubleshooting

### Compilation Error: "The type or namespace 'Tool' is not defined"

Add the `FsAgent.Tools` namespace:

```fsharp
// Before
open FsAgent.AST

// After
open FsAgent.Tools
```

### Compilation Error: "The type 'AgentFormat' is not defined"

Replace `AgentFormat` with `AgentHarness`:

```fsharp
// Before
opts.OutputFormat <- MarkdownWriter.AgentFormat.Opencode

// After
opts.OutputFormat <- MarkdownWriter.Opencode
```

### Compilation Error: "This expression was expected to have type 'Tool list'"

Replace string tool names with Tool union cases:

```fsharp
// Before
tools ["write" :> obj; "bash" :> obj]

// After
tools [Write; Bash]
```

### Compilation Error: "The field, constructor or member 'toolMap' is not defined"

Replace `toolMap` with `tools` and `disallowedTools`:

```fsharp
// Before
toolMap [("write", true); ("bash", false)]

// After
tools [Write]
disallowedTools [Bash]
```

## Need Help?

- Check the updated [CLAUDE.md](CLAUDE.md) for architecture details
- Review the test suite in `tests/FsAgent.Tests/MarkdownWriterTests.fs` for examples
- Open an issue on GitHub if you encounter migration problems
