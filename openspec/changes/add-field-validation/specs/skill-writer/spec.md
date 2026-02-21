## MODIFIED Requirements

### Requirement: renderSkill produces SKILL.md-compatible Markdown
The system SHALL provide `AgentWriter.renderSkill` that accepts a `Skill`, an `AgentHarness`, and an `Options -> unit` configurator, and returns a string containing YAML frontmatter followed by Markdown body sections.

#### Scenario: Render skill with frontmatter fields
- **WHEN** `renderSkill` is called with a skill containing `name`, `description`, `license`, `compatibility`, and `metadata` frontmatter keys
- **THEN** the output begins with `---`, includes each key serialized as YAML, and ends the frontmatter block with `---`

#### Scenario: Render skill with no frontmatter raises validation error
- **WHEN** `renderSkill` is called with `Skill.empty` (empty frontmatter)
- **THEN** a `System.Exception` is raised with a message containing both `'name'` and `'description'`

## ADDED Requirements

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
