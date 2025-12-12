# fsagent
A small DSL and library for generating custom agent files for popular agent tools

Provides an immutable AST for representing agent files, with constructors for common sections like role, objective, instructions, etc.

## Build & Test

```bash
dotnet build                    # Build project
dotnet test                     # Run unit tests
```

## Using OpenSpec

This project uses OpenSpec for spec-driven development. OpenSpec manages change proposals, specifications, and ensures structured development.

### Essential Commands

```bash
openspec list                  # List active changes
openspec list --specs          # List specifications
openspec show [item]           # Display change or spec details
openspec validate [item]       # Validate changes or specs
openspec archive <change-id> [--yes|-y]   # Archive completed changes
```

For planning changes or new features, create proposals in `openspec/changes/`. See `openspec/AGENTS.md` for detailed instructions.
