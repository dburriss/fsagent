## 1. AST changes

- [x] 1.1 Add `Markdown` variant to `DataFormat` DU in `src/FsAgent/AST.fs`
- [x] 1.2 Update `inferFormat` in `AST.fs` to map `.md` and `.markdown` extensions to `Markdown`

## 2. Writer changes

- [x] 2.1 Add `Markdown -> "markdown"` case to `formatToLanguageTag` in `src/FsAgent/Writers.fs`

## 3. Tests

- [x] 3.1 Add C-category tests: `inferFormat ".md"` returns `Markdown`, `inferFormat ".markdown"` returns `Markdown`, `inferFormat ".txt"` still returns `Unknown`
- [x] 3.2 Add C-category test: `Imported(path, Markdown, false)` renders as raw text (no code block)
- [x] 3.3 Add C-category test: `Imported(path, Markdown, true)` renders with ` ```markdown ` fenced code block
- [x] 3.4 Add A-category test: `sectionFrom "Build & Test" "some.md"` in a `prompt { }` CE produces `Section("Build & Test", [Imported("some.md", Markdown, false)])`
- [x] 3.5 Run `dotnet test` and confirm all tests pass

## 4. DSL builder changes

- [x] 4.1 Add `sectionFrom` custom operation to `src/FsAgent/Prompt.fs`
- [x] 4.2 Add `sectionFrom` custom operation to `src/FsAgent/Agent.fs`
- [x] 4.3 Add `sectionFrom` custom operation to `src/FsAgent/Skill.fs`
- [x] 4.4 Add `sectionFrom` custom operation to `src/FsAgent/Command.fs`
- [x] 4.5 Run `dotnet build` and confirm no errors

## 5. Extract AGENTS.fsx content to files

- [x] 5.1 Create `knowledge/agents/` directory
- [x] 5.2 Extract `FsAgent` section content to `knowledge/agents/fsagent.md`
- [x] 5.3 Extract `General` section content to `knowledge/agents/general.md`
- [x] 5.4 Extract `Tech Stack` section content to `knowledge/agents/tech-stack.md`
- [x] 5.5 Extract `Build & Test` section content to `knowledge/agents/build-and-test.md`
- [x] 5.6 Extract `Repository Overview` section content to `knowledge/agents/repository-overview.md`
- [x] 5.7 Extract `Templating (Fue)` section content to `knowledge/agents/templating.md`
- [x] 5.8 Extract `Coding Guidelines` section content to `knowledge/agents/coding-guidelines.md`
- [x] 5.9 Extract `Git Conventions` section content to `knowledge/agents/git-conventions.md`

## 6. Rewrite AGENTS.fsx

- [x] 6.1 Rewrite `AGENTS.fsx` to use `sectionFrom` for all 8 sections (replacing inline triple-quoted strings)
- [x] 6.2 Run `dotnet fsi AGENTS.fsx` to regenerate `AGENTS.md` and `CLAUDE.md`
- [x] 6.3 Verify regenerated output matches previous content exactly

## 7. Changelog

- [x] 7.1 Add `feat: add Markdown DataFormat variant` entry to `CHANGELOG.md`
- [x] 7.2 Add `feat: add sectionFrom DSL operation to all builders` entry to `CHANGELOG.md`
