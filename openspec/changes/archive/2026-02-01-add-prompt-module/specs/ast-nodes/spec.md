## ADDED Requirements

### Requirement: Template node case
The system SHALL add Template case to Node discriminated union containing inline template text.

#### Scenario: Template node structure
- **WHEN** Template("Hello {{{name}}}") is created
- **THEN** node SHALL contain text field with value "Hello {{{name}}}"

### Requirement: TemplateFile node case
The system SHALL add TemplateFile case to Node discriminated union containing file path.

#### Scenario: TemplateFile node structure
- **WHEN** TemplateFile("templates/greeting.txt") is created
- **THEN** node SHALL contain path field with value "templates/greeting.txt"

## REMOVED Requirements

### Requirement: AST Construction API (core sections and examples)
**Reason**: Prompt-specific constructors (role, objective, instructions, context, output, example, examples) are being moved to Prompt module in FsAgent.Prompts namespace.
**Migration**: Use Prompt.role, Prompt.objective, Prompt.instructions, Prompt.context, Prompt.output, Prompt.example, Prompt.examples from FsAgent.Prompts namespace.

## MODIFIED Requirements

### Requirement: Frontmatter Helper Constructors
The system SHALL provide helper constructors for frontmatter values to support DSL ergonomics in the AST module of FsAgent.AST namespace.

#### Scenario: Construct string value
- **WHEN** `AST.fmStr "value"` is used
- **THEN** it returns `"value" :> obj`

#### Scenario: Construct numeric value
- **WHEN** `AST.fmNum 42.0` is used
- **THEN** it returns `42.0 :> obj`

#### Scenario: Construct boolean value
- **WHEN** `AST.fmBool true` is used
- **THEN** it returns `true :> obj`

#### Scenario: Construct list value
- **WHEN** `AST.fmList [box "a"; box "b"]` is used
- **THEN** it returns `["a"; "b"] :> obj`

#### Scenario: Construct map value
- **WHEN** `AST.fmMap (Map.ofList [("k", "v" :> obj)])` is used
- **THEN** it returns `Map(...) :> obj`

### Requirement: Import Reference with Format Inference
The system SHALL provide `importRef` and `importRawRef` in AST module that infer format from filename extension.

#### Scenario: Infer YAML format
- **WHEN** `AST.importRef "data.yml"` is used
- **THEN** it returns Imported("data.yml", Yaml, true)

#### Scenario: Infer JSON format
- **WHEN** `AST.importRef "data.json"` is used
- **THEN** it returns Imported("data.json", Json, true)

#### Scenario: Import raw without code block
- **WHEN** `AST.importRawRef "data.json"` is used
- **THEN** it returns Imported("data.json", Json, false)
