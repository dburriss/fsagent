## 1. Project Structure

- [ ] 1.1 Add `Skill.fs` to `src/FsAgent/FsAgent.fsproj` after `Command.fs` and before `Writers.fs`
- [ ] 1.2 Add `SkillTests.fs` to `tests/FsAgent.Tests/FsAgent.Tests.fsproj`

## 2. Skill Domain Layer (`Skill.fs`)

- [ ] 2.1 Define `Skill` record in `FsAgent.Skills` namespace with `Frontmatter: Map<string, obj>` and `Sections: Node list`
- [ ] 2.2 Define `Skill.empty` with empty frontmatter map and empty sections list
- [ ] 2.3 Implement `SkillBuilder` CE with `Yield`, `Zero`, and `Run` members
- [ ] 2.4 Add `name`, `description`, `license`, `compatibility` custom operations (string → frontmatter key)
- [ ] 2.5 Add `metadata` custom operation (`Map<string, obj>` → frontmatter key `"metadata"`)
- [ ] 2.6 Add `section` custom operation (name + content string → `Section(name, [Text content])`)
- [ ] 2.7 Add `prompt` custom operation (appends `Prompt.Sections` to `Skill.Sections`)
- [ ] 2.8 Add `import` and `importRaw` custom operations (path → `AST.importRef` / `AST.importRawRef`)
- [ ] 2.9 Add `template` and `templateFile` custom operations (→ `Template` / `TemplateFile` nodes)
- [ ] 2.10 Expose `let skill = SkillBuilder()` in an `[<AutoOpen>]` module

## 3. Writer Layer (`Writers.fs`)

- [ ] 3.1 Open `FsAgent.Skills` in `Writers.fs`
- [ ] 3.2 Implement `AgentWriter.renderSkill` accepting `Skill`, `AgentHarness`, and `Options -> unit`
- [ ] 3.3 Serialize `Skill.Frontmatter` to YAML frontmatter block (reuse/extract scalar serialization helper from `renderMd`)
- [ ] 3.4 Serialize `Map<string, obj>` metadata value as indented YAML block mapping
- [ ] 3.5 Render `Skill.Sections` using the existing `writeNode` logic with harness-aware template resolution
- [ ] 3.6 Apply `Options` fields: `TemplateVariables`, `HeadingFormatter`, `RenameMap`, `GeneratedFooter`, `DisableCodeBlockWrapping`

## 4. Library Exposure (`Library.fs`)

- [ ] 4.1 Add `type Skill = FsAgent.Skills.Skill` type alias
- [ ] 4.2 Add `let skill = FsAgent.Skills.SkillBuilder.skill` to `DSL` module

## 5. Tests (`SkillTests.fs`)

- [ ] 5.1 Test `Skill.empty` has empty frontmatter and sections (A)
- [ ] 5.2 Test `skill { () }` equals `Skill.empty` (A)
- [ ] 5.3 Test each frontmatter CE operation writes correct key/value (A)
- [ ] 5.4 Test `section` appends correct `Section` node (A)
- [ ] 5.5 Test `prompt` appends prompt sections to skill sections (A)
- [ ] 5.6 Test `template` and `templateFile` append correct nodes (A)
- [ ] 5.7 Test `renderSkill` produces `---` frontmatter block with all keys (A)
- [ ] 5.8 Test `renderSkill` serializes `metadata` map as nested YAML mapping (A)
- [ ] 5.9 Test `renderSkill` renders `Section` node as `# Heading` (A)
- [ ] 5.10 Test `renderSkill` resolves `{{{tool Bash}}}` correctly per harness (`Opencode`, `ClaudeCode`) (A)
- [ ] 5.11 Test `renderSkill` with empty skill produces no frontmatter block (A)
- [ ] 5.12 Test `renderSkill` respects `HeadingFormatter` option (A)
- [ ] 5.13 Test `renderSkill` respects `TemplateVariables` option (A)
- [ ] 5.14 Test `renderAgent` output is unchanged after adding `renderSkill` (A)

## 6. Verification

- [ ] 6.1 Run `dotnet build` — zero errors
- [ ] 6.2 Run `dotnet test` — all tests pass
- [ ] 6.3 Update `CHANGELOG.md` with the new `skill-builder` and `skill-writer` additions
