# Tool Map Enrichers Design

## Problem Statement

Current tool configuration in FsAgent is stringly-typed and redundant:

```fsharp
agent {
    tools ["write"; "bash"]              // obj list → all enabled
    toolMap ["write", true; "bash", false]  // explicit control
    disallowedTools ["write"; "bash"]    // convenience for disabling
}
```

**Pain Points:**
1. No discoverability - must know exact tool names
2. No IDE autocomplete or compile-time checking
3. Three different operations that do overlapping things
4. All agent-specific tools would need to be in a single discriminated union

## Core Insight

**Separate concerns by layer:**

```
┌────────────────────────────────────────────────────────┐
│                    Separation of Concerns               │
├────────────────────────────────────────────────────────┤
│                                                         │
│  Agent Definition Layer (DSL):                          │
│    - Universal tools only in DU (Write, Edit, Bash)    │
│    - Agent focuses on its core behavior                 │
│    - Simple, discoverable API                           │
│                                                         │
│  ┌──────────────────────────────────────────┐          │
│  │ agent {                                  │          │
│  │     tools [Write; Bash]                  │          │
│  │ }                                        │          │
│  └──────────────────────────────────────────┘          │
│                        │                                │
│                        ▼                                │
│  Writer Configuration Layer:                            │
│    - Context-aware enrichment                           │
│    - Platform-specific tools                            │
│    - Agent-specific MCP tools                           │
│    - Access to full agent context                       │
│                                                         │
│  ┌──────────────────────────────────────────┐          │
│  │ writeMarkdown agent (fun opts ->         │          │
│  │     opts.ToolEnricher <- enricherFn      │          │
│  │ )                                        │          │
│  └──────────────────────────────────────────┘          │
│                        │                                │
│                        ▼                                │
│  Output: Markdown/JSON with enriched tool map           │
│                                                         │
└────────────────────────────────────────────────────────┘
```

## Proposed Design

### 1. Tool Discriminated Union (Universal Tools Only)

```fsharp
type Tool =
    | Write
    | Edit
    | Bash
    | WebFetch
    | Todo
    | Custom of string  // Escape hatch for agent-specific tools

module Tool =
    let toString = function
        | Write -> "write"
        | Edit -> "edit"
        | Bash -> "bash"
        | WebFetch -> "webfetch"
        | Todo -> "todo"
        | Custom s -> s

    let fromString = function
        | "write" -> Some Write
        | "edit" -> Some Edit
        | "bash" -> Some Bash
        | "webfetch" -> Some WebFetch
        | "todo" -> Some Todo
        | s -> Some (Custom s)
```

### 2. Rename AgentFormat → AgentHarness

More accurately represents the execution platform/harness rather than just output format:

```fsharp
type AgentHarness =
    | Opencode
    | Copilot

// OutputType remains separate
type OutputType =
    | Md
    | Json
    | Yaml
```

### 3. Tool Enricher Function

Writer-time enrichment with full context:

```fsharp
type ToolEnricher =
    AgentHarness -> Agent -> Map<string, obj> -> Map<string, obj>
    //   ^           ^              ^                  ^
    //   |           |              |                  └─ Enriched tool map
    //   |           |              └──── Current tool map from agent definition
    //   |           └─────────────────── Full agent context (frontmatter, sections)
    //   └─────────────────────────────── Platform being written to

type Options = {
    mutable OutputFormat: AgentHarness  // Renamed from AgentFormat
    mutable ToolEnricher: ToolEnricher option  // New
    // ... other existing options
}
```

### 4. Simplified Agent Builder

Remove `toolMap`, keep only:
- `tools` - Add tools
- `disallowedTools` - Remove tools (convenience)

```fsharp
agent {
    name "my-agent"
    tools [Write; Edit; Bash]
    disallowedTools [Bash]  // Results in: Write, Edit
}
```

**Alternative:** Remove `disallowedTools` too, since enricher can handle removal.

## Usage Patterns

### Pattern 1: Agent-Specific MCP Tools

```fsharp
let databaseEnricher: ToolEnricher =
    fun harness agent toolMap ->
        match agent.Frontmatter |> Map.tryFind "name" with
        | Some (:? string as "database-agent") ->
            toolMap
            |> Map.add "mcp_db_query" (box true)
            |> Map.add "mcp_db_execute" (box true)
        | _ -> toolMap

writeMarkdown myAgent (fun opts ->
    opts.ToolEnricher <- Some databaseEnricher
)
```

