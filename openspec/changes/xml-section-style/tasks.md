## 1. Writers.fs — Type and Options

- [x] 1.1 Add `SectionStyle` DU (`Markdown` | `Xml`) to `Writers.fs` after the `OutputType` DU
- [x] 1.2 Add `mutable SectionStyle: SectionStyle` field to the `Options` record
- [x] 1.3 Set `SectionStyle = Markdown` in `defaultOptions`

## 2. Writers.fs — renderMd Section handling

- [x] 2.1 Update `writeNode` in `renderMd` to extract `displayName` using `RenameMap` and `HeadingFormatter` before the style branch
- [x] 2.2 Add `match opts.SectionStyle with` branch: `Markdown` case preserves existing blank-line guard and heading logic
- [x] 2.3 Add `Xml` case: emit `<displayName>`, recurse into content, emit `</displayName>` (no level-based blank-line guards)

## 3. Writers.fs — renderSkill Section handling

- [x] 3.1 Apply identical `writeNode` changes in `renderSkill` (same three steps as 2.1–2.3)

## 4. Build verification

- [x] 4.1 Run `dotnet build` — must be green before writing tests

## 5. Tests — AgentWriterTests.fs

- [x] 5.1 Add test: `defaultOptions` has `SectionStyle = Markdown`
- [x] 5.2 Add test: existing Markdown rendering unchanged (top-level section → `# name`)
- [x] 5.3 Add test: `SectionStyle.Xml` top-level section renders `<name>...</name>`
- [x] 5.4 Add test: `SectionStyle.Xml` nested sections render as nested XML tags
- [x] 5.5 Add test: `SectionStyle.Xml` output contains no `#` heading lines for sections
- [x] 5.6 Add test: `RenameMap` applies to XML tag name
- [x] 5.7 Add test: `renderSkill` with `SectionStyle.Xml` renders sections as XML tags

## 6. Test and changelog

- [x] 6.1 Run `dotnet test` — must be green
- [x] 6.2 Add entry to `CHANGELOG.md` under next version for the new `SectionStyle` option
