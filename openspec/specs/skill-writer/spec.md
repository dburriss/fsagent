## Requirements

### Requirement: renderSkill produces SKILL.md-compatible Markdown
The system SHALL provide `AgentWriter.renderSkill` that accepts a `Skill`, an `AgentHarness`, and an `Options -> unit` configurator, and returns a string containing YAML frontmatter followed by Markdown body sections.

#### Scenario: Render skill with frontmatter fields
- **WHEN** `renderSkill` is called with a skill containing `name`, `description`, `license`, `compatibility`, and `metadata` frontmatter keys
- **THEN** the output begins with `---`, includes each key serialized as YAML, and ends the frontmatter block with `---`

#### Scenario: Render skill with no frontmatter raises validation error
- **WHEN** `renderSkill` is called with `Skill.empty` (empty frontmatter)
- **THEN** a `System.Exception` is raised with a message containing both `'name'` and `'description'`

### Requirement: renderSkill validates required frontmatter fields before serializing
`renderSkill` SHALL validate that `Frontmatter` contains non-empty, non-whitespace values for both `"name"` and `"description"` keys before performing any output serialization. All validation errors SHALL be collected and reported in a single exception.

#### Scenario: missing name key raises validation error
- **WHEN** `renderSkill` is called with a `Skill` whose `Frontmatter` does not contain `"name"`
- **THEN** a `System.Exception` is raised with a message containing `'name'`

#### Scenario: blank name value raises validation error
- **WHEN** `renderSkill` is called with a `Skill` whose `Frontmatter["name"]` is `""`
- **THEN** a `System.Exception` is raised with a message containing `'name'`

#### Scenario: missing description key raises validation error
- **WHEN** `renderSkill` is called with a `Skill` whose `Frontmatter` does not contain `"description"`
- **THEN** a `System.Exception` is raised with a message containing `'description'`

#### Scenario: both name and description missing reports both errors
- **WHEN** `renderSkill` is called with `Skill.empty` (no frontmatter keys)
- **THEN** a single `System.Exception` is raised whose message contains both `'name'` and `'description'`

#### Scenario: valid name and description does not raise
- **WHEN** `renderSkill` is called with non-empty `name` and `description` in frontmatter
- **THEN** no exception is raised and output is returned

#### Scenario: validation is harness-independent
- **WHEN** `renderSkill` is called with missing `name` and any `AgentHarness` value (`Opencode`, `Copilot`, `ClaudeCode`)
- **THEN** a `System.Exception` is raised regardless of harness

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
