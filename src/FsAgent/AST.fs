namespace FsAgent.AST

type DataFormat =
    | Yaml
    | Json
    | Toon
    | Unknown

/// <summary>
/// Discriminated union representing agent tool capabilities with type-safe references.
/// Tool names are automatically mapped to platform-specific names at write time based on the target AgentHarness.
/// </summary>
type Tool =
    /// <summary>File writing capability. Maps to "write" (Opencode/Copilot) or "Write" (ClaudeCode).</summary>
    | Write
    /// <summary>File editing capability. Maps to "edit" (Opencode/Copilot) or "Edit" (ClaudeCode).</summary>
    | Edit
    /// <summary>Shell command execution capability. Maps to "bash" (Opencode), "Bash" (ClaudeCode), or "execute" (Copilot).</summary>
    | Bash
    /// <summary>Shell command execution capability (preferred name). Maps to "bash" (Opencode), "Bash" (ClaudeCode), or "execute" (Copilot).</summary>
    | Shell
    /// <summary>File reading capability. Maps to "read" (Opencode/Copilot) or "Read" (ClaudeCode).</summary>
    | Read
    /// <summary>File pattern matching capability. Maps to "grep" (Opencode), "Glob" (ClaudeCode), or "search" (Copilot).</summary>
    | Glob
    /// <summary>Directory listing capability. Maps to "list" (Opencode), "Glob" (ClaudeCode), or "search" (Copilot).</summary>
    | List
    /// <summary>Language Server Protocol capability. Maps to "lsp" (Opencode), "LSP" (ClaudeCode), or not supported (Copilot).</summary>
    | LSP
    /// <summary>Execute predefined skills. Maps to "skill" for all harnesses.</summary>
    | Skill
    /// <summary>Task management write operations. Maps to "todowrite" (Opencode), ["TaskCreate", "TaskUpdate"] (ClaudeCode), or "todo" (Copilot).</summary>
    | TodoWrite
    /// <summary>Task management read operations. Maps to "todoread" (Opencode), ["TaskList", "TaskGet", "TaskUpdate"] (ClaudeCode), or "todo" (Copilot).</summary>
    | TodoRead
    /// <summary>HTTP fetching capability. Maps to "webfetch" (Opencode), "WebFetch" (ClaudeCode), or "web" (Copilot).</summary>
    | WebFetch
    /// <summary>Web search capability. Maps to not supported (Opencode), "WebSearch" (ClaudeCode), or "web" (Copilot).</summary>
    | WebSearch
    /// <summary>Ask user questions capability. Maps to "question" (Opencode), "AskUserQuestion" (ClaudeCode), or not supported (Copilot).</summary>
    | Question
    /// <summary>Task management capability. Maps to "todo" (Opencode/Copilot) or "Todo" (ClaudeCode).</summary>
    | Todo
    /// <summary>
    /// Custom tool for MCP (Model Context Protocol) tools or platform-specific extensions.
    /// The string value passes through unchanged to all harnesses.
    /// Example: Custom "mcp_database" or Custom "github_api"
    /// </summary>
    | Custom of string

type Node =
    | Text of string
    | Section of name: string * content: Node list
    | List of Node list
    | Imported of sourcePath: string * format: DataFormat * wrapInCodeBlock: bool
    | Template of text: string
    | TemplateFile of path: string

module AST =
    let fmStr (value: string) : obj = value :> obj

    let fmNum (value: float) : obj = value :> obj

    let fmBool (value: bool) : obj = value :> obj

    let fmList (value: obj list) : obj = value :> obj

    let fmMap (value: Map<string, obj>) : obj = value :> obj

    let inferFormat (path: string) : DataFormat =
        match System.IO.Path.GetExtension(path).ToLower() with
        | ".yml" | ".yaml" -> Yaml
        | ".json" -> Json
        | ".toon" -> Toon
        | _ -> Unknown

    let importRef (path: string) : Node =
        Imported(path, inferFormat path, true)

    let importRawRef (path: string) : Node =
        Imported(path, inferFormat path, false)
