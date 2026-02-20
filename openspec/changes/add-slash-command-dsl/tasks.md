## 1. Preparation

- [x] 1.1 Run `dotnet build` and `dotnet test` to confirm baseline passes
- [x] 1.2 Review `src/FsAgent/Prompt.fs` and `src/FsAgent/Writers.fs` to confirm `Prompt.role/objective/instructions/context/output/examples` signatures and `writeMd` internal shape

## 2. Core Type and Builder

- [x] 2.1 Create `src/FsAgent/Command.fs` with `namespace FsAgent.Commands`
- [x] 2.2 Define `SlashCommand` record: `{ Name: string; Description: string; Sections: Node list }`
- [x] 2.3 Add `module SlashCommand` with `let empty` value
- [x] 2.4 Implement `CommandBuilder` CE with `Yield`, `Zero`, `Run`
- [x] 2.5 Add `name` and `description` custom operations
- [x] 2.6 Add section operations: `role`, `objective`, `instructions`, `context`, `output`
- [x] 2.7 Add `section` operation (name + content string)
- [x] 2.8 Add `import` and `importRaw` operations (via `AST.importRef` / `AST.importRawRef`)
- [x] 2.9 Add `template` and `templateFile` operations
- [x] 2.10 Add `examples` operation (takes `Node list`)
- [x] 2.11 Add `prompt` operation (merges `Prompt.Sections` into command)
- [x] 2.12 Expose `let command = CommandBuilder()` in `[<AutoOpen>] module CommandBuilder`

## 3. Project File

- [x] 3.1 Add `<Compile Include="Command.fs" />` to `src/FsAgent/FsAgent.fsproj` after `Agent.fs` and before `Writers.fs`

## 4. Writer Extension

- [x] 4.1 Add `writeCommand (cmd: SlashCommand) (configure: Options -> unit) : string` to `MarkdownWriter` module in `src/FsAgent/Writers.fs`
- [x] 4.2 Construct synthetic `Agent` with `Frontmatter = Map.ofList ["description", AST.fmStr cmd.Description]` and `Sections = cmd.Sections`
- [x] 4.3 Construct `WriterContext` with `AgentName = Some cmd.Name`, `AgentDescription = Some cmd.Description`
- [x] 4.4 Dispatch to `writeMd` / `writeJson` / `writeYaml` based on `opts.OutputType`

## 5. Library Re-exports

- [x] 5.1 Add `type SlashCommand = Commands.SlashCommand` to `src/FsAgent/Library.fs` (alongside existing type aliases)
- [x] 5.2 Add `let command = Commands.CommandBuilder.command` inside `module DSL` in `Library.fs`

## 6. Tests

- [x] 6.1 Create `tests/FsAgent.Tests/CommandTests.fs` with `module FsAgent.Tests.CommandTests`
- [x] 6.2 Add compile entry `<Compile Include="CommandTests.fs" />` to `tests/FsAgent.Tests/FsAgent.Tests.fsproj`
- [x] 6.3 Write test: `command with description renders frontmatter` — `description` key present in output
- [x] 6.4 Write test: `command name is not in frontmatter` — `name:` absent from YAML block
- [x] 6.5 Write test: `command sections render correctly` — `role`, `instructions`, `section` appear as `#` headings
- [x] 6.6 Write test: `command template renders with harness` — `{{{tool Bash}}}` resolves for Opencode
- [x] 6.7 Write test: `command import embeds file` — `import` node produces expected output
- [x] 6.8 Write test: `command renders identically for Copilot and ClaudeCode` — output matches Opencode
- [x] 6.9 Write test: `empty command renders only description frontmatter` — minimal valid output

## 7. Changelog and Verification

- [x] 7.1 Add entry under `[Unreleased]` in `CHANGELOG.md`: `feat: add SlashCommand DSL type, command CE builder, and MarkdownWriter.writeCommand`
- [x] 7.2 Run `dotnet build` — confirm zero errors
- [x] 7.3 Run `dotnet test` — confirm all tests pass including new CommandTests
