# Agent Enricher Design

## Overview

General-purpose agent transformation at write time. Instead of specialized enrichers (ToolEnricher, SectionEnricher, etc.), use a single composable `AgentEnricher` that transforms the entire Agent structure.

## Core Type

```fsharp
type AgentEnricher = AgentHarness -> Agent -> Agent
```

**Signature:**
- **Input**: Target harness (Opencode, Copilot) and current agent
- **Output**: Transformed agent
- **Timing**: Applied during serialization, before converting to string
- **Errors**: Throws exceptions (no Result wrapper)

## Capabilities

```
┌─────────────────────────────────────────────────┐
│         Agent Enricher Capabilities              │
├─────────────────────────────────────────────────┤
│                                                  │
│  Frontmatter Transformations:                    │
│    • Modify tools map                            │
│    • Add/remove platform-specific metadata       │
│    • Adjust temperature/model based on harness   │
│    • Transform any frontmatter field             │
│                                                  │
│  Section Transformations:                        │
│    • Add platform-specific sections              │
│    • Remove unsupported sections                 │
│    • Transform section content                   │
│    • Reorder sections                            │
│                                                  │
│  Import Transformations:                         │
│    • Resolve imports differently per platform    │
│    • Add platform-specific imports               │
│                                                  │
└─────────────────────────────────────────────────┘
```

## Writer Integration

### Options Type

```fsharp
type Options = {
    mutable OutputFormat: AgentHarness  // Renamed from AgentFormat
    mutable AgentEnricher: AgentEnricher option
    // ... other existing options
}
```

### Usage

```fsharp
writeMarkdown agent (fun opts ->
    opts.OutputFormat <- Opencode
    opts.AgentEnricher <- Some myEnricher
)
```

### Execution Flow

```
Agent Definition (DSL)
        ↓
    Agent AST
        ↓
AgentEnricher (if configured)
        ↓
Enriched Agent AST
        ↓
  Serialization
        ↓
String Output (Markdown/JSON/YAML)
```

## Helper Functions

Provide common operations to make enrichers easier to write:

```fsharp
module AgentEnricher =
    /// Modify the tools map
    val modifyTools : (Map<string, obj> -> Map<string, obj>) -> AgentEnricher

    /// Add a section to the agent
    val addSection : string -> Node list -> AgentEnricher

    /// Remove sections matching a predicate
    val removeSections : (string -> bool) -> AgentEnricher

    /// Modify frontmatter field
    val modifyFrontmatter : string -> (obj option -> obj option) -> AgentEnricher

    /// Add/update frontmatter field
    val setFrontmatter : string -> obj -> AgentEnricher

    /// Remove frontmatter field
    val removeFrontmatter : string -> AgentEnricher

    /// Compose multiple enrichers
    val compose : AgentEnricher list -> AgentEnricher

    /// Identity enricher (no-op)
    val identity : AgentEnricher
```

## Implementation Examples

### Example 1: Tools Enrichment

```fsharp
// Using helper
let toolsEnricher: AgentEnricher =
    AgentEnricher.modifyTools (fun toolMap ->
        toolMap
        |> Map.add "mcp_db_query" (box true)
        |> Map.add "mcp_db_execute" (box true)
    )

// Manual implementation
let toolsEnricherManual: AgentEnricher =
    fun harness agent ->
        let enrichedTools =
            match agent.Frontmatter |> Map.tryFind "tools" with
            | Some (:? Map<string, obj> as toolMap) ->
                toolMap
                |> Map.add "mcp_db_query" (box true)
                |> Map.add "mcp_db_execute" (box true)
            | _ -> Map.empty

        { agent with
            Frontmatter = agent.Frontmatter |> Map.add "tools" (box enrichedTools) }
```

### Example 2: Platform-Specific Sections

```fsharp
let sectionEnricher: AgentEnricher =
    fun harness agent ->
        match harness with
        | Opencode ->
            // Add OpenCode-specific context
            agent
            |> AgentEnricher.addSection "OpenCode Context" [
                Text "You have access to MCP servers for extended functionality."
            ]
        | Copilot ->
            // Remove MCP-related sections for Copilot
            agent
            |> AgentEnricher.removeSections (fun name ->
                name.Contains("MCP") || name.Contains("OpenCode")
            )
```

### Example 3: Agent-Name-Based Enrichment

```fsharp
let nameBasedEnricher: AgentEnricher =
    fun harness agent ->
        match agent.Frontmatter |> Map.tryFind "name" with
        | Some (:? string as "database-agent") ->
            agent
            |> AgentEnricher.modifyTools (fun tools ->
                tools
                |> Map.add "mcp_db_query" (box true)
                |> Map.add "mcp_db_execute" (box true)
            )
            |> AgentEnricher.addSection "Database Context" [
                Text "You have access to database query tools."
            ]

        | Some (:? string as "github-agent") ->
            agent
            |> AgentEnricher.modifyTools (fun tools ->
                tools
                |> Map.add "mcp_github_pr" (box true)
                |> Map.add "mcp_github_issue" (box true)
            )

        | _ -> agent
```

