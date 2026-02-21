// Run `dotnet fsi examples/agents-md.fsx` from the repo root to emit AGENTS.md.
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open System.IO
open FsAgent.Commands
open FsAgent.Prompts
open FsAgent.Writers

let scriptDir = __SOURCE_DIRECTORY__

// A slash command that generates a minimal, reusable AGENTS.md file.
// AGENTS.md is read by AI coding agents at session start to orient them
// to the project: key files, guidelines, tech stack, and build/test commands.

let agentsMdPrompt =
    prompt {
        objective """AGENTS.md is placed at the repository root and read automatically by AI coding agents.
It orients the agent to the project: what matters, how to build and test, and any conventions to follow.
Keep it concise — the agent reads the whole file at session start."""
        instructions """1. Identify the key files an agent should know about (README, architecture docs, etc.).
2. Fill in the tech stack (language, frameworks, notable libraries).
3. Document the build and test commands exactly as run from the repo root.
4. Add any project-specific coding conventions or constraints.
5. Write the file to `AGENTS.md` in the repository root."""
        output """Key files:
- `README.md`
- `<architecture or design doc>`

# General
- Keep answers succinct and information-dense.
- Prefer simplicity and incremental changes.
- State uncertainty explicitly when information is incomplete.
- Work in small steps and verify each change.

# Tech Stack
- <language and version>
- <primary frameworks>
- <notable libraries>

# Build & Test
```bash
<build command>     # e.g. dotnet build / npm run build
<test command>      # e.g. dotnet test / npm test
```

# Coding Guidelines
- <convention 1>
- <convention 2>

# Git Conventions
- <branch naming>
- <commit message format>"""
    }

let agentsMdCommand =
    command {
        // name "create-agents-md"
        description "Generate a minimal AGENTS.md file for your project"
        prompt agentsMdPrompt
    }

let outputPath = Path.Combine(scriptDir, "AGENTS.md.example")

let markdown = AgentWriter.renderCommand agentsMdCommand (fun _ -> ())

File.WriteAllText(outputPath, markdown)
printfn "Wrote %s" outputPath
printfn ""
printfn "Preview:"
printfn "--------"
printfn "%s" markdown
