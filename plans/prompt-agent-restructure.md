# Plan: Add Prompt Module and Refactor FsAgent

## Overview

Refactor FsAgent to separate Prompt and Agent concerns by:
1. Adding Prompt as a first-class type (same structure as Agent)
2. Extending Node types to support templates (like Imported nodes)
3. Splitting Library.fs into domain-focused files (AST.fs, Prompt.fs, Agent.fs, Writers.fs)
4. Integrating Fue template rendering at write time
5. Creating PromptBuilder CE and updating AgentBuilder (breaking change)

## Key Design Decisions

- **Template as Nodes**: Template/TemplateFile are Node types (like Imported), appended to Sections list, order matters
- **Prompt structure**: Same as Agent (`Frontmatter + Sections`), but writer doesn't output frontmatter block
- **Breaking change**: Remove role/objective/instructions/context/output/examples from AgentBuilder
- **File organization**: Domain-focused (AST.fs → Prompt.fs → Agent.fs → Writers.fs)
- **Template rendering**: At write time (in writer), using Fue with variables from options
- **Namespace organization**: FsAgent.AST, FsAgent.Prompts, FsAgent.Agents, FsAgent.Writers with AutoOpen builders

## Implementation Steps

### 1. Add Fue Dependency

**File**: `src/FsAgent/FsAgent.fsproj`

Add after line 45 (after YamlDotNet):
```xml
<PackageReference Include="Fue" Version="2.2.0" />
```

### 2. Create AST.fs (Foundation Layer)

**New file**: `src/FsAgent/AST.fs`

Move from Library.fs:
- `DataFormat` enum (lines 9-13)
- `Node` discriminated union (lines 15-19) - **EXTEND with Template/TemplateFile**
- `AST` module (lines 26-69) - **REMOVE prompt-specific constructors** (role, objective, instructions, context, output, example, examples)

```fsharp
namespace FsAgent.AST

type DataFormat =
    | Yaml
    | Json
    | Toon
    | Unknown

type Node =
    | Text of string
    | Section of name: string * content: Node list
    | List of Node list
    | Imported of sourcePath: string * format: DataFormat * wrapInCodeBlock: bool
    | Template of text: string                      // NEW
    | TemplateFile of path: string                  // NEW

module AST =
    // Keep: inferFormat, importRef, importRawRef, fmStr, fmNum, fmBool, fmList, fmMap
    // Remove: role, objective, instructions, context, output, example, examples
    // (These move to Prompt module)

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
```

### 3. Create Prompt.fs (Prompt Domain)

**New file**: `src/FsAgent/Prompt.fs`

