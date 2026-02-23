---
description: Teach an agent how to author a new skill using the fsagent F# DSL.
license: MIT
metadata: 
  author: fsagent example
  version: 1.0.0
name: create-skill
---

# Overview

A *skill* is defined with the `skill { ... }` computation expression.
Skills are loaded on demand and teach the agent a focused domain of knowledge or behaviour.
They require `name` and `description` in the frontmatter.

# Minimal Example

```fsharp
open FsAgent.Skills
open FsAgent.Writers

let mySkill =
    skill {
        name        "my-skill"
        description "Teach the agent how to do X."
        section "Instructions" "Step-by-step instructions for doing X."
    }

// Write to .opencode/skills/my-skill/SKILL.md
let path =
    FileWriter.writeSkill mySkill AgentWriter.Opencode (Project ".") (fun _ -> ())
printfn "Wrote %s" path
```

# Metadata Fields

Optional frontmatter fields:

| Field           | Purpose                              |
|-----------------|--------------------------------------|
| `license`       | License identifier (e.g. "MIT")      |
| `compatibility` | Prerequisite note (e.g. tool needed) |
| `metadata`      | Arbitrary key/value map              |

# Template Nodes

Use `template` to inject harness-aware tool references at render time.
Use triple braces — **never double braces**:

```fsharp
skill {
    name "bash-skill"
    description "Use Bash for shell commands."
    template "Run commands with the {{{tool Bash}}} tool."
}
```

The `{{{tool Bash}}}` placeholder resolves to the harness-correct tool name.

# Workflow

1. Define the skill with `skill { ... }`.
2. Call `FileWriter.writeSkill skill harness scope configure`.
3. In the harness, load the skill by name to inject its instructions.
