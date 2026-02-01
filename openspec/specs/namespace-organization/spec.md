## ADDED Requirements

### Requirement: FsAgent.AST namespace
The system SHALL organize AST types (DataFormat, Node) and AST module in FsAgent.AST namespace.

#### Scenario: Access AST types
- **WHEN** user opens FsAgent.AST
- **THEN** system SHALL make DataFormat, Node types and AST module available

### Requirement: FsAgent.Prompts namespace
The system SHALL organize Prompt type and Prompt module in FsAgent.Prompts namespace.

#### Scenario: Access Prompt types
- **WHEN** user opens FsAgent.Prompts
- **THEN** system SHALL make Prompt type and Prompt module available

### Requirement: FsAgent.Agents namespace
The system SHALL organize Agent type and Agent module in FsAgent.Agents namespace.

#### Scenario: Access Agent types
- **WHEN** user opens FsAgent.Agents
- **THEN** system SHALL make Agent type and Agent module available

### Requirement: FsAgent.Writers namespace
The system SHALL organize MarkdownWriter and Template modules in FsAgent.Writers namespace.

#### Scenario: Access Writer modules
- **WHEN** user opens FsAgent.Writers
- **THEN** system SHALL make MarkdownWriter and Template modules available

### Requirement: AutoOpen for PromptBuilder
The system SHALL mark PromptBuilder module with AutoOpen attribute in FsAgent.Prompts namespace.

#### Scenario: Builder available without explicit import
- **WHEN** user opens FsAgent.Prompts
- **THEN** system SHALL make prompt builder directly available without opening PromptBuilder module

### Requirement: AutoOpen for AgentBuilder
The system SHALL mark AgentBuilder module with AutoOpen attribute in FsAgent.Agents namespace.

#### Scenario: Agent builders available without explicit import
- **WHEN** user opens FsAgent.Agents
- **THEN** system SHALL make agent and meta builders directly available without opening AgentBuilder module

### Requirement: AST.fs file
The system SHALL create AST.fs file containing DataFormat, Node types and AST module.

#### Scenario: AST.fs contains shared types
- **WHEN** project is compiled
- **THEN** AST.fs SHALL define DataFormat enum, Node DU, and AST module with utility functions

### Requirement: Prompt.fs file
The system SHALL create Prompt.fs file containing Prompt type, Prompt module, and PromptBuilder.

#### Scenario: Prompt.fs contains prompt definitions
- **WHEN** project is compiled
- **THEN** Prompt.fs SHALL define Prompt type, Prompt module with constructors, and PromptBuilder CE

### Requirement: Agent.fs file
The system SHALL create Agent.fs file containing Agent type, Agent module, MetaBuilder, and AgentBuilder.

#### Scenario: Agent.fs contains agent definitions
- **WHEN** project is compiled
- **THEN** Agent.fs SHALL define Agent type, Agent module, MetaBuilder, and AgentBuilder CE

### Requirement: Writers.fs file
The system SHALL create Writers.fs file containing Template module and MarkdownWriter module.

#### Scenario: Writers.fs contains serialization logic
- **WHEN** project is compiled
- **THEN** Writers.fs SHALL define Template module for rendering and MarkdownWriter module for serialization

### Requirement: Library.fs backward compatibility
The system SHALL maintain Library.fs as re-export layer for backward compatibility.

#### Scenario: Re-export types in FsAgent namespace
- **WHEN** user opens FsAgent
- **THEN** system SHALL provide type aliases: DataFormat, Node, Agent, Prompt

#### Scenario: Re-export DSL module
- **WHEN** user opens FsAgent.DSL
- **THEN** system SHALL provide meta, agent, prompt builders as re-exports

### Requirement: Compilation order
The system SHALL compile files in dependency order: AST.fs, Prompt.fs, Agent.fs, Writers.fs, Library.fs.

#### Scenario: FsAgent.fsproj compilation order
- **WHEN** project is built
- **THEN** system SHALL compile AST.fs before Prompt.fs, Prompt.fs before Agent.fs, Agent.fs before Writers.fs, Writers.fs before Library.fs

### Requirement: Module access patterns
The system SHALL support three access patterns: namespace import (builder auto-available), module access (Prompt.role), and module import (open Prompt module).

#### Scenario: Namespace import pattern
- **WHEN** user writes `open FsAgent.Prompts` and `prompt { ... }`
- **THEN** system SHALL resolve prompt builder via AutoOpen

#### Scenario: Module access pattern
- **WHEN** user writes `open FsAgent.Prompts` and `Prompt.role "text"`
- **THEN** system SHALL resolve role function from Prompt module

#### Scenario: Module import pattern
- **WHEN** user writes `open FsAgent.Prompts.Prompt` and `role "text"`
- **THEN** system SHALL resolve role function directly without module prefix
