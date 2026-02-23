---
description: Teach an agent how to author a new slash command using the fsagent F# DSL.
license: MIT
metadata: 
  author: fsagent example
  version: 1.0.0
name: create-command
---

# Overview

A *slash command* is defined with the `command { ... }` computation expression.
It requires a `name` and `description`. The body is a prompt the agent runs when the command is invoked.

# Minimal Example

```fsharp
open FsAgent.Commands
open FsAgent.Writers

let greetCommand =
    command {
        name        "greet"
        description "Greet the user warmly."
        section "Instructions" "Say hello and introduce yourself."
    }

// Write to .opencode/commands/greet.md
let path =
    FileWriter.writeCommand greetCommand AgentWriter.Opencode (Project ".") None (fun _ -> ())
printfn "Wrote %s" path
```

# Reusing a Prompt

Compose a `prompt { ... }` and embed it with the `prompt` operation:

```fsharp
open FsAgent.Prompts
open FsAgent.Commands

let sharedPrompt =
    prompt {
        objective    "Summarise the current git diff."
        instructions "Run `git diff --staged`, then write a concise summary."
        output       "A bullet list of changed files with a one-line description each."
    }

let diffSummaryCommand =
    command {
        name        "diff-summary"
        description "Summarise the staged git diff."
        prompt sharedPrompt
    }
```

# Using $ARGUMENTS

Add `$ARGUMENTS` anywhere in section text to forward the text the user types after the command name.

```fsharp
let searchCommand =
    command {
        name        "search-code"
        description "Search the codebase for a pattern."
        section "Task" "Search the codebase for: $ARGUMENTS"
    }
```

# Workflow

1. Define the command with `command { ... }`.
2. Call `FileWriter.writeCommand cmd harness scope namespace_ configure`.
   Pass `None` for namespace_ unless targeting ClaudeCode subdirectories.
3. The command is immediately available as `/name` in the harness.
