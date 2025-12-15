# OpenSpec Usage – Quick Guide for Agents in this Codebase

Audience: AI assistants and developers working in this repo who need a reliable, spec‑driven workflow to plan changes and implement them predictably.

Overview
- OpenSpec is a lightweight, spec‑driven workflow used to align humans and AI before coding. No API keys needed.
- Current truth lives in openspec/specs; proposed changes live in openspec/changes. Archiving merges approved deltas into specs.
- Use strict formatting for requirements and scenarios to enable automated validation and safe archival.

When to use OpenSpec
- Create a change proposal when adding features, making breaking changes, changing architecture/patterns, performance work that alters behavior, or security changes.
- Skip proposals for bug fixes that restore intended behavior, typos, formatting/comments, non‑breaking dependency updates, configuration tweaks, and tests for existing behavior.

Core workflow (3 stages)
1) Create proposal
   - Pick a unique change-id (kebab-case, verb‑led: add‑, update‑, remove‑, refactor‑).
   - Scaffold: changes/<id>/{proposal.md, tasks.md, optional design.md} and per‑capability delta specs under changes/<id>/specs/<capability>/spec.md.
   - Prefer modifying existing capabilities over duplicating specs.
   - Validate early: openspec validate <id> --strict.
2) Implement tasks
   - Read proposal.md (why/what), design.md if present (technical decisions), tasks.md (checklist).
   - Implement sequentially; mark tasks done only after completion.
   - Do not start implementation until proposal is approved.
3) Archive change
   - After deployment, run openspec archive <change-id> --yes to move changes/<id> → changes/archive/YYYY-MM-DD-<id> and update specs.
   - For tooling‑only changes, use --skip-specs to avoid spec updates.
   - Validate again: openspec validate --strict.

Spec and delta format (critical)
- Requirements use headings: "### Requirement: <name>".
- Every requirement must include at least one scenario using exactly "#### Scenario: <name>".
- Use SHALL/MUST for normative phrasing.
- Delta operations within change deltas:
  - ## ADDED Requirements – new capabilities.
  - ## MODIFIED Requirements – altered behavior; paste the full updated requirement block + scenarios.
  - ## REMOVED Requirements – deprecated features.
  - ## RENAMED Requirements – name changes; pair with MODIFIED if content changes.
- Pitfall: Using MODIFIED without pasting the previous full content drops detail during archive. If you’re adding orthogonal behavior, prefer ADDED.

Directory structure
- openspec/project.md – project conventions and context.
- openspec/specs/<capability>/spec.md – source of truth per capability.
- openspec/specs/<capability>/design.md – technical patterns (optional per capability).
- openspec/changes/<id>/{proposal.md, tasks.md, design.md} – proposal and plan.
- openspec/changes/<id>/specs/<capability>/spec.md – delta specs per affected capability.
- openspec/changes/archive/YYYY-MM-DD-<id>/ – archived, completed changes.

Before starting any task
- Read relevant specs in openspec/specs/<capability>/spec.md.
- Check pending changes in openspec/changes for conflicts.
- Read openspec/project.md for conventions.
- Enumerate current items: openspec list and openspec list --specs.
- If the request is ambiguous, ask 1–2 clarifying questions before scaffolding any change.

CLI essentials
- openspec list – list active changes.
- openspec list --specs – list specifications.
- openspec show <item> – display change or spec; use --type spec or --json when needed.
- openspec validate <item> --strict – comprehensive validation.
- openspec archive <change-id> [--yes|-y] – archive after deployment.
- Optional interactive modes: openspec show, openspec validate (bulk checks).

Search guidance
- Enumerate specs: openspec spec list --long (or --json).
- Enumerate changes: openspec list.
- Show details: openspec show <spec-id> --type spec; openspec show <change-id> --json --deltas-only.
- Full‑text search for requirements/scenarios: rg -n "Requirement:|Scenario:" openspec/specs.

Naming conventions
- Change IDs: kebab-case; prefer verb‑led prefixes add‑/update‑/remove‑/refactor‑; ensure uniqueness.
- Capability names: verb‑noun (user-auth, payment-capture); single purpose per capability.

Best practices
- Simplicity first: aim for <100 lines of new code; single‑file implementations until proven insufficient; avoid unnecessary frameworks.
- Add complexity only with real data/scale triggers.
- Clear references: use file.ts:42 format for code locations; reference specs by path (e.g., specs/auth/spec.md). Link related changes and PRs.

Troubleshooting
- "Change must have at least one delta": ensure changes/<id>/specs contains .md files with operation prefixes (## ADDED Requirements, etc.).
- "Requirement must have at least one scenario": confirm exact "#### Scenario:" headers.
- Silent scenario parsing failures: enforce exact format; debug with openspec show <change-id> --json --deltas-only.

Integration with AI tools
- Many assistants support native slash commands (e.g., Claude Code, Cursor, Copilot, OpenCode): /openspec:proposal, /openspec:apply, /openspec:archive.
- If slash commands aren’t available, ask in natural language: "Create an OpenSpec change proposal for …"; follow the workflow above.
- Keep AGENTS.md managed block intact so openspec update can refresh instructions.

This repo’s key references
- openspec/AGENTS.md – authoritative instructions for assistants; open this file whenever requests mention proposals, specs, changes, or plans.
- openspec/project.md – project context and conventions used throughout.
- openspec/specs/* – current truth for capabilities (agent-ast, agent-dsl, markdown-writer, bootstrap-solution).

External references (for deeper background)
- OpenSpec GitHub: https://github.com/Fission-AI/OpenSpec
- Community guide (DEV.to): How to make AI follow your instructions more (OpenSpec) – practical walkthrough of Proposal → Apply → Archive.

Summary
- Use OpenSpec to agree on behavior first. Draft proposals with precise requirement/scenario formatting, validate strictly, implement tasks sequentially, and archive to merge approved changes back into living specs. Specs are truth; changes are proposals—keep them in sync.
