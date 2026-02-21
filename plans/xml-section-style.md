# Plan: XML Section Style Option

## Motivation

Many modern LLMs parse structured prompts more reliably when sections use XML tags
(`<role>...</role>`) rather than Markdown headings (`## role`). This is a rendering
concern only — the AST is format-agnostic. A new `SectionStyle` option in `AgentWriter.Options`
lets callers opt in to XML output without touching the DSL or AST.

## Design

### New type — `SectionStyle` DU

Add to `src/FsAgent/Writers.fs`, alongside `OutputType` (~line 107):

```fsharp
type SectionStyle =
    | Markdown   // ## heading (current behaviour, default)
    | Xml        // <heading>...</heading>
```

### `Options` change

Add one field to the `Options` record (~line 130):

```fsharp
mutable SectionStyle: SectionStyle
```

Default in `defaultOptions` (~line 149):

```fsharp
SectionStyle = Markdown
```

### `writeNode` rendering change

Both `writeNode` implementations (`renderMd` ~line 421, `renderSkill` ~line 629) share
identical `Section` handling. The change in each:

**Current:**
```fsharp
| Section (name, content) ->
    if level = 1 then ...
    let displayName = ...
    let heading = String.replicate level "#" + " " + displayName
    sb.AppendLine(heading) |> ignore
    sb.AppendLine() |> ignore
    for c in content do writeNode c (level + 1)
```

**New:**
```fsharp
| Section (name, content) ->
    let displayName =
        opts.RenameMap |> Map.tryFind name |> Option.defaultValue name
        |> (opts.HeadingFormatter |> Option.defaultValue id)
    match opts.SectionStyle with
    | Markdown ->
        if level = 1 then
            let str = sb.ToString()
            if str.Length > 0 && not (str.EndsWith("\n\n")) then
                sb.AppendLine() |> ignore
        let heading = String.replicate level "#" + " " + displayName
        sb.AppendLine(heading) |> ignore
        sb.AppendLine() |> ignore
        for c in content do writeNode c (level + 1)
    | Xml ->
        sb.AppendLine($"<{displayName}>") |> ignore
        for c in content do writeNode c (level + 1)
        sb.AppendLine($"</{displayName}>") |> ignore
```

Notes:
- XML mode does not use `level` for indentation — nesting is conveyed by the tag structure.
- XML mode does not insert the blank-line guards (those are Markdown formatting concerns).
- `RenameMap` and `HeadingFormatter` still apply to the tag name in XML mode.
- `HeadingFormatter` in XML mode should produce valid XML element names (caller's
  responsibility — no sanitisation added here).

## Affected Files

| File | Change |
|---|---|
| `src/FsAgent/Writers.fs` | Add `SectionStyle` DU; add field to `Options`; update `defaultOptions`; update both `writeNode` implementations |
| `tests/FsAgent.Tests/AgentWriterTests.fs` | Add tests for `SectionStyle.Xml` output |
| `CHANGELOG.md` | Add entry under next version |

## Out of Scope

- No AST changes — `Section` node is unchanged.
- No JSON/YAML renderer changes — `nodeToObj` is already format-neutral.
- No `CustomWriter` signature changes.
- No XML escaping of section content — content is written verbatim, as it is today for Markdown.
- No per-section style control (mixed Markdown + XML in one document).
- No `renderSkill` public API changes.

## Execution Order

1. Add `SectionStyle` DU to `Writers.fs` (after `OutputType`)
2. Add `mutable SectionStyle: SectionStyle` to `Options` record
3. Update `defaultOptions` to set `SectionStyle = Markdown`
4. Update `writeNode` in `renderMd` to branch on `opts.SectionStyle`
5. Update `writeNode` in `renderSkill` identically
6. `dotnet build` — must be green
7. Add tests covering:
   - Default (Markdown) behaviour unchanged
   - `SectionStyle.Xml` top-level section renders `<name>...</name>`
   - `SectionStyle.Xml` nested sections render nested tags
   - `RenameMap` applies to XML tag name
8. `dotnet test` — must be green
9. Update `CHANGELOG.md`
