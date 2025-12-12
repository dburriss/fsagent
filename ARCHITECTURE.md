Below is a concise architecture design document reflecting the clarified layering and the addition of non-markdown output writers (e.g., JSON). Tone is direct and engineering-focused.

---

# Architecture Design

## Overview

The system builds agent definitions through a high-level F# DSL or directly through an AST. Writers convert the AST into a final artifact (Markdown, JSON, or flavour-specific variations). Writers are *not* low-level; they are first-class output components. Low-level concerns (import pipeline, serialization, IO) live beneath both DSL and writers.

---

# Layered Architecture

```
+---------------------------------------------------------------+
| 1. Agent DSL (Top Layer)                                      |
|    - F# computation expression                                 |
|    - Declarative API for building agents                       |
|    - Role, objective, instructions, examples, context, etc.    |
|    - Does not know anything about output formats               |
+---------------------+-----------------------------------------+
                      |
                      v
+---------------------------------------------------------------+
| 2. Agent AST (Intermediate Representation)                     |
|    - Flavour-agnostic, pure data model                         |
|    - Node-based structure (Text, Section, List, Imported) |
|    - Agent root with frontmatter: Map<string, obj> and sections: Node list |
|    - Holds import references: sourcePath + declared DataFormat (yaml|json|toon)   |
+---------------------+-----------------------------------------+
                      |
                      v
+---------------------------------------------------------------+
| 3. Writers (Output Layer — High-Level)                         |
|    - Convert AST into final output                             |
|    - MarkdownWriter (baseline)                                 |
|    - JsonWriter (alternative baseline)                         |
|    - Flavour wrappers:                                         |
|         * OpenCodeWriter                                       |
|         * CopilotWriter                                        |
|         * ClaudeWriter                                         |
|    - Responsible for:                                          |
|         * frontmatter shaping                                  |
|         * layout rules                                          |
|         * serialization format for imported data                |
|    - Writers may wrap a “base” writer to extend/override.      |
|      Example: OpenCodeWriter(MarkdownWriter())                 |
+---------------------+-----------------------------------------+
                      |
                      v
+---------------------------------------------------------------+
| 4. Import Pipeline & Serialization (Low-Level)                 |
|    - File reader (IO)                                          |
|    - Format parsers: YAML → obj, JSON → obj                    |
|    - TOON serializer                                           |
|    - Re-serialization utilities: obj → TOON/JSON/YAML          |
|    - These do not know about Markdown, AST structure, or       |
|      flavour conventions                                       |
+---------------------------------------------------------------+
```

---

# Key Architectural Decisions

## 1. **Writers Are First-Class Output Components**

Writers sit *just below* the AST. They are the main API for producing deliverables.
Users will typically do:

```fsharp
let agent = agent { ... }
let output = OpenCodeWriter(MarkdownWriter()).Write(agent)
```

Design reasons:

* Writers represent concrete output formats, not “utilities”.
* Multiple formats (Markdown, JSON) require parallel writer hierarchies.
* Avoids leaking formatting rules into the DSL or AST.

---

## 2. **Writer Composition via “Base Writer”**

A flavour writer (OpenCode, Copilot, Claude) wraps a base writer to modify frontmatter and layout.

Example:

```fsharp
OpenCodeWriter(MarkdownWriter())
```

For JSON output:

```fsharp
OpenCodeWriter(JsonWriter())
```

This creates a *matrix* of possibilities without branching DSL code.

---

## 3. **Support for Multiple Output Formats (Markdown and JSON)**

Two “baselines” exist:

* `MarkdownWriter`
* `JsonWriter`

Both implement:

```fsharp
Write(agent, ?importFormat:ImportFormat) -> string
```

Flavour writers only modify frontmatter and structural rules but do not assume Markdown.

---

## 4. **Imported Data Resolution at Write Time**

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

## 5. **Stratified Design**

The DSL is the highest abstraction.
The AST is a pure intermediate representation.
Writers produce external artifacts.
Serializers and import utilities remain low-level and ignorant of writers.

This prevents semantic leakage and keeps each layer’s responsibility narrow.

---

## 6. **AST Is Immutable and Transformation-Friendly**

The AST must remain:

* immutable,
* flavour-agnostic,
* safe for transformations (e.g., merging, validation, normalization).

Writers can post-process by constructing a temporary augmented AST, but the original stays untouched.

---

# Component Responsibilities

## DSL

* Provide ergonomic domain-specific combinators.
* Hide Node/AST boilerplate.
* No IO.
* No formatting logic.
* F# Computation Expression using F# Builder

## AST

* Stable core model.
* Small surface, easy to render.
* Stores import references (path + declared format); no parsed data in the AST.
* F# Discriminated Unions and functions.

## Writers

* Decide layout, output rendering, flavour metadata.
* Reformat imports.
* Writers may perform IO for imports via lower-level modules; they do not mutate the AST and handle missing files/parse errors predictably.

## Importers & Serializers

* Perform file-system reads.
* Parse YAML/JSON into plain objects.
* Produce TOON/YAML/JSON output.
* No knowledge of writers or agent structure.

---

# Extensibility

### Adding a new DSL primitive

Add a builder combinator mapping directly to a Node.

### Adding a new output flavour

Implement:

```fsharp
type MyFlavourWriter(baseWriter: IAgentWriter) =
    interface IAgentWriter with
        member _.Write(agent, ?importFormat) =
            let augmented = agent |> addFrontMatter "flavour" "myflavour"
            baseWriter.Write(augmented, ?importFormat = importFormat)
```

### Adding a new output format (e.g., HTML)

Implement a new baseline writer:

```fsharp
type HtmlWriter() =
    interface IAgentWriter with
        member _.Write(agent, ?importFormat) = ...
```

Flavour writers automatically layer on top.

---

If you want, I can produce the equivalent **C4 diagrams**, a **full module layout**, or a **concrete F# skeleton implementation**.

