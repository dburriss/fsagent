## 1. AST changes

- [ ] 1.1 Add `Markdown` variant to `DataFormat` DU in `src/FsAgent/AST.fs`
- [ ] 1.2 Update `inferFormat` in `AST.fs` to map `.md` and `.markdown` extensions to `Markdown`

## 2. Writer changes

- [ ] 2.1 Add `Markdown -> "markdown"` case to `formatToLanguageTag` in `src/FsAgent/Writers.fs`

## 3. Tests

- [ ] 3.1 Add C-category tests: `inferFormat ".md"` returns `Markdown`, `inferFormat ".markdown"` returns `Markdown`, `inferFormat ".txt"` still returns `Unknown`
- [ ] 3.2 Add C-category test: `Imported(path, Markdown, false)` renders as raw text (no code block)
- [ ] 3.3 Add C-category test: `Imported(path, Markdown, true)` renders with ` ```markdown ` fenced code block
- [ ] 3.4 Add A-category test: `sectionFrom "Build & Test" "some.md"` in a `prompt { }` CE produces `Section("Build & Test", [Imported("some.md", Markdown, false)])`
- [ ] 3.5 Run `dotnet test` and confirm all tests pass

## 4. DSL builder changes

- [ ] 4.1 Add `sectionFrom` custom operation to `src/FsAgent/Prompt.fs`
- [ ] 4.2 Add `sectionFrom` custom operation to `src/FsAgent/Agent.fs`
- [ ] 4.3 Add `sectionFrom` custom operation to `src/FsAgent/Skill.fs`
- [ ] 4.4 Add `sectionFrom` custom operation to `src/FsAgent/Command.fs`
- [ ] 4.5 Run `dotnet build` and confirm no errors

## 5. Extract AGENTS.fsx content to files

- [ ] 5.1 Create `knowledge/agents/` directory
- [ ] 5.2 Extract `FsAgent` section content to `knowledge/agents/fsagent.md`
- [ ] 5.3 Extract `General` section content to `knowledge/agents/general.md`
- [ ] 5.4 Extract `Tech Stack` section content to `knowledge/agents/tech-stack.md`
- [ ] 5.5 Extract `Build & Test` section content to `knowledge/agents/build-and-test.md`
- [ ] 5.6 Extract `Repository Overview` section content to `knowledge/agents/repository-overview.md`
- [ ] 5.7 Extract `Templating (Fue)` section content to `knowledge/agents/templating.md`
- [ ] 5.8 Extract `Coding Guidelines` section content to `knowledge/agents/coding-guidelines.md`
- [ ] 5.9 Extract `Git Conventions` section content to `knowledge/agents/git-conventions.md`

## 6. Rewrite AGENTS.fsx

- [ ] 6.1 Rewrite `AGENTS.fsx` to use `sectionFrom` for all 8 sections (replacing inline triple-quoted strings)
- [ ] 6.2 Run `dotnet fsi AGENTS.fsx` to regenerate `AGENTS.md` and `CLAUDE.md`
- [ ] 6.3 Verify regenerated output matches previous content exactly

## 7. Changelog

- [ ] 7.1 Add `feat: add Markdown DataFormat variant` entry to `CHANGELOG.md`
- [ ] 7.2 Add `feat: add sectionFrom DSL operation to all builders` entry to `CHANGELOG.md`
