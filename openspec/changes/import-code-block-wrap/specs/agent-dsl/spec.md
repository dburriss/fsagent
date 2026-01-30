## MODIFIED Requirements

### Requirement: Import operation
The CE SHALL support `import "path"` that creates an import reference with inferred format, intended for code-block-wrapped embedding by the writer. The CE SHALL also support `importRaw "path"` for raw embedding without wrapping.

#### Scenario: Import with extension (code-block intent)
- **WHEN** `import "data.yml"` is used
- **THEN** an import reference with path "data.yml" and format Yaml is added
- **AND** the operation name signals intent for code-block-wrapped embedding

#### Scenario: ImportRaw with extension (raw intent)
- **WHEN** `importRaw "data.json"` is used
- **THEN** an import reference with path "data.json" and format Json is added
- **AND** the operation name signals intent for raw embedding without wrapping

### Requirement: Imported Data References via DSL
The system SHALL allow declaring imported data references within the DSL that become path references in the AST with format inferred from filename. The DSL SHALL provide both `import` (code-block intent) and `importRaw` (raw intent) operations.

#### Scenario: Declare import reference with inferred format (code-block intent)
- **WHEN** `import "knowledge/import-data.rules.json"` is used inside the CE
- **THEN** the AST holds a reference node with `sourcePath = "knowledge/import-data.rules.json"` and `DataFormat = json` inferred from the `.json` extension
- **AND** no parsed object is embedded in the AST (writers resolve at write time)

#### Scenario: Declare importRaw reference with inferred format (raw intent)
- **WHEN** `importRaw "knowledge/import-data.rules.json"` is used inside the CE
- **THEN** the AST holds a reference node with `sourcePath = "knowledge/import-data.rules.json"` and `DataFormat = json` inferred from the `.json` extension
- **AND** no parsed object is embedded in the AST (writers resolve at write time)
