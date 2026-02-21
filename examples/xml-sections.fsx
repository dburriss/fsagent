// Run `dotnet fsi examples/xml-sections.fsx` from the repo root.
// Demonstrates SectionStyle.Xml — renders Section nodes as <tag>...</tag>
// instead of Markdown headings. Useful when targeting LLMs that parse
// structured prompts more reliably with XML tags.
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.Writers

let reviewPrompt =
    prompt {
        role "You are an expert code reviewer. Be direct and specific."
        objective "Review the provided diff and surface issues by category."
        instructions """1. Read the diff carefully.
2. Group findings under the relevant section.
3. If a section has no findings, omit it."""
        output "Respond only with the structured review — no preamble."
    }

let agent =
    agent {
        description "A concise code-review assistant"
        prompt reviewPrompt
    }

// --- Markdown output (default) ---
printfn "=== Markdown (default) ==="
let markdownOutput = AgentWriter.renderAgent agent (fun opts ->
    opts.IncludeFrontmatter <- false)
printfn "%s" markdownOutput

// --- XML output ---
printfn "=== XML sections ==="
let xmlOutput =
    AgentWriter.renderAgent agent (fun opts ->
        opts.SectionStyle <- AgentWriter.Xml
        opts.IncludeFrontmatter <- false)
printfn "%s" xmlOutput

// --- XML with renamed tags ---
printfn "=== XML sections with renamed tags ==="
let xmlRenamedOutput =
    AgentWriter.renderAgent agent (fun opts ->
        opts.SectionStyle <- AgentWriter.Xml
        opts.IncludeFrontmatter <- false
        opts.RenameMap <-
            Map.ofList [
                "role",         "system"
                "objective",    "task"
                "instructions", "steps"
                "output",       "format"
            ])
printfn "%s" xmlRenamedOutput
