---
description: Teach an agent how to author a new skill using the fsagent F# DSL.
license: MIT
metadata: 
  author: fsagent example
  version: 2.0.0
name: create-skill
---

# Boilerplate

```fsharp
#r "../../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"
#r "nuget: Fue, 2.2.0"  // For template nodes with {{{tool Name}}}

open FsAgent.Skills
open FsAgent.Writers
```

# Pattern

```fsharp
let mySkill = skill {
    name        "my-skill"
    description "Teach the agent how to do X."
    license     "MIT"
    section "Instructions" "Step-by-step guide for doing X."
    template "Use the {{{tool Bash}}} tool to run commands."
}

let path = FileWriter.writeSkill mySkill AgentWriter.Opencode (Project ".") (fun _ -> ())
printfn "Wrote %s" path
```

# Operations

```
name : string (required)
description : string (required)
license : string
compatibility : string (e.g., "Requires X tool")
metadata : Map<string, obj> (arbitrary key/value pairs)
section : string * string
prompt : Prompt (embed a reusable prompt)
import : string (code-block wrapped)
importRaw : string (raw embed)
template : string (Fue template with {{{variable}}} and {{{tool Name}}})
templateFile : string (load template from file path)
```

# Reference

```
For template rendering and tool name injection, see Templating (Fue) in AGENTS.md.
For FileWriter API, see PROJECT.md.
```