### Example 4: Composite Enricher

```fsharp
let myEnricher =
    AgentEnricher.compose [
        toolsEnricher
        sectionEnricher
        nameBasedEnricher
    ]

writeMarkdown agent (fun opts ->
    opts.OutputFormat <- Opencode
    opts.AgentEnricher <- Some myEnricher
)
```

## Benefits

1. **Single Responsibility** - One enrichment point instead of many specialized ones
2. **Composability** - Easy to combine multiple transformations using `compose`
3. **Flexibility** - Can handle any transformation need (tools, sections, frontmatter)
4. **Pure Function** - `Agent → Agent` is clean, testable, and predictable
5. **Platform-Aware** - Has access to target harness for conditional logic
6. **Type-Safe** - Works on Agent AST, not strings

## Related Changes

### 1. Rename AgentFormat → AgentHarness

```fsharp
// Before
type AgentFormat = Opencode | Copilot

// After
type AgentHarness = Opencode | Copilot
```

**Rationale**: "Harness" better represents the execution platform/environment rather than just the output format.

### 2. Tool Discriminated Union

From the `tool-map-enrichers.md` plan:

```fsharp
type Tool =
    | Write
    | Edit
    | Bash
    | WebFetch
    | Todo
    | Custom of string

module Tool =
    let toString = function
        | Write -> "write"
        | Edit -> "edit"
        | Bash -> "bash"
        | WebFetch -> "webfetch"
        | Todo -> "todo"
        | Custom s -> s
```

### 3. Simplified Agent Builder

Remove `toolMap`, keep:
- `tools` - Add tools using DU
- `disallowedTools` - Remove tools (convenience)

```fsharp
agent {
    tools [Write; Edit; Bash]
    disallowedTools [Bash]  // Results in: Write, Edit
}
```

## Design Decisions

### ✓ Modify Agent AST, not strings
Enricher transforms the data structure before serialization, ensuring type safety and composability.

### ✓ Provide helper functions
Common operations (modifyTools, addSection, etc.) reduce boilerplate and make enrichers easier to write.

### ✓ Throw exceptions on error
No Result wrapper. Let exceptions bubble up naturally. Keep the API simple.

### ✗ No before/after hooks (for now)
Single enrichment point is sufficient. Can revisit if use cases emerge.

### ✗ No specialized enrichers
AgentEnricher handles all cases. Don't create ToolEnricher, SectionEnricher, etc.

## Implementation Location

```
src/FsAgent/
  ├── Agent.fs         - Agent type, AgentBuilder
  ├── Writers.fs       - MarkdownWriter with AgentEnricher support
  └── Enrichers.fs     - AgentEnricher module with helpers (NEW)
```

Or add to existing files:
- AgentEnricher type definition in `Writers.fs`
- Helper functions in `Agent.fs` or new `Enrichers.fs`

## Testing Strategy

### Unit Tests for Helpers

```fsharp
[<Fact>]
let ``modifyTools adds tools to map`` () =
    let agent = agent { tools [Write] }
    let enricher = AgentEnricher.modifyTools (Map.add "custom" (box true))
    let enriched = enricher Opencode agent

    let tools = enriched.Frontmatter.["tools"] :?> Map<string, obj>
    tools.ContainsKey "custom" |> should be True

[<Fact>]
let ``addSection appends section to agent`` () =
    let agent = agent { section "Role" "Assistant" }
    let enricher = AgentEnricher.addSection "Context" [Text "New context"]
    let enriched = enricher Opencode agent

    enriched.Sections.Length |> should equal 2

[<Fact>]
let ``compose applies enrichers in order`` () =
    let agent = Agent.empty
    let enricher = AgentEnricher.compose [
        AgentEnricher.setFrontmatter "step" (box 1)
        AgentEnricher.setFrontmatter "step" (box 2)
    ]
    let enriched = enricher Opencode agent

    enriched.Frontmatter.["step"] :?> int |> should equal 2
```

### Integration Tests

```fsharp
[<Fact>]
let ``writeMarkdown applies enricher before serialization`` () =
    let agent = agent {
        name "test-agent"
        tools [Write]
    }

    let enricher = AgentEnricher.modifyTools (Map.add "custom" (box true))

    let output =
        MarkdownWriter.writeMarkdown agent (fun opts ->
            opts.AgentEnricher <- Some enricher
        )

    output |> should contain "custom: true"
```

## Migration Path

1. Add `AgentEnricher` type and helpers
2. Update `Options` to include `AgentEnricher`
3. Update `writeMarkdown` to apply enricher before serialization
4. Add tests for helpers and integration
5. Document usage patterns in README/examples
6. Optional: Deprecate any existing specialized enrichers (if they exist)

## Open Questions

None - design decisions confirmed.

## Next Steps

1. Implement `AgentEnricher` type in `Writers.fs`
2. Implement helper functions in `Enrichers.fs` or `Agent.fs`
3. Update `writeMarkdown` to apply enricher
4. Add unit tests for helpers
5. Add integration tests
6. Update documentation with examples
