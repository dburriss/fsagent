# Plan: `sectionFrom` DSL Operation and Markdown Format

Status: Done

## Problem

Large inline triple-quoted strings in `AGENTS.fsx` and skill `.fsx` files are hard to read and maintain. Section content should be editable as standalone files rather than embedded string literals.

## Goal

Add a `sectionFrom` DSL operation that creates a named section whose body is loaded from an external file at write time. Also add `Markdown` as a first-class `DataFormat` variant so `.md` files are handled explicitly rather than falling through to `Unknown`.

## Changes

### 1. Add `Markdown` to `DataFormat` (AST.fs)

Add `Markdown` as a new case to the `DataFormat` discriminated union:

```fsharp
type DataFormat =
    | Yaml
    | Json
    | Toon
    | Markdown
    | Unknown
```

Update `inferFormat` to map `.md` and `.markdown` extensions to `Markdown`:

```fsharp
| ".md" | ".markdown" -> Markdown
```

### 2. Handle `Markdown` in the writer (Writers.fs)

In the `resolveImportedContent` function (and/or `writeNode` for the `Imported` case), ensure `Markdown` format is treated as raw text (no code block wrapping by default, no special serialization). The language tag for code block wrapping (if ever requested explicitly) should be `"markdown"`.

### 3. Add `sectionFrom` operation to all 4 builders

Add to `Prompt.fs`, `Agent.fs`, `Skill.fs`, and `Command.fs`:

```fsharp
[<CustomOperation("sectionFrom")>]
member _.SectionFrom(state, name: string, path: string) =
    { state with Sections = state.Sections @ [Section(name, [AST.importRawRef path])] }
```

This creates `Section(name, [Imported(path, inferFormat path, false)])`. For `.md` files, `inferFormat` now returns `Markdown` instead of `Unknown`.

### 4. Add tests

**C-category (communication) test** -- verify `Section(name, [Imported(...)])` renders correctly:
- A section with an `Imported` child renders as heading + file content (no code block)
- A section with an `Imported(wrapInCodeBlock=true)` child renders as heading + fenced code block
- Markdown format is inferred for `.md` and `.markdown` extensions

**A-category (acceptance) test** -- verify `sectionFrom` DSL operation:
- `sectionFrom "Build & Test" "some.md"` produces `Section("Build & Test", [Imported("some.md", Markdown, false)])`

### 5. Extract AGENTS.fsx content to `knowledge/agents/`

Create one `.md` file per section:

| File | Section name in AGENTS.fsx |
|------|---------------------------|
| `knowledge/agents/fsagent.md` | `FsAgent` |
| `knowledge/agents/general.md` | `General` |
| `knowledge/agents/tech-stack.md` | `Tech Stack` |
| `knowledge/agents/build-and-test.md` | `Build & Test` |
| `knowledge/agents/repository-overview.md` | `Repository Overview` |
| `knowledge/agents/templating.md` | `Templating (Fue)` |
| `knowledge/agents/coding-guidelines.md` | `Coding Guidelines` |
| `knowledge/agents/git-conventions.md` | `Git Conventions` |

Each file contains exactly the string content currently embedded in the `section` call -- no heading (the heading is supplied by the `sectionFrom` operation).

### 6. Rewrite AGENTS.fsx

```fsharp
let agentsMd =
    prompt {
        sectionFrom "FsAgent"             "knowledge/agents/fsagent.md"
        sectionFrom "General"             "knowledge/agents/general.md"
        sectionFrom "Tech Stack"          "knowledge/agents/tech-stack.md"
        sectionFrom "Build & Test"        "knowledge/agents/build-and-test.md"
        sectionFrom "Repository Overview" "knowledge/agents/repository-overview.md"
        sectionFrom "Templating (Fue)"    "knowledge/agents/templating.md"
        sectionFrom "Coding Guidelines"   "knowledge/agents/coding-guidelines.md"
        sectionFrom "Git Conventions"     "knowledge/agents/git-conventions.md"
    }
```

### 7. Update CHANGELOG.md

Add entry for:
- `feat: add Markdown DataFormat variant`
- `feat: add sectionFrom DSL operation to all builders`

## Sequence

1. Add `Markdown` to `DataFormat` and update `inferFormat` (AST.fs)
2. Verify writer handles `Markdown` format correctly (Writers.fs) -- update language tag if needed
3. Add tests (A + C categories) and confirm they pass
4. Add `sectionFrom` to all 4 builders
5. Run build + tests
6. Create `knowledge/agents/` files with extracted content
7. Rewrite `AGENTS.fsx` to use `sectionFrom`
8. Run `dotnet fsi AGENTS.fsx` to regenerate `AGENTS.md` and `CLAUDE.md`
9. Verify output matches the previous content exactly
10. Update `CHANGELOG.md`

## Non-goals

- No change to how `import` (with code block wrapping) works
- No markdown structure parsing (section content is treated as opaque text)
- No changes to `templateFile` behaviour
- No new `Markdown` variant-specific writer logic beyond the language tag
