// Run `dotnet fsi examples/opencode-skill-factory/opencode-skill-factory.fsx` from the repo root.
//
// Demonstrates the FileWriter convenience API by generating three artifacts
// and writing them directly into the local .opencode directory:
//
//   .opencode/skills/create-agent/SKILL.md      — teaches the agent how to define agents
//   .opencode/skills/create-command/SKILL.md    — teaches the agent how to define commands
//   .opencode/skills/create-skill/SKILL.md      — teaches the agent how to define skills
//   .opencode/agents/fsagent-author.md          — sub-agent that orchestrates all three
//   .opencode/commands/new-fsagent-artifact.md  — slash command to invoke the sub-agent

#r "../../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"
// Fue is a transitive dependency required for Template node rendering.
#r "nuget: Fue, 2.2.0"

open FsAgent.Agents
open FsAgent.Commands
open FsAgent.Prompts
open FsAgent.Skills
open FsAgent.Tools
open FsAgent.Writers

// The project root is two directories above /examples/opencode-skill-factory.
let projectRoot =
    System.IO.Path.GetFullPath(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))

let scope = Project projectRoot

// ── Skills ───────────────────────────────────────────────────────────────────

let createAgentSkill =
    skill {
        name "create-agent"
        description "Teach an agent how to author a new agent definition using the fsagent F# DSL."
        license "MIT"
        metadata (Map.ofList [
            "author",  box "fsagent example"
            "version", box "1.0.0"
        ])

        section "Overview" """An *agent* in fsagent is defined with the `agent { ... }` computation expression.
It combines frontmatter metadata (name, description, model, tools) with a prompt body.

Key rule: the `name` field is required — it becomes the output filename."""

        section "Minimal Example" """```fsharp
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
```"""

        section "Adding Tools" """Restrict or allow specific tools using `tools` and `disallowedTools`.

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
`Question`, `Skill`, `TodoWrite`, `TodoRead`, `Custom "tool-name"`."""

        section "Workflow" """1. Define the agent with `agent { ... }`.
2. Call `FileWriter.writeAgent agent harness scope configure` to render and write.
3. Restart the harness (or reload agents) so the new agent is available."""
    }

let createCommandSkill =
    skill {
        name "create-command"
        description "Teach an agent how to author a new slash command using the fsagent F# DSL."
        license "MIT"
        metadata (Map.ofList [
            "author",  box "fsagent example"
            "version", box "1.0.0"
        ])

        section "Overview" """A *slash command* is defined with the `command { ... }` computation expression.
It requires a `name` and `description`. The body is a prompt the agent runs when the command is invoked."""

        section "Minimal Example" """```fsharp
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
```"""

        section "Reusing a Prompt" """Compose a `prompt { ... }` and embed it with the `prompt` operation:

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
```"""

        section "Using $ARGUMENTS" """Add `$ARGUMENTS` anywhere in section text to forward the text the user types after the command name.

```fsharp
let searchCommand =
    command {
        name        "search-code"
        description "Search the codebase for a pattern."
        section "Task" "Search the codebase for: $ARGUMENTS"
    }
```"""

        section "Workflow" """1. Define the command with `command { ... }`.
2. Call `FileWriter.writeCommand cmd harness scope namespace_ configure`.
   Pass `None` for namespace_ unless targeting ClaudeCode subdirectories.
3. The command is immediately available as `/name` in the harness."""
    }

let createSkillSkill =
    skill {
        name "create-skill"
        description "Teach an agent how to author a new skill using the fsagent F# DSL."
        license "MIT"
        metadata (Map.ofList [
            "author",  box "fsagent example"
            "version", box "1.0.0"
        ])

        section "Overview" """A *skill* is defined with the `skill { ... }` computation expression.
Skills are loaded on demand and teach the agent a focused domain of knowledge or behaviour.
They require `name` and `description` in the frontmatter."""

        section "Minimal Example" """```fsharp
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
```"""

        section "Metadata Fields" """Optional frontmatter fields:

| Field           | Purpose                              |
|-----------------|--------------------------------------|
| `license`       | License identifier (e.g. "MIT")      |
| `compatibility` | Prerequisite note (e.g. tool needed) |
| `metadata`      | Arbitrary key/value map              |"""

        section "Template Nodes" """Use `template` to inject harness-aware tool references at render time.
Use triple braces — **never double braces**:

```fsharp
skill {
    name "bash-skill"
    description "Use Bash for shell commands."
    template "Run commands with the {{{tool Bash}}} tool."
}
```

The `{{{tool Bash}}}` placeholder resolves to the harness-correct tool name."""

        section "Workflow" """1. Define the skill with `skill { ... }`.
2. Call `FileWriter.writeSkill skill harness scope configure`.
3. In the harness, load the skill by name to inject its instructions."""
    }

// ── Agent ────────────────────────────────────────────────────────────────────

let authorPrompt =
    prompt {
        role """You are an expert in the fsagent F# DSL. You author agents, commands, and skills
and write them to the correct .opencode locations using the FileWriter API."""
        instructions """When asked to create an artifact:
1. Identify whether the user wants an agent, command, or skill.
2. Load the corresponding skill for detailed guidance:
   - `/skill create-agent` for agents
   - `/skill create-command` for commands
   - `/skill create-skill` for skills
3. Write a self-contained `.fsx` script using the fsagent DSL and FileWriter.
4. Run the script with `dotnet fsi <script.fsx>` from the project root.
5. Confirm the output path and show a preview of the first 20 lines."""
        context """The fsagent library is a F# DSL for generating AI agent files.
Key namespaces: FsAgent.Agents, FsAgent.Commands, FsAgent.Skills, FsAgent.Writers.
FileWriter convenience functions: writeAgent, writeCommand, writeSkill.
All take (artifact, harness, scope, configure) — scope is usually `Project "."`.
Always build the project first with `dotnet build` if the DLL may be stale."""
    }

let fsagentAuthor =
    agent {
        name        "fsagent-author"
        description "Creates agents, commands, and skills using the fsagent F# DSL and writes them to .opencode."
        tools       [ Read; Glob; Bash; Write ]
        prompt      authorPrompt
    }

// ── Command ──────────────────────────────────────────────────────────────────

let newArtifactCommand =
    command {
        name        "new-fsagent-artifact"
        description "Create a new agent, command, or skill with the fsagent DSL. Usage: /new-fsagent-artifact <type> <name>"
        section "Task" """Create a new fsagent artifact.

Arguments: $ARGUMENTS

Steps:
1. Parse the arguments: the first word is the artifact type (agent | command | skill),
   the remainder is the desired name.
2. Invoke the `fsagent-author` sub-agent with that context.
3. The sub-agent will load the appropriate skill and produce a runnable .fsx script.
4. Run the script and confirm the written path."""
    }

// ── Write all artifacts ───────────────────────────────────────────────────────

let harness = AgentWriter.Opencode

let written = [
    FileWriter.writeSkill   createAgentSkill   harness scope (fun _ -> ())
    FileWriter.writeSkill   createCommandSkill harness scope (fun _ -> ())
    FileWriter.writeSkill   createSkillSkill   harness scope (fun _ -> ())
    FileWriter.writeAgent   fsagentAuthor      harness scope (fun _ -> ())
    FileWriter.writeCommand newArtifactCommand harness scope None (fun _ -> ())
]

written |> List.iter (printfn "Wrote %s")
