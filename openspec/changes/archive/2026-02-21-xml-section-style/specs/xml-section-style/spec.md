## ADDED Requirements

### Requirement: SectionStyle option on AgentWriter.Options
The system SHALL provide a `SectionStyle` field on `AgentWriter.Options` with a `Markdown` default value, accepting values `Markdown` or `Xml`.

#### Scenario: Default SectionStyle is Markdown
- **WHEN** `AgentWriter.defaultOptions()` is called
- **THEN** `Options.SectionStyle` SHALL equal `Markdown`

#### Scenario: Caller can set SectionStyle to Xml
- **WHEN** caller sets `opts.SectionStyle <- Xml`
- **THEN** subsequent render calls SHALL use XML tag output for Section nodes

### Requirement: Markdown section rendering unchanged by default
The system SHALL render Section nodes as Markdown headings when `SectionStyle` is `Markdown`, preserving existing behaviour.

#### Scenario: Top-level section renders as H1
- **WHEN** agent has a section named "role" AND `SectionStyle` is `Markdown`
- **THEN** output SHALL contain "# role"

#### Scenario: Nested section renders with depth-appropriate heading
- **WHEN** agent has a nested section AND `SectionStyle` is `Markdown`
- **THEN** output SHALL contain the correct `##`/`###` heading for the nesting depth

### Requirement: XML section rendering with SectionStyle.Xml
The system SHALL render Section nodes as `<name>...</name>` XML tags when `SectionStyle` is `Xml`.

#### Scenario: Top-level section renders as XML tag
- **WHEN** agent has a section named "role" AND `SectionStyle` is `Xml`
- **THEN** output SHALL contain `<role>` and `</role>` wrapping the section content

#### Scenario: Nested sections render as nested XML tags
- **WHEN** agent has a section "instructions" containing a sub-section "rules" AND `SectionStyle` is `Xml`
- **THEN** output SHALL contain `<instructions>` enclosing `<rules>...</rules>`

#### Scenario: XML mode does not emit Markdown headings
- **WHEN** `SectionStyle` is `Xml`
- **THEN** output SHALL NOT contain any `#` heading lines for Section nodes

### Requirement: RenameMap applies to XML tag names
The system SHALL apply `Options.RenameMap` substitutions to the tag name when `SectionStyle` is `Xml`.

#### Scenario: Renamed section uses new name as XML tag
- **WHEN** `SectionStyle` is `Xml` AND `RenameMap` maps "role" to "system"
- **THEN** output SHALL contain `<system>` and `</system>` instead of `<role>` and `</role>`

### Requirement: HeadingFormatter applies to XML tag names
The system SHALL apply `Options.HeadingFormatter` to the tag name when `SectionStyle` is `Xml`.

#### Scenario: Formatted section name used as XML tag
- **WHEN** `SectionStyle` is `Xml` AND `HeadingFormatter` transforms names to uppercase
- **THEN** output SHALL contain the transformed name as the XML tag

### Requirement: XML section style applies to both renderMd and renderSkill
The system SHALL respect `SectionStyle` when rendering via both `renderMd` and `renderSkill`.

#### Scenario: renderMd respects Xml style
- **WHEN** rendering an agent with `renderMd` AND `SectionStyle` is `Xml`
- **THEN** Section nodes SHALL render as XML tags

#### Scenario: renderSkill respects Xml style
- **WHEN** rendering a skill with `renderSkill` AND `SectionStyle` is `Xml`
- **THEN** Section nodes SHALL render as XML tags
