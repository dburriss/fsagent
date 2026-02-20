## ADDED Requirements

### Requirement: renderSkill produces SKILL.md-compatible Markdown
The system SHALL provide `AgentWriter.renderSkill` that accepts a `Skill`, an `AgentHarness`, and an `Options -> unit` configurator, and returns a string containing YAML frontmatter followed by Markdown body sections.

#### Scenario: Render skill with frontmatter fields
- **WHEN** `renderSkill` is called with a skill containing `name`, `description`, `license`, `compatibility`, and `metadata` frontmatter keys
- **THEN** the output begins with `---`, includes each key serialized as YAML, and ends the frontmatter block with `---`

#### Scenario: Render skill with no frontmatter
- **WHEN** `renderSkill` is called with `Skill.empty`
- **THEN** no `---` frontmatter block is emitted and the output is an empty string or contains only body content

### Requirement: Frontmatter key serialization
`renderSkill` SHALL serialize frontmatter values according to their type:
- `string` → plain scalar (no quotes unless required by YAML)
- `float` → numeric scalar
- `bool` → `true` or `false`
- `obj list` → YAML block sequence (`- item` per line)
- `Map<string, obj>` → YAML block mapping (indented `key: value` per entry)

#### Scenario: Serialize metadata as nested mapping
- **WHEN** `Frontmatter` contains `"metadata" → Map ["author","alice"; "version","1.0"]`
- **THEN** the rendered frontmatter includes `metadata:` followed by indented `author: alice` and `version: 1.0` lines

#### Scenario: Serialize list value as block sequence
- **WHEN** `Frontmatter` contains `"tags" → ["a"; "b"]` (as `obj list`)
- **THEN** the rendered frontmatter includes `tags:` followed by `  - a` and `  - b`

### Requirement: Body section rendering
`renderSkill` SHALL render `Sections` using the same Markdown node rendering as `renderAgent`, with heading levels starting at `#` (level 1).

#### Scenario: Render named section as heading
- **WHEN** `Sections` contains `Section("Steps", [Text "Do this."])`
- **THEN** the output contains `# Steps` followed by `Do this.`

#### Scenario: Render template node with harness
- **WHEN** `Sections` contains `Template "{{{tool Bash}}}"` and harness is `Opencode`
- **THEN** the rendered output contains `bash`

#### Scenario: Render template node with Copilot harness
- **WHEN** `Sections` contains `Template "{{{tool Bash}}}"` and harness is `Copilot`
- **THEN** the rendered output contains `bash`

### Requirement: AgentHarness controls tool-name resolution
`renderSkill` SHALL accept an `AgentHarness` value and use it when resolving `{{{tool X}}}` references inside `Template` and `TemplateFile` nodes.

#### Scenario: ClaudeCode harness resolves Bash to capitalized form
- **WHEN** `Sections` contains `Template "{{{tool Bash}}}"` and harness is `ClaudeCode`
- **THEN** the rendered output contains `Bash`

### Requirement: Options configurator is respected
`renderSkill` SHALL apply the same `Options` fields used by `renderAgent` where applicable: `TemplateVariables`, `HeadingFormatter`, `RenameMap`, `GeneratedFooter`, and `DisableCodeBlockWrapping`.

#### Scenario: Apply heading formatter
- **WHEN** `Options.HeadingFormatter` is set to `String.toUpper` and a section named `"overview"` is present
- **THEN** the rendered heading is `# OVERVIEW`

#### Scenario: Inject template variables
- **WHEN** `Sections` contains `Template "Hello {{{name}}}"` and `TemplateVariables` contains `"name" → "world"`
- **THEN** the rendered output contains `Hello world`

### Requirement: No breaking changes to existing render functions
Adding `renderSkill` SHALL NOT alter the signatures or behaviour of `renderAgent`, `renderPrompt`, or `renderCommand`.

#### Scenario: Existing renderAgent output is unchanged
- **WHEN** `renderAgent` is called with any previously valid input after adding `renderSkill`
- **THEN** the output is identical to the output produced before the change