### Pattern 2: Platform-Specific Restrictions

```fsharp
let platformEnricher: ToolEnricher =
    fun harness agent toolMap ->
        match harness with
        | Opencode ->
            toolMap  // Opencode supports MCP tools
        | Copilot ->
            // Copilot doesn't support MCP, strip them out
            toolMap
            |> Map.filter (fun k v -> not (k.StartsWith("mcp_")))
```

### Pattern 3: Metadata-Driven Configuration

```fsharp
let metadataEnricher: ToolEnricher =
    fun harness agent toolMap ->
        let tags =
            agent.Frontmatter
            |> Map.tryFind "tags"
            |> Option.bind (function :? string list as lst -> Some lst | _ -> None)
            |> Option.defaultValue []

        match tags with
        | lst when lst |> List.contains "database" ->
            toolMap |> Map.add "mcp_db_query" (box true)
        | lst when lst |> List.contains "github" ->
            toolMap |> Map.add "mcp_github_pr" (box true)
        | _ -> toolMap
```

### Pattern 4: Composable Enrichers

```fsharp
let compose (enrichers: ToolEnricher list) : ToolEnricher =
    fun harness agent toolMap ->
        enrichers
        |> List.fold (fun map enricher -> enricher harness agent map) toolMap

let myEnricher =
    compose [
        databaseEnricher
        platformEnricher
        metadataEnricher
    ]

writeMarkdown agent (fun opts ->
    opts.ToolEnricher <- Some myEnricher
)
```

## Complete Flow

```
┌─────────────────────────────────────────────────────┐
│                    Tool Configuration Flow           │
├─────────────────────────────────────────────────────┤
│                                                      │
│  1. Agent Definition (Author)                        │
│     ┌──────────────────────────────────────┐        │
│     │ agent {                              │        │
│     │     name "my-agent"                  │        │
│     │     tools [Write; Edit]              │        │
│     │ }                                    │        │
│     └──────────────────────────────────────┘        │
│                         │                            │
│                         ▼                            │
│     Base Tool Map: { write: true, edit: true }      │
│                         │                            │
│                         │                            │
│  2. Writer Configuration (Library User)              │
│     ┌──────────────────────────────────────┐        │
│     │ writeMarkdown agent (fun opts ->     │        │
│     │     opts.OutputFormat <- Opencode    │        │
│     │     opts.ToolEnricher <- Some (      │        │
│     │         fun harness agent toolMap -> │        │
│     │             match agent.name with    │        │
│     │             | "my-agent" ->          │        │
│     │                 toolMap              │        │
│     │                 |> Map.add "mcp_db"  │        │
│     │             | _ -> toolMap           │        │
│     │     )                                │        │
│     │ )                                    │        │
│     └──────────────────────────────────────┘        │
│                         │                            │
│                         ▼                            │
│     Enriched: { write: true, edit: true,            │
│                 mcp_db: true }                       │
│                         │                            │
│                         ▼                            │
│  3. Markdown Output                                  │
│     ┌──────────────────────────────────────┐        │
│     │ ---                                  │        │
│     │ tools:                               │        │
│     │   write: true                        │        │
│     │   edit: true                         │        │
│     │   mcp_db: true                       │        │
│     │ ---                                  │        │
│     └──────────────────────────────────────┘        │
│                                                      │
└─────────────────────────────────────────────────────┘
```

## Benefits

1. **Discoverability** - IDE autocomplete for universal tools
2. **Type Safety** - Compile-time checking for common tools
3. **Flexibility** - Custom tools via `Custom of string`
4. **Separation of Concerns** - Agent definition vs. writer configuration
5. **Composability** - Enrichers can be combined and reused
6. **Context-Aware** - Access to harness, agent metadata, and current tool map
7. **No Explosion** - DU contains only universal tools, not every possible MCP tool

## Open Questions

1. **Should `disallowedTools` remain?** Enricher can handle removal, but it's convenient.
2. **Should enricher be optional or always present?** Default identity vs. Some/None
3. **Multiple enrichers or single?** Compose externally vs. internal list

## Implementation Notes

- Tool enrichment happens at write time, not agent definition time
- Enricher receives immutable tool map, returns new map (pure function)
- Multiple enrichers can be composed using standard functional composition
- Agent definition remains simple and focused on universal concerns
