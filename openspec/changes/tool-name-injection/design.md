## Context

Prompt templates in `src/FsAgent/Writers.fs` are rendered via the `Template` module using the [Fue](https://github.com/slaveOftime/Fue) library. Currently, `renderInline` and `renderFile` accept a `TemplateVariables` map and pass it directly to Fue's `fromText`/`fromFile` functions. Tool names are hard-coded as string literals in templates, making them harness-specific.

The `AgentHarness` DU (`Opencode | Copilot | ClaudeCode`) lives inside `MarkdownWriter` (Writers.fs:41). The `Tool` DU lives in `Tools.fs`. The `writeMd` function's `Template` and `TemplateFile` branches call `Template.renderInline`/`renderFile` directly with `opts.TemplateVariables`.

Fue supports injecting functions as values in the data context, which enables `{{{tool Bash}}}` style interpolation if a `tool` function is registered in the context map.

## Goals / Non-Goals

**Goals:**
- Enable `{{{tool <Name>}}}` syntax in templates that resolves to the harness-correct tool name at write time
- Add `renderWithHarness` and `renderFileWithHarness` to the `Template` module that inject the `tool` function into the Fue context
- Build a `toolNameMap` inside `MarkdownWriter` that maps `Tool` DU case name strings to `Tool` values, then resolves them via the existing harness tool-name logic
- Update `writeMd`'s `Template` and `TemplateFile` branches to use the harness-aware render functions when a harness is present in `opts`
- Move `AgentHarness` type definition above the `Template` module to resolve the forward-dependency

**Non-Goals:**
- No AST changes
- No DSL changes
- No breaking changes to existing `renderInline`/`renderFile` behaviour or callers
- No changes to how non-tool template variables are resolved

## Decisions

### 1. Inject `tool` as a Fue function value, not a pre-expanded variable map

**Decision:** Register a `tool: string -> string` function in the Fue data context rather than pre-expanding all tool names into separate variables (e.g., `toolBash`, `toolRead`).

**Rationale:** Fue supports function injection. A single `tool` function keeps the template syntax minimal (`{{{tool Bash}}}`) and avoids polluting the variable map with N entries per harness. Pre-expansion would require callers to know which tools exist and would not compose well with `Custom of string`.

**Alternative considered:** Pre-expand all tools into a `Map<string,string>` of `toolBash → "bash"`, etc. Rejected because it leaks implementation details into every template and requires updating variable maps when new `Tool` cases are added.

---

### 2. `toolNameMap` lookup by DU case name string; `toolToString` injected as parameter

**Decision:** Build a `toolNameMap: Map<string, Tool>` using the string name of each `Tool` case (e.g., `"Bash" → Bash`, `"Read" → Read`) inside `MarkdownWriter`. Pass it — along with `toolToString` — as parameters into `renderWithHarness`/`renderFileWithHarness` rather than referencing them directly. The injected `tool` function closes over these parameters to resolve names. Unknown names fall through to `Tool.Custom name`, returning the argument string unchanged — consistent with the existing `Custom` pass-through in `toolToString`.

**Rationale:** Fue functions receive string arguments. Matching by DU case name is the most direct mapping between template syntax (`{{{tool Bash}}}`) and the F# type. Passing `toolToString` as a parameter avoids a forward-dependency within `Writers.fs` (the `Template` module is compiled before `MarkdownWriter`). The `Custom` fallback is the existing behaviour for unrecognised tool strings and requires no special-casing.

**Alternative considered:** Use `Reflection` to enumerate `Tool` cases dynamically. Rejected due to complexity and fragility; an explicit map is simpler and avoids a runtime dependency on reflection.

---

### 3. Harness-aware render as new functions, not replacement

**Decision:** Add `renderWithHarness` and `renderFileWithHarness` alongside the existing `renderInline`/`renderFile`. The `writeMd` branches switch to the harness-aware variants; existing callers remain unaffected.

**Rationale:** Preserves the current behaviour for callers that do not use tool injection. Avoids making `AgentHarness` a required parameter of the core render functions, which would break existing usages and violate the non-goal of no breaking changes.

---

### 4. Move `AgentHarness` above `Template` module

**Decision:** Relocate the `AgentHarness` type definition to before the `Template` module in `Writers.fs`.

**Rationale:** `renderWithHarness` needs `AgentHarness` as a parameter type. F# requires types to be defined before use in the same file. Since both `Template` and `MarkdownWriter` live in `Writers.fs`, this is the minimal structural change needed.

## Risks / Trade-offs

- **Fue function injection API stability** → Fue's `add` accepts `obj`; injecting an `FSharpFunc` should work but is undocumented behaviour. Mitigation: cover with acceptance tests against all three harnesses to catch regressions early.
- **Unknown `Tool` case name in template** → Unknown names fall through to `Tool.Custom name`, which `toolToString` returns as-is for all harnesses. No special handling needed; this is the defined behaviour.
- **`writeMd` harness source** → `writeMd` uses `ctx.Format` (already in scope) as the `AgentHarness`; it is always passed to `renderWithHarness`. There is no optional/None fallback path — the render functions always receive a harness.
