---
description: Teach an agent how to author a new slash command using the fsagent F# DSL.
license: MIT
metadata: 
  author: fsagent example
  version: 2.0.0
name: create-command
---

# Boilerplate

```fsharp
#r "../../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Commands
open FsAgent.Prompts
open FsAgent.Writers
```

# Pattern

```fsharp
let myCommand = command {
    name        "my-command"
    description "Does something useful."
    section "Task" "Handle the user's input: $ARGUMENTS"
    prompt (prompt {
        objective "Process the input."
        instructions "Run relevant tools and summarize."
    })
}

let path = FileWriter.writeCommand myCommand AgentWriter.Opencode (Project ".") None (fun _ -> ())
printfn "Wrote %s" path
```

# Operations

```
name : string (required)
description : string (required)
section : string * string
prompt : Prompt (embed a reusable prompt)
import : string (code-block wrapped)
importRaw : string (raw embed)
template : string (Fue template with {{{variable}}} syntax)
templateFile : string (load template from file path)

Use $ARGUMENTS anywhere to forward the text after the command name.
Use {{{tool Name}}} to inject harness-correct tool names.
```

# Reference

```
For template rendering and tool name injection, see Templating (Fue) in AGENTS.md.
For FileWriter API and namespace parameter, see PROJECT.md.
```