```fsharp
namespace FsAgent.Prompts

open FsAgent.AST

type Prompt = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module Prompt =
    // Moved from AST module
    let role (text: string) : Node =
        Section("role", [Text text])

    let objective (text: string) : Node =
        Section("objective", [Text text])

    let instructions (text: string) : Node =
        Section("instructions", [Text text])

    let context (text: string) : Node =
        Section("context", [Text text])

    let output (text: string) : Node =
        Section("output", [Text text])

    let example (title: string) (content: string) : Node =
        Section("example", [Text title; Text content])

    let examples (examples: Node list) : Node =
        Section("examples", examples)

    let empty : Prompt = {
        Frontmatter = Map.empty
        Sections = []
    }

// AutoOpen makes the builder available when opening FsAgent.Prompts
[<AutoOpen>]
module PromptBuilder =

    type PromptBuilder() =
        member _.Yield _ = Prompt.empty

        member _.Run(prompt) = prompt

        // Metadata operations (stored in Frontmatter but not written as frontmatter)
        [<CustomOperation("meta")>]
        member _.Meta(prompt, frontmatter: Map<string, obj>) =
            { prompt with Frontmatter = frontmatter }

        [<CustomOperation("name")>]
        member _.Name(prompt, value: string) =
            let fm = prompt.Frontmatter |> Map.add "name" (AST.fmStr value)
            { prompt with Frontmatter = fm }

        [<CustomOperation("description")>]
        member _.Description(prompt, value: string) =
            let fm = prompt.Frontmatter |> Map.add "description" (AST.fmStr value)
            { prompt with Frontmatter = fm }

        [<CustomOperation("author")>]
        member _.Author(prompt, value: string) =
            let fm = prompt.Frontmatter |> Map.add "author" (AST.fmStr value)
            { prompt with Frontmatter = fm }

        [<CustomOperation("version")>]
        member _.Version(prompt, value: string) =
            let fm = prompt.Frontmatter |> Map.add "version" (AST.fmStr value)
            { prompt with Frontmatter = fm }

        [<CustomOperation("license")>]
        member _.License(prompt, value: string) =
            let fm = prompt.Frontmatter |> Map.add "license" (AST.fmStr value)
            { prompt with Frontmatter = fm }

        // Content operations (append to Sections)
        [<CustomOperation("role")>]
        member _.Role(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.role text] }

        [<CustomOperation("objective")>]
        member _.Objective(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.objective text] }

        [<CustomOperation("instructions")>]
        member _.Instructions(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.instructions text] }

        [<CustomOperation("context")>]
        member _.Context(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.context text] }

        [<CustomOperation("output")>]
        member _.Output(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.output text] }

        [<CustomOperation("examples")>]
        member _.Examples(prompt, examples: Node list) =
            { prompt with Sections = prompt.Sections @ [Prompt.examples examples] }

        [<CustomOperation("section")>]
        member _.Section(prompt, name: string, content: string) =
            { prompt with Sections = prompt.Sections @ [Section(name, [Text content])] }

        // Import operations
        [<CustomOperation("import")>]
        member _.Import(prompt, path: string) =
            { prompt with Sections = prompt.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(prompt, path: string) =
            { prompt with Sections = prompt.Sections @ [AST.importRawRef path] }

        // Template operations (NEW - like import/importRaw)
        [<CustomOperation("template")>]
        member _.Template(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Template text] }

        [<CustomOperation("templateFile")>]
        member _.TemplateFile(prompt, path: string) =
            { prompt with Sections = prompt.Sections @ [TemplateFile path] }

    let prompt = PromptBuilder()
```

### 4. Create Agent.fs (Agent Domain)

**New file**: `src/FsAgent/Agent.fs`

```fsharp
namespace FsAgent.Agents

open FsAgent.AST
open FsAgent.Prompts

type Agent = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module Agent =
    let empty : Agent = {
        Frontmatter = Map.empty
        Sections = []
    }

// AutoOpen makes builders available when opening FsAgent.Agents
[<AutoOpen>]
module AgentBuilder =

    type MetaBuilder() =
        member _.Yield _ = Map.empty<string, obj>
        member _.Run(map) = map

        [<CustomOperation("kv")>]
        member _.Kv(map, key: string, value: string) =
            map |> Map.add key (AST.fmStr value)

        [<CustomOperation("kvList")>]
        member _.KvList(map, key: string, value: string list) =
            map |> Map.add key (AST.fmList (value |> List.map box))

        [<CustomOperation("kvObj")>]
        member _.KvObj(map, key: string, value: Map<string, obj>) =
            map |> Map.add key (AST.fmMap value)

        [<CustomOperation("kvListObj")>]
        member _.KvListObj(map, key: string, value: obj list) =
            map |> Map.add key (AST.fmList value)

    let meta = MetaBuilder()

    type AgentBuilder() =
        member _.Yield _ = Agent.empty

        member _.Run(agent) = agent

        [<CustomOperation("meta")>]
        member _.Meta(agent, frontmatter: Map<string, obj>) =
            { agent with Frontmatter = frontmatter }

        [<CustomOperation("name")>]
        member _.Name(agent, value: string) =
            let fm = agent.Frontmatter |> Map.add "name" (AST.fmStr value)
            { agent with Frontmatter = fm }

        [<CustomOperation("description")>]
        member _.Description(agent, value: string) =
            let fm = agent.Frontmatter |> Map.add "description" (AST.fmStr value)
            { agent with Frontmatter = fm }

        [<CustomOperation("author")>]
        member _.Author(agent, value: string) =
            let fm = agent.Frontmatter |> Map.add "author" (AST.fmStr value)
            { agent with Frontmatter = fm }

        [<CustomOperation("version")>]
        member _.Version(agent, value: string) =
            let fm = agent.Frontmatter |> Map.add "version" (AST.fmStr value)
            { agent with Frontmatter = fm }

        [<CustomOperation("license")>]
        member _.License(agent, value: string) =
            let fm = agent.Frontmatter |> Map.add "license" (AST.fmStr value)
            { agent with Frontmatter = fm }

        [<CustomOperation("model")>]
        member _.Model(agent, value: string) =
            let fm = agent.Frontmatter |> Map.add "model" (AST.fmStr value)
            { agent with Frontmatter = fm }

        [<CustomOperation("temperature")>]
        member _.Temperature(agent, value: float) =
            let fm = agent.Frontmatter |> Map.add "temperature" (AST.fmNum value)
            { agent with Frontmatter = fm }

        [<CustomOperation("maxTokens")>]
        member _.MaxTokens(agent, value: int) =
            let fm = agent.Frontmatter |> Map.add "maxTokens" (box value)
            { agent with Frontmatter = fm }

        [<CustomOperation("tools")>]
        member _.Tools(agent, tools: string list) =
            let fm = agent.Frontmatter |> Map.add "tools" (AST.fmList (tools |> List.map box))
            { agent with Frontmatter = fm }

        // Prompt reference (embeds prompt sections)
        [<CustomOperation("prompt")>]
        member _.Prompt(agent, prompt: Prompt) =
            // Merge prompt sections into agent sections
            { agent with Sections = agent.Sections @ prompt.Sections }

        // Generic section operation
        [<CustomOperation("section")>]
        member _.Section(agent, name: string, content: string) =
            { agent with Sections = agent.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("import")>]
        member _.Import(agent, path: string) =
            { agent with Sections = agent.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(agent, path: string) =
            { agent with Sections = agent.Sections @ [AST.importRawRef path] }

    let agent = AgentBuilder()
```

