# Agent Harness Tools

This document provides an overview of the agent harness tools that they provide. FsAgent abstracts these away using the `Tool` type, but here are the underlying tools that are available:

FsAgent | Opencode | Claude | GH Copilot | Description
--- | --- | --- | --- | ---
Shell | `bash` | `Bash` | `execute` | Execute shell commands
Edit | `edit` | `Edit` | `edit` | Edit files using string replacement
Write | `write` | `Write` | `write` | Write files
Read | `read` | `Read` | `read` | Read files
Glob | `grep` | `Glob` | `search` | Search files using glob patterns/grep
List | `list` | `Glob` | `search` | List files in a directory
LSP | `lsp` | `LSP` | `_` | Language Server Protocol for code analysis
Skill | `skill` | `Skill` | `skill` | Execute predefined skills
TodoWrite | `todowrite` | `TaskCreate` | `todo` | Write todo items
TodoRead | `todoread` | `TaskList` | `todo` | Read todo items
WebFetch | `webfetch` | `WebFetch` | `web` | Fetch content from the web
WebSearch | `_` | `WebSearch` | `web` | Search the web
Question | `question` | `AskUserQuestion` | `_` | Ask the user a question

## Notes

- Claude also has `TaskGet` and `TaskUpdate`, which should be included in TodoRead and TodoWrite respectively.
- GH Copilot combines `TodoRead` and `TodoWrite` into a single `todo` tool as well as `WebFetch` and `WebSearch` into a single `web` tool.
- For those that are missing from FsAgent, you can create custom tools using the `Custom` constructor of the `Tool` type. See the example below:

```fsharp
open FsAgent.AST  // For Tool type
let myAgent =
    agent {
        ...
        tools [ Custom "my-custom-tool"; Read; Write ]
        ...
    }
```

## References

- [Opencode Tools](https://opencode.ai/docs/tools/)
- [Claude Tools](https://code.claude.com/docs/en/settings#tools-available-to-claude)
- [GitHub Copilot Tools](https://docs.github.com/en/copilot/reference/custom-agents-configuration#tools)
