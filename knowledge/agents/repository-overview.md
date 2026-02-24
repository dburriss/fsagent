An F# DSL and library for generating custom agent files for AI agent tools. The project uses stratified design: DSL layer builds AST, writers convert AST to output formats (Markdown, JSON), and low-level import pipeline handles file parsing and serialization.

Key directories:
- `/src` - Root project files (DSL, AST, Writers, Serialization modules)
- `/tests` - Test projects follow ABC style
- `/docs` - Basic docs for developers
- `/knowledge` - Summaries for compacting knowledge for AGENTS
- `.opencode/` - OpenCode plugin configuration and command definitions
- `openspec/` - Project specifications and change proposals