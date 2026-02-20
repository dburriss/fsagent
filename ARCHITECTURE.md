Below is a concise architecture design document reflecting the clarified layering and the addition of non-markdown output writers (e.g., JSON). Tone is direct and engineering-focused.

---

# Architecture Design

## Overview

The system builds agent definitions through a high-level F# DSL or directly through an AST. Writers convert the AST into a final artifact (Markdown, JSON, or flavour-specific variations). Writers are *not* low-level; they are first-class output components. Low-level concerns (import pipeline, serialization, IO) live beneath both DSL and writers.

---

# Layered Architecture

```
+---------------------------------------------------------------+
| 1. DSL Layer (Top Layer)                                       |
|    - F# computation expressions                                |
|    - prompt { ... } - Build reusable prompts                   |
|    - agent { ... } - Build agents referencing prompts          |
|    - Declarative API with role, objective, instructions, etc.  |
|    - Does not know anything about output formats               |
|    - Supports template operations with variable placeholders   |
+---------------------+-----------------------------------------+
                      |
                      v
+---------------------------------------------------------------+
| 2. Domain Types (Intermediate Representation)                  |
|    - Prompt: Reusable content with Frontmatter + Sections      |
|    - Agent: Configuration with Frontmatter + Sections          |
|    - Flavour-agnostic, pure data models                        |
|    - Node-based structure (Text, Section, List, Imported,      |
|      Template, TemplateFile)                                   |
|    - Holds import references and template definitions          |
+---------------------+-----------------------------------------+
                      |
                      v
+---------------------------------------------------------------+
| 3. Writers (Output Layer — High-Level)                         |
|    - Convert domain types into final output                    |
|    - AgentWriter module (in FsAgent.Writers namespace)         |
|    - Supports Md, Json, Yaml output types via Options          |
|    - renderAgent: Renders agents with frontmatter              |
|    - renderPrompt: Renders prompts without frontmatter         |
|    - renderCommand: Renders slash commands                     |
|    - Template rendering at write time with variables           |
|    - Responsible for:                                          |
|         * frontmatter shaping                                  |
|         * layout rules                                         |
|         * template variable substitution                       |
|         * serialization format for imported data               |
|    - Configurable via Options (OutputFormat, RenameMap, etc.)  |
+---------------------+-----------------------------------------+
                      |
                      v
+---------------------------------------------------------------+
| 4. Low-Level Utilities                                         |
|    - File reader (IO)                                          |
|    - Format parsers: YAML → obj, JSON → obj                    |
|    - TOON serializer                                           |
|    - Template engine (Fue integration)                         |
|    - Re-serialization utilities: obj → TOON/JSON/YAML          |
|    - These do not know about domain types or writers           |
+---------------------------------------------------------------+
```

---

# Key Architectural Decisions

## 1. **AgentWriter Module and Options-Based Dispatch**

`AgentWriter` is the single writer module. It dispatches to internal `renderMd`, `renderJson`, or `renderYaml` based on `opts.OutputType`. Harness-specific frontmatter shaping (Opencode struct format vs Copilot/ClaudeCode list format) is controlled by `opts.OutputFormat`.

Users call:

```fsharp
let agent = agent { ... }
let output = AgentWriter.renderAgent agent (fun opts ->
    opts.OutputFormat <- AgentWriter.Copilot)
```

The three public functions are:
- `renderAgent` — renders `Agent` with frontmatter
- `renderPrompt` — renders `Prompt` without frontmatter
- `renderCommand` — renders `SlashCommand` with description frontmatter

---

## 2. **Imported Data Resolution at Write Time**

Imported nodes store:

* sourcePath
* declared format (`DataFormat`)

Writers resolve the referenced files via low-level import/parsing modules and decide how to serialize them:

* Preserve original format
* Force TOON
* Force JSON
* Force YAML

This keeps the AST pure while allowing flexible output formatting.

---

## 3. **Stratified Design**

The DSL is the highest abstraction.
The AST is a pure intermediate representation.
Writers produce external artifacts.
Serializers and import utilities remain low-level and ignorant of writers.

This prevents semantic leakage and keeps each layer’s responsibility narrow.

---

## 4. **AST Is Immutable and Transformation-Friendly**

The AST must remain:

* immutable,
* flavour-agnostic,
* safe for transformations (e.g., merging, validation, normalization).

Writers can post-process by constructing a temporary augmented AST, but the original stays untouched.

---

# Component Responsibilities

## DSL

**Prompt Builder (`prompt { ... }`):**
* Create reusable prompt content with role, objective, instructions, context, output, examples
* Supports metadata: name, description, author, version, license
* Template operations: `template "text {{{var}}}"` and `templateFile "path.txt"`
* Import references via `import "path"` and `importRaw "path"`
* Generic `section` for ad hoc content
* Pure, no IO, no formatting logic

**Agent Builder (`agent { ... }`):**
* References prompts via `prompt promptInstance` operation
* Supports configuration metadata: model, temperature, maxTokens, tools
* Can reference multiple prompts (sections are merged)
* Generic `section` for ad hoc content
* Import references via `import "path"` and `importRaw "path"`
* Frontmatter building with `meta { kv ... }` for generic keys
* Pure and flavour-agnostic

