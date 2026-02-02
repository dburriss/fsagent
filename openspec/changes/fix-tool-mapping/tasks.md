## 1. Extend Tool Type with New Variants

- [x] 1.1 Add Read tool variant to Tool discriminated union in AST.fs
- [x] 1.2 Add Glob tool variant to Tool discriminated union in AST.fs
- [x] 1.3 Add List tool variant to Tool discriminated union in AST.fs
- [x] 1.4 Add LSP tool variant to Tool discriminated union in AST.fs
- [x] 1.5 Add Skill tool variant to Tool discriminated union in AST.fs
- [x] 1.6 Add TodoWrite tool variant to Tool discriminated union in AST.fs
- [x] 1.7 Add TodoRead tool variant to Tool discriminated union in AST.fs
- [x] 1.8 Add WebSearch tool variant to Tool discriminated union in AST.fs
- [x] 1.9 Add Question tool variant to Tool discriminated union in AST.fs
- [x] 1.10 Add Shell tool variant to Tool discriminated union in AST.fs
- [x] 1.11 Add XML documentation to each new Tool variant describing its purpose and mapping

## 2. Modify toolToString Function Signature

- [x] 2.1 Change toolToString return type from string to string list in Writers.fs
- [x] 2.2 Wrap existing single-tool mappings in lists (e.g., ["write"], ["bash"])
- [x] 2.3 Update all existing pattern match cases for Write, Edit, Bash, WebFetch, Todo, Custom

## 3. Implement Tool Mappings for Opencode Harness

- [x] 3.1 Add Shell → ["bash"] mapping for Opencode
- [x] 3.2 Add Read → ["read"] mapping for Opencode
- [x] 3.3 Add Glob → ["grep"] mapping for Opencode
- [x] 3.4 Add List → ["list"] mapping for Opencode
- [x] 3.5 Add LSP → ["lsp"] mapping for Opencode
- [x] 3.6 Add Skill → ["skill"] mapping for Opencode
- [x] 3.7 Add TodoWrite → ["todowrite"] mapping for Opencode
- [x] 3.8 Add TodoRead → ["todoread"] mapping for Opencode
- [x] 3.9 Add WebSearch → [] (empty, not supported) mapping for Opencode
- [x] 3.10 Add Question → ["question"] mapping for Opencode

## 4. Implement Tool Mappings for ClaudeCode Harness

- [x] 4.1 Add Shell → ["Bash"] mapping for ClaudeCode
- [x] 4.2 Add Read → ["Read"] mapping for ClaudeCode
- [x] 4.3 Add Glob → ["Glob"] mapping for ClaudeCode
- [x] 4.4 Add List → ["Glob"] mapping for ClaudeCode
- [x] 4.5 Add LSP → ["LSP"] mapping for ClaudeCode
- [x] 4.6 Add Skill → ["skill"] mapping for ClaudeCode
- [x] 4.7 Add TodoWrite → ["TaskCreate"; "TaskUpdate"] mapping for ClaudeCode (one-to-many)
- [x] 4.8 Add TodoRead → ["TaskList"; "TaskGet"; "TaskUpdate"] mapping for ClaudeCode (one-to-many)
- [x] 4.9 Add WebSearch → ["WebSearch"] mapping for ClaudeCode
- [x] 4.10 Add Question → ["AskUserQuestion"] mapping for ClaudeCode

## 5. Implement Tool Mappings for Copilot Harness

- [x] 5.1 Add Shell → ["execute"] mapping for Copilot
- [x] 5.2 Add Read → ["read"] mapping for Copilot
- [x] 5.3 Add Glob → ["search"] mapping for Copilot
- [x] 5.4 Add List → ["search"] mapping for Copilot
- [x] 5.5 Add LSP → [] (empty, not supported) mapping for Copilot
- [x] 5.6 Add Skill → ["skill"] mapping for Copilot
- [x] 5.7 Add TodoWrite → ["todo"] mapping for Copilot
- [x] 5.8 Add TodoRead → ["todo"] mapping for Copilot
- [x] 5.9 Add WebSearch → ["web"] mapping for Copilot
- [x] 5.10 Add WebFetch → ["web"] mapping for Copilot
- [x] 5.11 Add Question → [] (empty, not supported) mapping for Copilot

## 6. Update Tool Collection and Deduplication Logic

- [x] 6.1 Update formatToolsFrontmatter to use List.collect instead of List.map
- [x] 6.2 Add List.distinct after List.collect to deduplicate tool names
- [x] 6.3 Verify that empty lists from unsupported tools are filtered out automatically

## 7. Testing

- [x] 7.1 Add unit tests for one-to-many mappings (TodoWrite, TodoRead for ClaudeCode)
- [x] 7.2 Add unit tests for deduplication (Glob + List → single "search" for Copilot)
- [x] 7.3 Add unit tests for missing tool handling (WebSearch for Opencode returns empty)
- [x] 7.4 Add unit tests for Custom tool pass-through across all harnesses
- [x] 7.5 Verify all existing tests still pass with string list return type

## 8. Documentation

- [x] 8.1 Update CLAUDE.md tool mapping table with all new tool variants
- [x] 8.2 Document one-to-many mapping behavior and deduplication in CLAUDE.md