**Note**: AgentBuilder NO LONGER has role/objective/instructions/context/output/examples operations (breaking change).

### 5. Create Writers.fs (Serialization Layer)

**New file**: `src/FsAgent/Writers.fs`

Key changes:
1. Add Template module with Fue integration
2. Extend `writeMd` to handle Template/TemplateFile nodes (like Imported)
3. Add `writePrompt` function (similar to writeMarkdown but no frontmatter output)
4. Keep `writeMarkdown` as alias to `writeAgent`

```fsharp
namespace FsAgent.Writers

open System
open System.Text
open System.IO
open FsAgent.AST
open FsAgent.Prompts
open FsAgent.Agents

module Template =
    open Fue.Data
    open Fue.Compiler

    type TemplateVariables = Map<string, obj>

    let private buildData (vars: TemplateVariables) =
        vars |> Map.fold (fun state key value ->
            state |> add key value) init

    let renderInline (text: string) (variables: TemplateVariables) : string =
        try
            let data = buildData variables
            data |> fromText text
        with
        | ex -> $"[Template error: {ex.Message}]"

    let renderFile (path: string) (variables: TemplateVariables) : string =
        try
            if not (File.Exists path) then
                $"[Template file not found: {path}]"
            else
                let data = buildData variables
                data |> fromFile path
        with
        | ex -> $"[Template error: {ex.Message}]"

module MarkdownWriter =

    type AgentFormat =
        | Opencode
        | Copilot

    type OutputType =
        | Md
        | Json
        | Yaml

    type WriterContext = {
        Format: AgentFormat
        OutputType: OutputType
        Timestamp: DateTime
        AgentName: string option
        AgentDescription: string option
    }

    type Options = {
        mutable OutputFormat: AgentFormat
        mutable OutputType: OutputType
        mutable DisableCodeBlockWrapping: bool
        mutable RenameMap: Map<string, string>
        mutable HeadingFormatter: (string -> string) option
        mutable GeneratedFooter: (WriterContext -> string) option
        mutable IncludeFrontmatter: bool
        mutable CustomWriter: (Agent -> Options -> string) option
        mutable TemplateVariables: Map<string, obj>  // NEW: For template rendering
    }

    let defaultOptions() = {
        OutputFormat = Opencode
        OutputType = Md
        DisableCodeBlockWrapping = false
        RenameMap = Map.empty
        HeadingFormatter = None
        GeneratedFooter = None
        IncludeFrontmatter = true
        CustomWriter = None
        TemplateVariables = Map.empty  // NEW
    }

    // [Keep existing private helper functions from Library.fs lines 172-303]
    // - applyHeadingFormatter
    // - formatFrontmatter
    // - loadImportContent
    // - nodeToObj (extend for Template/TemplateFile)
    // - writeMd (extend for Template/TemplateFile nodes)
    // - writeJson
    // - writeYaml

    // NEW: Write prompt (no frontmatter output)
    let writePrompt (prompt: Prompt) (configure: Options -> unit) : string =
        let opts = defaultOptions()
        configure opts

        let promptName = prompt.Frontmatter.TryFind "name" |> Option.map string
        let promptDescription = prompt.Frontmatter.TryFind "description" |> Option.map string
        let ctx = {
            Format = opts.OutputFormat
            OutputType = opts.OutputType
            Timestamp = DateTime.Now
            AgentName = promptName
            AgentDescription = promptDescription
        }

        // Convert to Agent-like structure but DON'T include frontmatter
        let agentLike = { Frontmatter = Map.empty; Sections = prompt.Sections }

        match opts.OutputType with
        | Md -> writeMd agentLike opts ctx
        | Json -> writeJson agentLike opts ctx
        | Yaml -> writeYaml agentLike opts ctx

    // Keep existing writeMarkdown for agents (extend writeMd to handle Template nodes)
    let writeAgent (agent: Agent) (configure: Options -> unit) : string =
        // [Keep existing implementation from Library.fs lines 320-348]
        // Extend to handle Template/TemplateFile nodes in writeMd
        ""

    let writeMarkdown = writeAgent  // Backward compatibility alias
```

