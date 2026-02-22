## ADDED Requirements

### Requirement: TOON serializer round-trip
The system SHALL parse a TOON string and re-serialize it to normalized TOON output via `ToonSerializer.serialize`.

#### Scenario: Valid TOON round-trips without error
- **WHEN** `ToonSerializer.serialize` is called with a valid TOON string
- **THEN** it SHALL return `Ok(normalizedToon)` where the result is valid TOON

#### Scenario: Malformed TOON returns Error
- **WHEN** `ToonSerializer.serialize` is called with a syntactically invalid TOON string
- **THEN** it SHALL return `Error(message)` where message describes the parse failure

#### Scenario: No exception propagates from serializer
- **WHEN** `ToonSerializer.serialize` is called with any input string
- **THEN** it SHALL NOT throw an exception; all failures MUST be returned as `Error`

### Requirement: TOON import content resolution
The system SHALL resolve `Imported` node content through `ToonSerializer.serialize` when `DataFormat` is `Toon`.

#### Scenario: Valid TOON import embeds normalized content
- **WHEN** an `Imported` node has `DataFormat.Toon` AND the file contains valid TOON
- **THEN** the embedded content SHALL be the normalized serializer output, not the raw file bytes

#### Scenario: Invalid TOON import embeds error marker
- **WHEN** an `Imported` node has `DataFormat.Toon` AND the file contains malformed TOON
- **THEN** the embedded content SHALL begin with `[TOON parse error: <message>]` followed by the raw content

#### Scenario: YAML import is unaffected
- **WHEN** an `Imported` node has `DataFormat.Yaml`
- **THEN** the embedded content SHALL be the raw file contents unchanged

#### Scenario: JSON import is unaffected
- **WHEN** an `Imported` node has `DataFormat.Json`
- **THEN** the embedded content SHALL be the raw file contents unchanged

#### Scenario: Unknown format import is unaffected
- **WHEN** an `Imported` node has `DataFormat.Unknown`
- **THEN** the embedded content SHALL be the raw file contents unchanged

### Requirement: TOON import code fence tag
The system SHALL wrap TOON imports in a ` ```toon ` fenced code block when `wrapInCodeBlock` is true.

#### Scenario: TOON import with wrapping uses toon fence
- **WHEN** an `Imported` node has `DataFormat.Toon` AND `wrapInCodeBlock` is true AND `DisableCodeBlockWrapping` is false
- **THEN** output SHALL contain ` ```toon ` opening fence and ` ``` ` closing fence around the content

#### Scenario: TOON import without wrapping emits raw content
- **WHEN** an `Imported` node has `DataFormat.Toon` AND `wrapInCodeBlock` is false
- **THEN** output SHALL contain the resolved content without code fences

### Requirement: Consistent import resolution across writers
The system SHALL use a single shared `resolveImportedContent` helper for `Imported` node content resolution in both `renderMd` and `renderSkill`.

#### Scenario: renderMd uses shared resolution
- **WHEN** `renderMd` processes an `Imported` node with `DataFormat.Toon`
- **THEN** it SHALL use `resolveImportedContent` and produce the same result as `renderSkill` for the same input

#### Scenario: renderSkill uses shared resolution
- **WHEN** `renderSkill` processes an `Imported` node with `DataFormat.Toon`
- **THEN** it SHALL use `resolveImportedContent` and produce the same result as `renderMd` for the same input
