# Builders Reference

Computation expressions for defining agent artifacts.

## Namespaces

```fsharp
open FsAgent.Prompts    // prompt { }
open FsAgent.Agents     // agent { }, meta { }
open FsAgent.Skills     // skill { }
open FsAgent.Commands   // command { }
open FsAgent.Tools      // Tool DU
```

---

## `prompt { }`

Produces a `Prompt` value. Prompts are reusable and can be embedded into agents, skills, and commands via `prompt <value>`.

| Operation | Argument | Notes |
|-----------|----------|-------|
| `name` | `string` | Frontmatter `name` |
| `description` | `string` | Frontmatter `description` |
| `author` | `string` | Frontmatter `author` |
| `version` | `string` | Frontmatter `version` |
| `license` | `string` | Frontmatter `license` |
| `role` | `string` | Adds a `## Role` section |
| `objective` | `string` | Adds an `## Objective` section |
| `instructions` | `string` | Adds an `## Instructions` section |
| `context` | `string` | Adds a `## Context` section |
| `output` | `string` | Adds an `## Output` section |
| `examples` | `Node list` | Adds an `## Examples` section; build items with `Prompt.example title body` |
| `section` | `name, content` | Arbitrary named section |
| `import` | `path: string` | Structured import (parsed as TOON/JSON/YAML) |
| `importRaw` | `path: string` | Raw file inclusion (no parsing) |
| `template` | `string` | Inline Fue template string (use `{{{var}}}`) |
| `templateFile` | `path: string` | Fue template loaded from file |
| `meta` | `Map<string,obj>` | Replace entire frontmatter map |

```fsharp
let reviewPrompt = prompt {
    name "code-review"
    role "You are a senior code reviewer."
    objective "Find bugs and design issues."
    instructions "Be concise. Cite line numbers."
    examples [
        Prompt.example "Missing null check" "Always guard nullable inputs."
    ]
}
```

---

## `agent { }`

Produces an `Agent` value.

| Operation | Argument | Notes |
|-----------|----------|-------|
| `name` | `string` | **Required** — used as output filename |
| `description` | `string` | |
| `author` | `string` | |
| `version` | `string` | |
| `license` | `string` | |
| `model` | `string` | e.g. `"gpt-4.1"`, `"claude-sonnet-4"` |
| `temperature` | `float` | Sampling temperature |
| `maxTokens` | `float` | Token budget |
| `tools` | `Tool list` | Type-safe tool list; see [Tool configuration](agent-harness-tools.md) |
| `disallowedTools` | `Tool list` | Tools to explicitly block |
| `prompt` | `Prompt` | Merges all sections from a `Prompt` value |
| `section` | `name, content` | Arbitrary named section |
| `import` | `path: string` | Structured import |
| `importRaw` | `path: string` | Raw file inclusion |
| `meta` | `Map<string,obj>` | Replace entire frontmatter map |

```fsharp
open FsAgent.Tools

let reviewAgent = agent {
    name "code-reviewer"
    description "Reviews code for correctness and risk"
    model "claude-sonnet-4"
    temperature 0.2
    tools [Read; Grep; Bash]
    prompt reviewPrompt
}
```

### `meta { }` builder

For custom frontmatter keys not covered by the typed operations:

```fsharp
let extraMeta = meta {
    kv "custom-key" "value"
    kvList "tags" ["fsharp"; "review"]
}

let myAgent = agent {
    name "my-agent"
    meta extraMeta
}
```

| Operation | Arguments | Notes |
|-----------|-----------|-------|
| `kv` | `key, value: string` | Single string value |
| `kvList` | `key, value: string list` | List of strings |
| `kvObj` | `key, Map<string,obj>` | Nested object |
| `kvListObj` | `key, obj list` | List of objects |

---

## `skill { }`

Produces a `Skill` value.

| Operation | Argument | Notes |
|-----------|----------|-------|
| `name` | `string` | **Required** — used as subdirectory name |
| `description` | `string` | |
| `license` | `string` | |
| `compatibility` | `string` | e.g. `"claude,opencode"` |
| `metadata` | `Map<string,obj>` | Additional frontmatter |
| `prompt` | `Prompt` | Merges sections |
| `section` | `name, content` | Arbitrary section |
| `import` | `path: string` | Structured import |
| `importRaw` | `path: string` | Raw file inclusion |
| `template` | `string` | Inline Fue template |
| `templateFile` | `path: string` | Fue template from file |

```fsharp
let reviewSkill = skill {
    name "code-review"
    description "Guides an agent through a structured code review"
    compatibility "claude,opencode,copilot"
    prompt reviewPrompt
    section "workflow" "1. Read the diff\n2. Identify risk areas\n3. Report findings"
}
```

---

## `command { }`

Produces a `SlashCommand` value.

| Operation | Argument | Notes |
|-----------|----------|-------|
| `name` | `string` | **Required** — slash command name |
| `description` | `string` | |
| `prompt` | `Prompt` | Merges sections |
| `section` | `name, content` | Arbitrary section |
| `import` | `path: string` | Structured import |
| `importRaw` | `path: string` | Raw file inclusion |
| `template` | `string` | Inline Fue template |
| `templateFile` | `path: string` | Fue template from file |

```fsharp
let reviewCmd = command {
    name "review"
    description "Run a structured code review on the current diff"
    prompt reviewPrompt
}
```

---

## Rendering to string

Use `AgentWriter` to render any artifact to a string without writing to disk:

```fsharp
open FsAgent.Writers

let markdown = AgentWriter.renderAgent reviewAgent (fun opts ->
    opts.OutputFormat <- AgentHarness.Opencode)

let skillMd  = AgentWriter.renderSkill reviewSkill AgentHarness.ClaudeCode (fun _ -> ())
let cmdMd    = AgentWriter.renderCommand reviewCmd (fun opts ->
    opts.OutputFormat <- AgentHarness.Copilot)
```

To write directly to disk, see [Writing files to disk](file-writer.md).

---

## Imports and templates

### `import` / `importRaw`

Both accept a file path relative to the working directory at render time.

- `import` — parses the file as TOON, JSON, or YAML and injects structured data.
- `importRaw` — includes the file contents verbatim.

```fsharp
let myPrompt = prompt {
    importRaw "knowledge/style-guide.md"
    import    "data/tools.json"
}
```

### Templates (Fue)

FsAgent uses [Fue](https://github.com/Dzoukr/Fue) for templating. Variables use **triple braces**:

```fsharp
let myPrompt = prompt {
    template "You are an expert in {{{language}}}. Focus on {{{topic}}}."
}
```

Double braces `{{...}}` are **not** supported — they will not be interpolated.

Use `templateFile` to load the template from a `.txt` or `.md` file instead of inlining it.