**Note**: The `writeMd` function needs to be extended to handle Template/TemplateFile nodes:

```fsharp
// In writeMd recursive function
match node with
| Template text ->
    sb.Append(Template.renderInline text opts.TemplateVariables) |> ignore
    sb.AppendLine() |> ignore
| TemplateFile path ->
    sb.Append(Template.renderFile path opts.TemplateVariables) |> ignore
    sb.AppendLine() |> ignore
| Imported(...) -> // existing logic
| Section(...) -> // existing logic
| Text(...) -> // existing logic
| List(...) -> // existing logic
```

### 6. Update Library.fs (Re-exports for convenience)

Keep Library.fs as a compatibility/convenience layer:

```fsharp
namespace FsAgent

// Re-export types for convenience
type DataFormat = AST.DataFormat
type Node = AST.Node
type Agent = Agents.Agent
type Prompt = Prompts.Prompt

// Re-export builders for backward compatibility
module DSL =
    let meta = Agents.meta
    let agent = Agents.agent
    let prompt = Prompts.prompt

// Re-export modules
module AST = AST.AST
module MarkdownWriter = Writers.MarkdownWriter
```

**Usage Examples:**

```fsharp
// Option 1: Use specific namespaces (recommended)
open FsAgent.Prompts
let p = prompt { name "test"; role "You are..." }

open FsAgent.Agents
let a = agent { name "test"; model "gpt-4"; prompt p }

// Option 2: Access modules directly
open FsAgent.Prompts
let node = Prompt.role "text"

// Option 3: Open the module
open FsAgent.Prompts.Prompt
let node = role "text"

// Option 4: Use convenience re-exports (backward compatibility)
open FsAgent
open FsAgent.DSL
let p = prompt { ... }
let a = agent { ... }
```

### 7. Update FsAgent.fsproj Compilation Order

```xml
<ItemGroup>
    <Compile Include="AST.fs" />
    <Compile Include="Prompt.fs" />
    <Compile Include="Agent.fs" />
    <Compile Include="Writers.fs" />
    <Compile Include="Library.fs" />
</ItemGroup>
```

### 8. Create New Test Files

**tests/FsAgent.Tests/PromptTests.fs**:
- A: prompt DSL → Prompt → writePrompt pipeline tests
- B: PromptBuilder operations
- C: Format validation

**tests/FsAgent.Tests/TemplateTests.fs**:
- A: Template rendering with Fue
- A: Variable substitution
- A: Template from inline vs file
- C: Error handling

