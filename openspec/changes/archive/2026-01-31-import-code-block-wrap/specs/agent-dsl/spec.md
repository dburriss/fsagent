## MODIFIED Requirements

### Requirement: Import operation
The CE SHALL support `import "path"` that creates an import reference with inferred format and `wrapInCodeBlock=true`. The CE SHALL also support `importRaw "path"` that creates an import reference with `wrapInCodeBlock=false`.

#### Scenario: Import creates node with wrapInCodeBlock true
- **WHEN** `import "data.yml"` is used
- **THEN** an `Imported` node is added with path "data.yml", format Yaml, and `wrapInCodeBlock=true`

#### Scenario: ImportRaw creates node with wrapInCodeBlock false
- **WHEN** `importRaw "data.json"` is used
- **THEN** an `Imported` node is added with path "data.json", format Json, and `wrapInCodeBlock=false`

### Requirement: Imported Data References via DSL
The system SHALL allow declaring imported data references within the DSL that become path references in the AST with format inferred from filename and wrapping intent specified by the operation.

#### Scenario: Declare import reference with code-block wrapping
- **WHEN** `import "knowledge/import-data.rules.json"` is used inside the CE
- **THEN** the AST holds an `Imported` node with `sourcePath = "knowledge/import-data.rules.json"`, `DataFormat = Json`, and `wrapInCodeBlock = true`
- **AND** no parsed object is embedded in the AST (writers resolve at write time)

#### Scenario: Declare importRaw reference without code-block wrapping
- **WHEN** `importRaw "knowledge/import-data.rules.json"` is used inside the CE
- **THEN** the AST holds an `Imported` node with `sourcePath = "knowledge/import-data.rules.json"`, `DataFormat = Json`, and `wrapInCodeBlock = false`
- **AND** no parsed object is embedded in the AST (writers resolve at write time)

## MODIFIED Requirements

### Requirement: AST Imported Node
The `Imported` node SHALL include a `wrapInCodeBlock` boolean field to indicate whether the content should be wrapped in a fenced code block when rendered.

#### Scenario: Imported node structure
- **WHEN** an `Imported` node is created
- **THEN** it contains `sourcePath: string`, `format: DataFormat`, and `wrapInCodeBlock: bool`
