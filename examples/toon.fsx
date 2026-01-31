// Run `dotnet fsi examples/toon.fsx` from the repo root to emit toon-agent.md.
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open System
open System.IO
open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.Writers

let scriptDir = __SOURCE_DIRECTORY__
let repoRoot = Path.GetFullPath(Path.Combine(scriptDir, ".."))
let toonDataPath = Path.Combine(scriptDir, "toon-data.toon")
let importPath = Path.GetRelativePath(repoRoot, toonDataPath)

Environment.CurrentDirectory <- repoRoot

// Create a reusable prompt for the TOON mission strategy
let toonPrompt =
    prompt {
        role "You are a narrative strategist for animated missions."
        objective "Map each catalog character to a mission brief in a structured, timely way."
        instructions "Use the imported TOON catalog to highlight signatures, roles, and mission hooks."
        context $"Catalog file: {importPath} contents available below."
        import importPath
        examples [
            Prompt.example
                "Character primer"
                "Summarize how the pilot, engineer, and scout work together for the Celestia Rescue mission."
        ]
        output "Produce a plan that briefly describes each character, their signature move, and a proposed mission tag."
    }

// Create the agent referencing the prompt
let toonMeta = meta {
    kv "org" "acme-corp"
    kvList "tags" ["toon"; "mission"; "example"]
}

let toonAgent =
    agent {
        meta toonMeta
        name "toon-importer"
        description "Imports a TOON catalog and builds mission summaries."
        model "gpt-4.1"
        tools [ "read"; "search"; "edit" ]
        author "Devon Burriss"
        version "1.0.0"
        license "MIT"
        temperature 0.7
        prompt toonPrompt
    }

let outputPath = Path.Combine(scriptDir, "toon-agent.md")

let markdown =
    MarkdownWriter.writeAgent toonAgent (fun opts ->
        opts.GeneratedFooter <- Some (fun ctx ->
            let timestamp = ctx.Timestamp.ToString("u")
            let agentName = ctx.AgentName |> Option.defaultValue "toon-importer"
            $"Generated {timestamp} for {agentName}"
        )
    )

File.WriteAllText(outputPath, markdown)
printfn "Wrote %s" outputPath
