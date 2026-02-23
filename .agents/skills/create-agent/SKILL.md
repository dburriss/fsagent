---
description: Teach an agent how to author a new agent definition using the fsagent F# DSL.
license: MIT
metadata: 
  author: fsagent example
  version: 1.0.0
name: create-agent
---

# Overview

An *agent* in fsagent is defined with the `agent { ... }` computation expression.
It combines frontmatter metadata (name, description, model, tools) with a prompt body.

Key rule: the `name` field is required — it becomes the output filename.

# Minimal Example

```fsharp
open FsAgent.Agents
open FsAgent.Writers

let myAgent =
    agent {
        name        "my-agent"
        description "A brief description shown in the agent picker."
        model       "claude-opus-4-5"
        prompt (prompt {
            role         "You are a helpful assistant."
            instructions "Answer concisely."
        })
    }

// Write to .opencode/agents/my-agent.md
let path =
    FileWriter.writeAgent myAgent AgentWriter.Opencode (Project ".") (fun _ -> ())
printfn "Wrote %s" path
```

# Adding Tools

Restrict or allow specific tools using `tools` and `disallowedTools`.

```fsharp
open FsAgent.Tools

let researchAgent =
    agent {
        name        "research-agent"
        description "Research-only agent with read and web access."
        tools       [ Read; WebFetch; WebSearch ]
        prompt (prompt {
            role "You are a research assistant."
        })
    }
```

Available tool values: `Write`, `Edit`, `Bash`, `Read`, `Glob`, `WebFetch`, `WebSearch`,
`Question`, `Skill`, `TodoWrite`, `TodoRead`, `Custom "tool-name"`.

# Workflow

1. Define the agent with `agent { ... }`.
2. Call `FileWriter.writeAgent agent harness scope configure` to render and write.
3. Restart the harness (or reload agents) so the new agent is available.
