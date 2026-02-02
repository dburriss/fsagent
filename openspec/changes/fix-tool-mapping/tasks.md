## 1. Extend Tool Type with New Variants

- [ ] 1.1 Add Read tool variant to Tool discriminated union in AST.fs
- [ ] 1.2 Add Glob tool variant to Tool discriminated union in AST.fs
- [ ] 1.3 Add List tool variant to Tool discriminated union in AST.fs
- [ ] 1.4 Add LSP tool variant to Tool discriminated union in AST.fs
- [ ] 1.5 Add Skill tool variant to Tool discriminated union in AST.fs
- [ ] 1.6 Add TodoWrite tool variant to Tool discriminated union in AST.fs
- [ ] 1.7 Add TodoRead tool variant to Tool discriminated union in AST.fs
- [ ] 1.8 Add WebSearch tool variant to Tool discriminated union in AST.fs
- [ ] 1.9 Add Question tool variant to Tool discriminated union in AST.fs
- [ ] 1.10 Add Shell tool variant to Tool discriminated union in AST.fs
- [ ] 1.11 Add XML documentation to each new Tool variant describing its purpose and mapping

## 2. Modify toolToString Function Signature

- [ ] 2.1 Change toolToString return type from string to string list in Writers.fs
- [ ] 2.2 Wrap existing single-tool mappings in lists (e.g., ["write"], ["bash"])
- [ ] 2.3 Update all existing pattern match cases for Write, Edit, Bash, WebFetch, Todo, Custom

## 3. Implement Tool Mappings for Opencode Harness

- [ ] 3.1 Add Shell → ["bash"] mapping for Opencode
- [ ] 3.2 Add Read → ["read"] mapping for Opencode
- [ ] 3.3 Add Glob → ["grep"] mapping for Opencode
- [ ] 3.4 Add List → ["list"] mapping for Opencode
- [ ] 3.5 Add LSP → ["lsp"] mapping for Opencode
- [ ] 3.6 Add Skill → ["skill"] mapping for Opencode
- [ ] 3.7 Add TodoWrite → ["todowrite"] mapping for Opencode
- [ ] 3.8 Add TodoRead → ["todoread"] mapping for Opencode
- [ ] 3.9 Add WebSearch → [] (empty, not supported) mapping for Opencode
- [ ] 3.10 Add Question → ["question"] mapping for Opencode

## 4. Implement Tool Mappings for ClaudeCode Harness

- [ ] 4.1 Add Shell → ["Bash"] mapping for ClaudeCode
- [ ] 4.2 Add Read → ["Read"] mapping for ClaudeCode
- [ ] 4.3 Add Glob → ["Glob"] mapping for ClaudeCode
- [ ] 4.4 Add List → ["Glob"] mapping for ClaudeCode
- [ ] 4.5 Add LSP → ["LSP"] mapping for ClaudeCode
- [ ] 4.6 Add Skill → ["skill"] mapping for ClaudeCode
- [ ] 4.7 Add TodoWrite → ["TaskCreate"; "TaskUpdate"] mapping for ClaudeCode (one-to-many)
- [ ] 4.8 Add TodoRead → ["TaskList"; "TaskGet"; "TaskUpdate"] mapping for ClaudeCode (one-to-many)
- [ ] 4.9 Add WebSearch → ["WebSearch"] mapping for ClaudeCode
- [ ] 4.10 Add Question → ["AskUserQuestion"] mapping for ClaudeCode

## 5. Implement Tool Mappings for Copilot Harness

- [ ] 5.1 Add Shell → ["execute"] mapping for Copilot
- [ ] 5.2 Add Read → ["read"] mapping for Copilot
- [ ] 5.3 Add Glob → ["search"] mapping for Copilot
- [ ] 5.4 Add List → ["search"] mapping for Copilot
- [ ] 5.5 Add LSP → [] (empty, not supported) mapping for Copilot
- [ ] 5.6 Add Skill → ["skill"] mapping for Copilot
- [ ] 5.7 Add TodoWrite → ["todo"] mapping for Copilot
- [ ] 5.8 Add TodoRead → ["todo"] mapping for Copilot
- [ ] 5.9 Add WebSearch → ["web"] mapping for Copilot
- [ ] 5.10 Add WebFetch → ["web"] mapping for Copilot
- [ ] 5.11 Add Question → [] (empty, not supported) mapping for Copilot

## 6. Update Tool Collection and Deduplication Logic

- [ ] 6.1 Update formatToolsFrontmatter to use List.collect instead of List.map
- [ ] 6.2 Add List.distinct after List.collect to deduplicate tool names
- [ ] 6.3 Verify that empty lists from unsupported tools are filtered out automatically

## 7. Testing

- [ ] 7.1 Add unit tests for one-to-many mappings (TodoWrite, TodoRead for ClaudeCode)
- [ ] 7.2 Add unit tests for deduplication (Glob + List → single "search" for Copilot)
- [ ] 7.3 Add unit tests for missing tool handling (WebSearch for Opencode returns empty)
- [ ] 7.4 Add unit tests for Custom tool pass-through across all harnesses
- [ ] 7.5 Verify all existing tests still pass with string list return type

## 8. Documentation

- [ ] 8.1 Update CLAUDE.md tool mapping table with all new tool variants
- [ ] 8.2 Document one-to-many mapping behavior and deduplication in CLAUDE.md
