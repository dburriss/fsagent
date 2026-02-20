## 1. Preparation

- [ ] 1.1 Run `dotnet build` and `dotnet test` to confirm baseline passes
- [ ] 1.2 Review `src/FsAgent/Prompt.fs` and `src/FsAgent/Writers.fs` to confirm `Prompt.role/objective/instructions/context/output/examples` signatures and `writeMd` internal shape

## 2. Core Type and Builder

- [ ] 2.1 Create `src/FsAgent/Command.fs` with `namespace FsAgent.Commands`
- [ ] 2.2 Define `SlashCommand` record: `{ Name: string; Description: string; Sections: Node list }`
- [ ] 2.3 Add `module SlashCommand` with `let empty` value
- [ ] 2.4 Implement `CommandBuilder` CE with `Yield`, `Zero`, `Run`
- [ ] 2.5 Add `name` and `description` custom operations
- [ ] 2.6 Add section operations: `role`, `objective`, `instructions`, `context`, `output`
- [ ] 2.7 Add `section` operation (name + content string)
- [ ] 2.8 Add `import` and `importRaw` operations (via `AST.importRef` / `AST.importRawRef`)
- [ ] 2.9 Add `template` and `templateFile` operations
- [ ] 2.10 Add `examples` operation (takes `Node list`)
- [ ] 2.11 Add `prompt` operation (merges `Prompt.Sections` into command)
- [ ] 2.12 Expose `let command = CommandBuilder()` in `[<AutoOpen>] module CommandBuilder`

## 3. Project File

- [ ] 3.1 Add `<Compile Include="Command.fs" />` to `src/FsAgent/FsAgent.fsproj` after `Agent.fs` and before `Writers.fs`

## 4. Writer Extension

- [ ] 4.1 Add `writeCommand (cmd: SlashCommand) (configure: Options -> unit) : string` to `MarkdownWriter` module in `src/FsAgent/Writers.fs`
- [ ] 4.2 Construct synthetic `Agent` with `Frontmatter = Map.ofList ["description", AST.fmStr cmd.Description]` and `Sections = cmd.Sections`
- [ ] 4.3 Construct `WriterContext` with `AgentName = Some cmd.Name`, `AgentDescription = Some cmd.Description`
- [ ] 4.4 Dispatch to `writeMd` / `writeJson` / `writeYaml` based on `opts.OutputType`

## 5. Library Re-exports

- [ ] 5.1 Add `type SlashCommand = Commands.SlashCommand` to `src/FsAgent/Library.fs` (alongside existing type aliases)
- [ ] 5.2 Add `let command = Commands.CommandBuilder.command` inside `module DSL` in `Library.fs`

## 6. Tests

- [ ] 6.1 Create `tests/FsAgent.Tests/CommandTests.fs` with `module FsAgent.Tests.CommandTests`
- [ ] 6.2 Add compile entry `<Compile Include="CommandTests.fs" />` to `tests/FsAgent.Tests/FsAgent.Tests.fsproj`
- [ ] 6.3 Write test: `command with description renders frontmatter` — `description` key present in output
- [ ] 6.4 Write test: `command name is not in frontmatter` — `name:` absent from YAML block
- [ ] 6.5 Write test: `command sections render correctly` — `role`, `instructions`, `section` appear as `#` headings
- [ ] 6.6 Write test: `command template renders with harness` — `{{{tool Bash}}}` resolves for Opencode
- [ ] 6.7 Write test: `command import embeds file` — `import` node produces expected output
- [ ] 6.8 Write test: `command renders identically for Copilot and ClaudeCode` — output matches Opencode
- [ ] 6.9 Write test: `empty command renders only description frontmatter` — minimal valid output

## 7. Changelog and Verification

- [ ] 7.1 Add entry under `[Unreleased]` in `CHANGELOG.md`: `feat: add SlashCommand DSL type, command CE builder, and MarkdownWriter.writeCommand`
- [ ] 7.2 Run `dotnet build` — confirm zero errors
- [ ] 7.3 Run `dotnet test` — confirm all tests pass including new CommandTests
