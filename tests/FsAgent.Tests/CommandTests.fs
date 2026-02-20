module FsAgent.Tests.CommandTests

open Xunit
open FsAgent
open FsAgent.Commands
open FsAgent.Writers
open FsAgent.AST
open System.IO

// A – Acceptance Tests: SlashCommand DSL and writeCommand writer

[<Fact>]
let ``A: command with description renders frontmatter`` () =
    let cmd =
        command {
            name "my-cmd"
            description "Does a thing"
        }
    let result = AgentWriter.renderCommand cmd (fun _ -> ())
    Assert.Contains("description: Does a thing", result)
    Assert.Contains("---", result)

[<Fact>]
let ``A: command name is not in frontmatter`` () =
    let cmd =
        command {
            name "my-cmd"
            description "Does a thing"
        }
    let result = AgentWriter.renderCommand cmd (fun _ -> ())
    // Find only the YAML front block (between first and second ---)
    let lines = result.Split('\n')
    let frontmatterLines =
        lines
        |> Array.skip 1  // skip first ---
        |> Array.takeWhile (fun l -> l.Trim() <> "---")
    let frontmatter = frontmatterLines |> String.concat "\n"
    Assert.DoesNotContain("name:", frontmatter)

[<Fact>]
let ``A: command sections render correctly`` () =
    let cmd =
        command {
            name "my-cmd"
            description "Test"
            role "You are a helper"
            instructions "Follow the steps"
            section "notes" "Extra notes here"
        }
    let result = AgentWriter.renderCommand cmd (fun _ -> ())
    Assert.Contains("# role", result)
    Assert.Contains("# instructions", result)
    Assert.Contains("# notes", result)

[<Fact>]
let ``A: command template renders with Opencode harness`` () =
    let cmd =
        command {
            name "bash-cmd"
            description "Runs bash"
            template "Use {{{tool Bash}}}"
        }
    let result = AgentWriter.renderCommand cmd (fun opts -> opts.OutputFormat <- AgentWriter.Opencode)
    Assert.Contains("bash", result)
    Assert.DoesNotContain("{{{tool Bash}}}", result)

[<Fact>]
let ``A: command import embeds file content`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "# Shared context\nSome important info")
    let cmd =
        command {
            name "ctx-cmd"
            description "Context command"
            importRaw tempFile
        }
    let result = AgentWriter.renderCommand cmd (fun _ -> ())
    Assert.Contains("Shared context", result)
    Assert.Contains("Some important info", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: command renders identically for Copilot and ClaudeCode`` () =
    let cmd =
        command {
            name "multi-harness"
            description "Cross-harness test"
            instructions "Do the thing"
        }
    let copilotResult = AgentWriter.renderCommand cmd (fun opts -> opts.OutputFormat <- AgentWriter.Copilot)
    let claudeResult = AgentWriter.renderCommand cmd (fun opts -> opts.OutputFormat <- AgentWriter.ClaudeCode)
    let opencodeResult = AgentWriter.renderCommand cmd (fun opts -> opts.OutputFormat <- AgentWriter.Opencode)
    Assert.Equal(opencodeResult, copilotResult)
    Assert.Equal(opencodeResult, claudeResult)

[<Fact>]
let ``A: empty command renders only description frontmatter`` () =
    let cmd =
        command {
            description ""
        }
    let result = AgentWriter.renderCommand cmd (fun _ -> ())
    Assert.Contains("---", result)
    Assert.Contains("description:", result)
    Assert.DoesNotContain("# ", result)
