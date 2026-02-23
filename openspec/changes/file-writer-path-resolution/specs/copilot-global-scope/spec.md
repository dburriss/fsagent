## ADDED Requirements

### Requirement: Resolve Copilot global scope via COPILOT_GLOBAL_ROOT env var
When no explicit `copilotRoot` is provided, the system SHALL check the `COPILOT_GLOBAL_ROOT` environment variable before raising `NotSupportedException`. If the variable is set to a non-empty string, that value SHALL be returned as the global root.

#### Scenario: COPILOT_GLOBAL_ROOT is set
- **WHEN** `ConfigPaths.resolveGlobalRoot Copilot None` is called and `COPILOT_GLOBAL_ROOT` is set to `"/path/to/github-private"`
- **THEN** the returned path is `"/path/to/github-private"`

#### Scenario: COPILOT_GLOBAL_ROOT not set raises NotSupportedException
- **WHEN** `ConfigPaths.resolveGlobalRoot Copilot None` is called and `COPILOT_GLOBAL_ROOT` is not set
- **THEN** a `NotSupportedException` is raised

### Requirement: Explicit copilotRoot parameter takes priority over env var
When an explicit `copilotRoot` value is supplied, it SHALL be used regardless of the `COPILOT_GLOBAL_ROOT` environment variable.

#### Scenario: Explicit root overrides env var
- **WHEN** `ConfigPaths.resolveGlobalRoot Copilot (Some "/explicit")` is called and `COPILOT_GLOBAL_ROOT` is also set
- **THEN** the returned path is `"/explicit"`

### Requirement: AgentFileWriter copilotRoot constructor parameter takes priority over env var
When `AgentFileWriter` is constructed with `?copilotRoot` set, it SHALL use that value for Copilot global scope resolution, ignoring `COPILOT_GLOBAL_ROOT`.

#### Scenario: AgentFileWriter copilotRoot overrides env var
- **WHEN** `AgentFileWriter` is constructed with `copilotRoot = "/ctor-root"` and Copilot global scope is resolved
- **THEN** the resolved root is `"/ctor-root"`
