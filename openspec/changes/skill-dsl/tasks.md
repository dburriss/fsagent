## 1. Project Structure

- [x] 1.1 Add `Skill.fs` to `src/FsAgent/FsAgent.fsproj` after `Command.fs` and before `Writers.fs`
- [x] 1.2 Add `SkillTests.fs` to `tests/FsAgent.Tests/FsAgent.Tests.fsproj`

## 2. Skill Domain Layer (`Skill.fs`)

- [x] 2.1 Define `Skill` record in `FsAgent.Skills` namespace with `Frontmatter: Map<string, obj>` and `Sections: Node list`
- [x] 2.2 Define `Skill.empty` with empty frontmatter map and empty sections list
- [x] 2.3 Implement `SkillBuilder` CE with `Yield`, `Zero`, and `Run` members
- [x] 2.4 Add `name`, `description`, `license`, `compatibility` custom operations (string → frontmatter key)
- [x] 2.5 Add `metadata` custom operation (`Map<string, obj>` → frontmatter key `"metadata"`)
- [x] 2.6 Add `section` custom operation (name + content string → `Section(name, [Text content])`)
- [x] 2.7 Add `prompt` custom operation (appends `Prompt.Sections` to `Skill.Sections`)
- [x] 2.8 Add `import` and `importRaw` custom operations (path → `AST.importRef` / `AST.importRawRef`)
- [x] 2.9 Add `template` and `templateFile` custom operations (→ `Template` / `TemplateFile` nodes)
- [x] 2.10 Expose `let skill = SkillBuilder()` in an `[<AutoOpen>]` module

## 3. Writer Layer (`Writers.fs`)

- [x] 3.1 Open `FsAgent.Skills` in `Writers.fs`
- [x] 3.2 Implement `AgentWriter.renderSkill` accepting `Skill`, `AgentHarness`, and `Options -> unit`
- [x] 3.3 Serialize `Skill.Frontmatter` to YAML frontmatter block (reuse/extract scalar serialization helper from `renderMd`)
- [x] 3.4 Serialize `Map<string, obj>` metadata value as indented YAML block mapping
- [x] 3.5 Render `Skill.Sections` using the existing `writeNode` logic with harness-aware template resolution
- [x] 3.6 Apply `Options` fields: `TemplateVariables`, `HeadingFormatter`, `RenameMap`, `GeneratedFooter`, `DisableCodeBlockWrapping`

## 4. Library Exposure (`Library.fs`)

- [x] 4.1 Add `type Skill = FsAgent.Skills.Skill` type alias
- [x] 4.2 Add `let skill = FsAgent.Skills.SkillBuilder.skill` to `DSL` module

## 5. Tests (`SkillTests.fs`)

- [x] 5.1 Test `Skill.empty` has empty frontmatter and sections (A)
- [x] 5.2 Test `skill { () }` equals `Skill.empty` (A)
- [x] 5.3 Test each frontmatter CE operation writes correct key/value (A)
- [x] 5.4 Test `section` appends correct `Section` node (A)
- [x] 5.5 Test `prompt` appends prompt sections to skill sections (A)
- [x] 5.6 Test `template` and `templateFile` append correct nodes (A)
- [x] 5.7 Test `renderSkill` produces `---` frontmatter block with all keys (A)
- [x] 5.8 Test `renderSkill` serializes `metadata` map as nested YAML mapping (A)
- [x] 5.9 Test `renderSkill` renders `Section` node as `# Heading` (A)
- [x] 5.10 Test `renderSkill` resolves `{{{tool Bash}}}` correctly per harness (`Opencode`, `ClaudeCode`) (A)
- [x] 5.11 Test `renderSkill` with empty skill produces no frontmatter block (A)
- [x] 5.12 Test `renderSkill` respects `HeadingFormatter` option (A)
- [x] 5.13 Test `renderSkill` respects `TemplateVariables` option (A)
- [x] 5.14 Test `renderAgent` output is unchanged after adding `renderSkill` (A)

## 6. Verification

- [x] 6.1 Run `dotnet build` — zero errors
- [x] 6.2 Run `dotnet test` — all tests pass
- [x] 6.3 Update `CHANGELOG.md` with the new `skill-builder` and `skill-writer` additions
