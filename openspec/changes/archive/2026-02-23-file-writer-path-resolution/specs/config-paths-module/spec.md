## ADDED Requirements

### Requirement: ConfigPaths.resolveProjectRoot returns harness-specific project root
The system SHALL provide `ConfigPaths.resolveProjectRoot (harness: AgentHarness) (rootDir: string) : string` returning the harness-specific subdirectory within a project root. The `rootDir` SHALL be resolved to an absolute path.

#### Scenario: Opencode project root
- **WHEN** `ConfigPaths.resolveProjectRoot Opencode "/repo"` is called
- **THEN** the returned path is `"/repo/.opencode"`

#### Scenario: Copilot project root
- **WHEN** `ConfigPaths.resolveProjectRoot Copilot "/repo"` is called
- **THEN** the returned path is `"/repo/.github"`

#### Scenario: ClaudeCode project root
- **WHEN** `ConfigPaths.resolveProjectRoot ClaudeCode "/repo"` is called
- **THEN** the returned path is `"/repo/.claude"`

### Requirement: ConfigPaths.resolveGlobalRoot returns harness-specific global root
The system SHALL provide `ConfigPaths.resolveGlobalRoot (harness: AgentHarness) (copilotRoot: string option) : string` returning the harness-specific global config root. For Copilot, resolution follows the priority: explicit `copilotRoot` → `COPILOT_GLOBAL_ROOT` env var → `NotSupportedException`.

#### Scenario: Opencode global root on Unix
- **WHEN** `ConfigPaths.resolveGlobalRoot Opencode None` is called on Linux or macOS
- **THEN** the returned path ends with `.config/opencode`

#### Scenario: ClaudeCode global root
- **WHEN** `ConfigPaths.resolveGlobalRoot ClaudeCode None` is called
- **THEN** the returned path ends with `.claude`

#### Scenario: Copilot global root with explicit override
- **WHEN** `ConfigPaths.resolveGlobalRoot Copilot (Some "/path/to/private")` is called
- **THEN** the returned path is `"/path/to/private"`

### Requirement: ConfigPaths is accessible via open FsAgent
The `ConfigPaths` module SHALL be re-exported from `Library.fs` so consumers using `open FsAgent` can access it without opening `FsAgent.Writers.FileWriter`.

#### Scenario: ConfigPaths available after open FsAgent
- **WHEN** a consumer writes `open FsAgent` and calls `ConfigPaths.resolveProjectRoot Opencode "/r"`
- **THEN** the call compiles and returns `"/r/.opencode"`