## Domain Types

**Prompt:**
* Reusable content unit with Frontmatter + Sections
* Metadata stored but not serialized to output (no YAML frontmatter block)
* Can be composed into agents

**Agent:**
* Configuration root with Frontmatter + Sections
* References prompts (sections merged at build time)
* Frontmatter serialized to YAML block in output

**Node Types:**
* Text, Section, List - content structure
* Imported - file references (path + format + wrapInCodeBlock)
* Template - inline template with variable placeholders
* TemplateFile - external template file reference
* Stores references only; resolution happens at write time

## Writers

* Decide layout, output rendering, flavour metadata
* Three main public functions:
  * `renderAgent` - renders agents with frontmatter
  * `renderPrompt` - renders prompts without frontmatter
  * `renderCommand` - renders slash commands with description frontmatter
* Template rendering at write time:
  * Resolve Template nodes with configured TemplateVariables
  * Load TemplateFile nodes from disk and render
  * Uses Fue template engine ({{{variable}}} syntax)
* Reformat imports based on options
* Writers perform IO for imports and templates; they do not mutate domain types
* Handle missing files/parse errors with error messages in output
* Configurable via Options record (OutputFormat, OutputType, TemplateVariables, etc.)

## Template Engine

* Fue integration for Mustache-like templating
* Variable interpolation: `{{{name}}}` renders values
* Supports nested properties: `{{{user.name}}}`
* Renders at write time with configured TemplateVariables map
* Error handling returns descriptive messages in output

## Importers & Serializers

* Perform file-system reads
* Parse YAML/JSON into plain objects
* Produce TOON/YAML/JSON output
* No knowledge of writers or domain types

---

# Namespace Organization

The codebase is organized into domain-focused namespaces:

```
FsAgent/
├── AST.fs          → FsAgent.AST
│   ├── DataFormat (enum)
│   ├── Node (DU with Template/TemplateFile)
│   └── AST module (helpers: fmStr, fmNum, inferFormat, importRef, etc.)
│
├── Prompt.fs       → FsAgent.Prompts
│   ├── Prompt type
│   ├── Prompt module (constructors: role, objective, instructions, etc.)
│   └── PromptBuilder (AutoOpen)
│
├── Agent.fs        → FsAgent.Agents
│   ├── Agent type
│   ├── Agent module
│   ├── MetaBuilder
│   └── AgentBuilder (AutoOpen)
│
├── Writers.fs      → FsAgent.Writers
│   ├── Template module (renderInline, renderFile)
│   └── AgentWriter module (renderAgent, renderPrompt, renderCommand, Options)
│
└── Library.fs      → FsAgent (backward compatibility)
    ├── Type aliases (DataFormat, Node, Agent, Prompt)
    ├── DSL module (re-exports: meta, agent, prompt)
    ├── AST module re-export
    └── AgentWriter module re-export
```

**Import Strategy:**
```fsharp
// New code (recommended)
open FsAgent.Prompts
open FsAgent.Agents
open FsAgent.Writers

// Existing code (backward compatible)
open FsAgent
open FsAgent.DSL
```

---

# Extensibility

### Adding a new prompt operation

Add a custom operation to PromptBuilder in Prompt.fs:

```fsharp
[<CustomOperation("myOperation")>]
member _.MyOperation(prompt, value: string) =
    { prompt with Sections = prompt.Sections @ [Section("mySection", [Text value])] }
```

### Adding a new agent operation

Add a custom operation to AgentBuilder in Agent.fs:

```fsharp
[<CustomOperation("myConfig")>]
member _.MyConfig(agent, value: string) =
    { agent with Frontmatter = agent.Frontmatter |> Map.add "myConfig" (AST.fmStr value) }
```

### Adding a new node type

Extend the Node discriminated union in AST.fs:

```fsharp
type Node =
    | Text of string
    | Section of name: string * content: Node list
    | List of Node list
    | Imported of sourcePath: string * format: DataFormat * wrapInCodeBlock: bool
    | Template of text: string
    | TemplateFile of path: string
    | MyCustomNode of data: string  // New node type
```

Then update writers to handle the new case.

### Adding a new output format

Extend `AgentWriter` module or create a new writer with similar signature:

```fsharp
let renderHtml (agent: Agent) (configure: Options -> unit) : string =
    // Custom HTML generation logic
    ...
```

---

# Future: Composable Writer Hierarchy

A possible future direction is a composable `IWriter` interface pattern where harness-specific writers wrap a base writer:

```fsharp
// Not yet implemented
type IWriter =
    abstract Write: Agent -> Options -> string

OpenCodeWriter(MarkdownWriter())  // harness wraps base format
OpenCodeWriter(JsonWriter())       // same harness, different format
```

This would create a matrix of (harness × format) without branching. Deferred to post-1.0 — the current `Options`-based dispatch is sufficient for the current harness and format count.