**tests/FsAgent.Tests/AgentPromptIntegrationTests.fs**:
- A: Agent referencing prompt via `prompt` operation
- A: Prompt sections merged into agent sections

### 9. Update Existing Test Files

**tests/FsAgent.Tests/AstTests.fs**:
- Update imports: `open FsAgent.AST`
- Remove tests for role/objective/instructions/context/output/example/examples (moved to PromptTests)
- Keep tests for importRef, importRawRef, inferFormat, frontmatter helpers

**tests/FsAgent.Tests/DslTests.fs**:
- Update imports: `open FsAgent.Agents` instead of `open FsAgent.DSL`
- Remove tests for agent builder role/objective/instructions operations
- Keep meta builder tests

**tests/FsAgent.Tests/MarkdownWriterTests.fs**:
- Update imports: `open FsAgent.Writers`
- Add tests for backward compatibility (writeMarkdown still works)
- Extend tests to verify Template/TemplateFile node handling

### 10. Update FsAgent.Tests.fsproj

```xml
<ItemGroup>
    <Compile Include="AstTests.fs" />
    <Compile Include="PromptTests.fs" />
    <Compile Include="TemplateTests.fs" />
    <Compile Include="DslTests.fs" />
    <Compile Include="MarkdownWriterTests.fs" />
    <Compile Include="AgentPromptIntegrationTests.fs" />
    <Compile Include="Program.fs" />
</ItemGroup>
```

## Migration Guide for Users

### Before (Old API):
```fsharp
open FsAgent
open FsAgent.DSL

let agent = agent {
    role "You are a code reviewer"
    objective "Find bugs"
    instructions "Check for edge cases"
}
```

### After (New API):
```fsharp
open FsAgent.Prompts
open FsAgent.Agents
open FsAgent.Writers

let reviewPrompt = prompt {
    name "code-review-prompt"
    role "You are a code reviewer"
    objective "Find bugs"
    instructions "Check for edge cases"
}

let myAgent = agent {
    name "code-reviewer"
    model "gpt-4"
    prompt reviewPrompt
}
```

### Template Usage:
```fsharp
open FsAgent.Prompts
open FsAgent.Writers

let reviewPrompt = prompt {
    template "Hello {{{username}}}, review {{{filename}}}"
    role "You are a code reviewer"
}

let output = MarkdownWriter.writePrompt reviewPrompt (fun opts ->
    opts.TemplateVariables <- Map.ofList [
        "username", "Alice" :> obj
        "filename", "Library.fs" :> obj
    ])
```

### Backward Compatibility (still works):
```fsharp
open FsAgent
open FsAgent.DSL

// Can still use DSL re-exports
let p = prompt { ... }
let a = agent { ... }
```

## Verification

After implementation:

1. **Build**: `dotnet build` succeeds with no errors
2. **Tests**: `dotnet test` - all tests pass
3. **Backward compatibility**: Existing code using `agent { ... }` with `meta` operations still works
4. **New functionality**:
   - Can create prompts with `prompt { ... }`
   - Can reference prompts in agents with `prompt promptExample`
   - Templates render with Fue using variables from options
   - Prompts don't output frontmatter blocks
5. **File organization**: Library split into AST.fs, Prompt.fs, Agent.fs, Writers.fs

## Critical Files

- `/Users/devon.burriss/Documents/GitHub/fsagent/main/src/FsAgent/Library.fs` - Split into 4 files
- `/Users/devon.burriss/Documents/GitHub/fsagent/main/src/FsAgent/FsAgent.fsproj` - Update compilation order, add Fue
- `/Users/devon.burriss/Documents/GitHub/fsagent/main/tests/FsAgent.Tests/` - Create PromptTests.fs, TemplateTests.fs, AgentPromptIntegrationTests.fs
- `/Users/devon.burriss/Documents/GitHub/fsagent/main/tests/FsAgent.Tests/AstTests.fs` - Remove prompt-related tests
- `/Users/devon.burriss/Documents/GitHub/fsagent/main/tests/FsAgent.Tests/DslTests.fs` - Update for new structure
- `/Users/devon.burriss/Documents/GitHub/fsagent/main/tests/FsAgent.Tests/MarkdownWriterTests.fs` - Extend for templates
