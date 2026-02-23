// Run `dotnet fsi .agents/skills/create-agent.fsx` from the repo root.
// Generates .agents/skills/create-agent/SKILL.md

#r "../../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Skills
open FsAgent.Writers

let projectRoot =
    System.IO.Path.GetFullPath(
        System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))

let createAgentSkill =
    skill {
        name "create-agent"
        description "Teach an agent how to author a new agent definition using the fsagent F# DSL."
        license "MIT"
        metadata (Map.ofList [
            "author",  box "fsagent example"
            "version", box "2.0.0"
        ])

        section "Boilerplate" """```fsharp
#r "../../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.Tools
open FsAgent.Writers
```"""

        section "Pattern" """```fsharp
let myAgent = agent {
    name        "my-agent"
    description "A helpful assistant."
    model       "claude-opus-4-5"
    tools       [Read; Write; Bash]
    prompt (prompt {
        role         "You are a helpful assistant."
        instructions "Answer concisely."
    })
}

let path = FileWriter.writeAgent myAgent AgentWriter.Opencode (Project ".") (fun _ -> ())
printfn "Wrote %s" path
```"""

        section "Operations" """```
name : string (required)
description : string
model : string
temperature : float
maxTokens : float
tools : Tool list
disallowedTools : Tool list
author : string
version : string
license : string
prompt : Prompt (embed a reusable prompt)
section : string * string
import : string (code-block wrapped)
importRaw : string (raw embed)
meta : MetaBuilder (use kv, kvList, kvObj, kvListObj for custom frontmatter)

Available tools: Write, Edit, Bash, Shell, Read, Glob, List, LSP, Skill, TodoWrite, TodoRead, WebFetch, WebSearch, Question, Todo, Custom "name"
```"""

        section "Reference" """```
For detailed type info and harness-specific tool mapping, see ARCHITECTURE.md.
For FileWriter API and harness support matrix, see PROJECT.md.
```"""
    }

let path =
    FileWriter.writeSkill createAgentSkill AgentWriter.Opencode (Project projectRoot) (fun _ -> ())
printfn "Wrote %s" path
