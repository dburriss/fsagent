#!/usr/bin/env dotnet fsi
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Agents
open FsAgent.Writers

printfn "=== Tool Configuration Format Examples ==="
printfn ""

// Example 1: List format (default output)
// Note: Internally stored as map {grep: true, bash: true, read: true}
printfn "1. List format output (allowlist) - Default behavior:"
printfn "-------------------------------------------------------"
let listAgent = agent {
    name "list-agent"
    description "Agent using list format"
    tools ["grep" :> obj; "bash" :> obj; "read" :> obj]
}
let listOutput = MarkdownWriter.writeAgent listAgent (fun _ -> ())
printfn "%s" listOutput
printfn ""

// Example 2: Map format using toolMap DSL
printfn "2. Map format (enable/disable) - Using toolMap DSL:"
printfn "----------------------------------------------------"
let mapAgent = agent {
    name "map-agent"
    description "Agent using map format"
    toolMap [
        ("bash", false)
        ("write", false)
        ("edit", true)
        ("read", true)
        ("webfetch", true)
    ]
}
let mapOutput = MarkdownWriter.writeAgent mapAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" mapOutput
printfn ""

// Example 3: Output map format (shows internal storage)
printfn "3. Output map format (shows internal true/false values):"
printfn "---------------------------------------------------------"
let convertedToMap = MarkdownWriter.writeAgent listAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" convertedToMap
printfn ""

// Example 4: Output list format (filters to enabled tools only)
printfn "4. Output list format from map (only enabled tools shown):"
printfn "-----------------------------------------------------------"
let convertedToList = MarkdownWriter.writeAgent mapAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" convertedToList
printfn ""

// Example 5: Auto format selection
printfn "5. Auto format selection (Copilot uses list):"
printfn "----------------------------------------------"
let copilotAgent = agent {
    name "copilot-agent"
    description "Agent for Copilot"
    tools ["grep"; "bash"]
}
let copilotOutput = MarkdownWriter.writeAgent copilotAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Copilot
    opts.ToolFormat <- MarkdownWriter.Auto)  // Auto selects ToolsList for Copilot
printfn "%s" copilotOutput
printfn ""

// Example 6: Allow/Disallow pattern (merges into internal map)
printfn "6. Allow/Disallow pattern (merged into map):"
printfn "---------------------------------------------"
let allowDisallowAgent = agent {
    name "mixed-agent"
    description "Agent using allow/disallow"
    tools ["grep" :> obj; "bash" :> obj; "read" :> obj; "edit" :> obj]
    disallowedTools ["bash"; "write"]  // Sets bash=false (override), write=false (add)
}
let allowDisallowOutput = MarkdownWriter.writeAgent allowDisallowAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" allowDisallowOutput
printfn ""

// Example 7: Output list format from allow/disallow (filters enabled)
printfn "7. Allow/Disallow as list output (only enabled shown):"
printfn "-------------------------------------------------------"
let allowDisallowListOutput = MarkdownWriter.writeAgent allowDisallowAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" allowDisallowListOutput
printfn ""

printfn "=== Summary ==="
printfn "- Internal storage: Tools are ALWAYS stored as a map with true/false values"
printfn "- List output: tools: [\"tool1\", \"tool2\"] - Shows only enabled tools"
printfn "- Map output: tools: { tool1: true, tool2: false } - Shows all with enable/disable state"
printfn "- Use 'tools' DSL to enable tools, 'toolMap' DSL for explicit control, 'disallowedTools' to disable"
printfn "- 'disallowedTools' merges with existing tools map, overriding values to false"
printfn "- Writer outputs to list or map format using ToolFormat option (default: ToolsList)"
printfn "- Auto format selection based on OutputFormat (Copilot/Opencode both default to list)"
