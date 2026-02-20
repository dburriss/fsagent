## 1. Structural Prerequisite

- [ ] 1.1 Move `AgentHarness` DU definition above the `Template` module in `Writers.fs`
- [ ] 1.2 Verify project compiles with no forward-reference errors after the move

## 2. Template Module – Harness-Aware Render Functions

- [ ] 2.1 Add `renderWithHarness` to the `Template` module: accepts inline template text, `toolNameMap: Map<string,Tool>`, `toolToString: AgentHarness -> Tool -> string`, `harness: AgentHarness`, and `TemplateVariables`; injects a `tool` function into the Fue context and returns the rendered string
- [ ] 2.2 Add `renderFileWithHarness` to the `Template` module: same signature but reads template from a file path; return `"[Template file not found: {path}]"` when the file is absent
- [ ] 2.3 Ensure existing `renderInline` and `renderFile` signatures and behaviour are unchanged

## 3. MarkdownWriter – toolNameMap and Dispatch

- [ ] 3.1 Build `toolNameMap: Map<string, Tool>` in `MarkdownWriter` mapping each `Tool` DU case name string (e.g., `"Bash"`) to its `Tool` value; include all current `Tool` cases
- [ ] 3.2 Update `writeMd`'s `Template` branch to call `renderWithHarness` with `ctx.Format`, `toolNameMap`, and `toolToString` instead of `renderInline`
- [ ] 3.3 Update `writeMd`'s `TemplateFile` branch to call `renderFileWithHarness` with the same arguments instead of `renderFile`

## 4. Acceptance Tests

- [ ] 4.1 Test `renderWithHarness` resolves `{{{tool Bash}}}` correctly for `Opencode` harness
- [ ] 4.2 Test `renderWithHarness` resolves `{{{tool Read}}}` correctly for `Copilot` harness
- [ ] 4.3 Test `renderWithHarness` resolves `{{{tool Bash}}}` correctly for `ClaudeCode` harness
- [ ] 4.4 Test `renderWithHarness` with an unknown tool name returns the name string unchanged (Custom fallback)
- [ ] 4.5 Test `renderWithHarness` preserves non-tool template variables alongside `{{{tool …}}}` substitutions
- [ ] 4.6 Test `renderFileWithHarness` resolves `{{{tool Read}}}` from a file for `Copilot` harness
- [ ] 4.7 Test `renderFileWithHarness` returns the "file not found" message when the path does not exist
- [ ] 4.8 Test `toolNameMap` lookup returns the correct `Tool` value for a known case name (e.g., `"Bash"`)

## 5. Build and Verify

- [ ] 5.1 Run `dotnet build` and confirm zero warnings/errors
- [ ] 5.2 Run `dotnet test` and confirm all tests pass
