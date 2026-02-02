## 1. Core Types (Foundation)

- [x] 1.1 Add Tool discriminated union to AST.fs with cases: Write, Edit, Bash, WebFetch, Todo, Custom of string
- [x] 1.2 Add Tool type re-export to Library.fs for public API access
- [x] 1.3 Add AgentHarness type to Writers.fs with cases: Opencode, Copilot, ClaudeCode
- [x] 1.4 Verify project builds successfully with new types added

## 2. Writer Harness-Specific Mapping

- [x] 2.1 Add toolToString function to Writers.fs with signature: AgentHarness -> Tool -> string
- [x] 2.2 Implement Opencode tool name mappings (lowercase: write, edit, bash, webfetch, todo)
- [x] 2.3 Research and implement Copilot tool name mappings from documentation
- [x] 2.4 Research and implement ClaudeCode tool name mappings from documentation (may be capitalized)
- [x] 2.5 Implement Custom tool passthrough (string unchanged for all harnesses)

## 3. Agent Builder Operations

- [x] 3.1 Update tools operation signature from obj list to Tool list in Agent.fs
- [x] 3.2 Modify tools operation to store Tool list directly in frontmatter["tools"] without string conversion
- [x] 3.3 Update disallowedTools operation signature from string list to Tool list in Agent.fs
- [x] 3.4 Modify disallowedTools to store Tool list in separate frontmatter["disallowedTools"] field
- [x] 3.5 Delete toolMap custom operation entirely from AgentBuilder in Agent.fs

## 4. Writer Frontmatter Formatting

- [x] 4.1 Update formatToolsFrontmatter to extract Tool list from frontmatter["tools"]
- [x] 4.2 Update formatToolsFrontmatter to extract Tool list from frontmatter["disallowedTools"]
- [x] 4.3 Implement Tool-to-string conversion using toolToString with current AgentHarness
- [x] 4.4 Implement merge logic where enabled tools map is created, disabled map is created, then disabled overrides enabled
- [x] 4.5 Handle type extraction robustly (support both Tool list and list<obj> patterns)

## 5. Rename AgentFormat to AgentHarness

- [x] 5.1 Rename AgentFormat type definition to AgentHarness in Writers.fs
- [x] 5.2 Update WriterContext Format field type from AgentFormat to AgentHarness
- [x] 5.3 Update Options OutputFormat field type from AgentFormat to AgentHarness
- [x] 5.4 Update all pattern matches on AgentFormat to AgentHarness throughout Writers.fs
- [x] 5.5 Add ClaudeCode cases to all pattern matches that handle harness types
- [x] 5.6 Update defaultOptions to use Opencode harness

## 6. Test Updates - AgentHarness

- [x] 6.1 Update test references from MarkdownWriter.AgentFormat to MarkdownWriter.AgentHarness
- [x] 6.2 Update test configurations using Opencode and Copilot harness values
- [x] 6.3 Add test cases for ClaudeCode harness
- [x] 6.4 Verify Copilot validation tests still work with AgentHarness

## 7. Test Updates - Typed Tools

- [x] 7.1 Update "tools DSL operation creates list format" test to use Tool list syntax [Write; Bash; Custom "grep"]
- [x] 7.2 Remove "toolMap DSL operation creates map format" test entirely
- [x] 7.3 Update "disallowedTools alone creates map" test to use Tool list syntax [Bash]
- [x] 7.4 Update "disallowedTools and tools operations can be combined" test to use Tool lists
- [x] 7.5 Update "disallowedTools and toolMap merged" test to remove toolMap usage, use only tools + disallowedTools
- [x] 7.6 Update "disallowedTools can override previously allowed tools" test to use Tool syntax
- [x] 7.7 Update "disallowedTools with ToolsList output format" test to use Tool syntax

## 8. Test Coverage - Harness-Specific Mapping

- [x] 8.1 Add test for Opencode tool name mapping (verify lowercase output)
- [x] 8.2 Add test for Copilot tool name mapping (verify correct names per documentation)
- [x] 8.3 Add test for ClaudeCode tool name mapping (verify capitalized or correct names)
- [x] 8.4 Add test for Custom tool passthrough across all harnesses
- [x] 8.5 Add test verifying same Tool list produces different strings for different harnesses

## 9. Documentation Updates

- [x] 9.1 Update CLAUDE.md references from AgentFormat to AgentHarness
- [x] 9.2 Document Tool discriminated union in CLAUDE.md with usage examples
- [x] 9.3 Create migration guide showing before/after examples for tools, disallowedTools, toolMap removal
- [x] 9.4 Document breaking changes in changelog with all signature changes and removals
- [x] 9.5 Add XML documentation comments to Tool type cases explaining when to use each

## 10. Verification and Validation

- [x] 10.1 Run dotnet build and verify zero compilation errors
- [x] 10.2 Run dotnet test and verify all tests pass
- [x] 10.3 Create manual smoke test agent with tools [Write; Bash; Custom "mcp_special"] and disallowedTools [Edit]
- [x] 10.4 Generate Opencode output and verify tool names are lowercase and Edit is disabled
- [x] 10.5 Generate Copilot output and verify tool names match Copilot specification
- [x] 10.6 Generate ClaudeCode output and verify tool names match Claude Code specification
- [x] 10.7 Verify frontmatter storage contains Tool lists, not string maps, by inspecting agent.Frontmatter
- [x] 10.8 Verify toolMap operation no longer exists and produces compilation error when attempted
