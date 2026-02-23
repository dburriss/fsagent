---
compatibility: Requires ck installed: https://beaconbay.github.io/ck/guide/installation.html
description: Teach an AI agent to use ck for semantic code search — finding code by meaning, not just keywords.
license: MIT
metadata: 
  author: FsAgent example
  tags: search,semantic,ck,codebase
  version: 1.0.0
name: ck-semantic-search
---

# Overview

ck (seek) is a hybrid code search tool that finds code by meaning using local AI embeddings.
It is fully grep-compatible and requires no API keys — all inference runs locally.

Search modes:
- `--sem`    Semantic search (by concept/meaning)
- `--lex`    Lexical search (exact keyword/text)
- `--hybrid` Combined semantic + keyword search
- `--regex`  Traditional regex pattern matching

# Setup

Index the project once before searching. ck maintains incremental updates automatically.

```bash
# Index the current directory
ck --index .

# Check index status
ck --status .

# Rebuild from scratch if needed
ck --clean . && ck --index .
```

Indexing ~100K LOC takes roughly 10 seconds. Subsequent searches return in under 500ms.

# Semantic Search

Use semantic search to find code by what it does, not what it is named.

```bash
# Find error handling logic
ck --sem "error handling" src/

# Find authentication code
ck --sem "user authentication" src/

# Find database connection logic
ck --sem "database connection pooling" src/

# Show relevance scores
ck --sem --scores "retry logic" src/
```

Semantic search works even when the exact words are not present in the code.

# Tuning Results

Control relevance threshold and result count to balance precision vs. recall.

```bash
# Default threshold (0.6) — good starting point
ck --sem "caching strategy" src/

# High confidence only (fewer, more relevant results)
ck --sem --threshold 0.7 "authentication" src/

# Broad search (cast a wider net)
ck --sem --threshold 0.4 "configuration loading" src/

# Limit to top 10 results (keeps context window manageable)
ck --sem --limit 10 "validation logic" src/

# Combine threshold and limit
ck --sem --threshold 0.7 --limit 20 "error handling" src/
```

Recommended defaults for AI agents: `--threshold 0.7 --limit 20`.

When no results are found, ck shows a near-miss hint with the closest match score
and suggests a lower threshold — use that feedback to retry with an adjusted value.

# Choosing a Search Mode

Pick the mode that fits the query:

| Mode       | Flag        | Use when                                   | Example                          |
|------------|-------------|--------------------------------------------|----------------------------------|
| Semantic   | `--sem`     | Looking for concepts or patterns           | "retry logic", "input validation" |
| Lexical    | `--lex`     | Looking for exact names or identifiers     | "getUserById", "AuthController"  |
| Hybrid     | `--hybrid`  | Balancing precision and recall             | "JWT token validation"           |
| Regex      | `--regex`   | Pattern matching                           | `class.*Handler`                 |

```bash
# Lexical: exact function name
ck --lex "renderSkill" src/

# Hybrid: concept + keyword
ck --hybrid "connection timeout" src/

# Regex: structural pattern
ck --regex "let render\w+" src/
```

# Output Formats

Use structured output for programmatic processing.

```bash
# JSONL — one result per line, optimal for streaming/parsing
ck --jsonl --sem "error handling" src/

# JSON — single array, good for small result sets
ck --json --sem "pattern" src/

# Metadata only (no snippets) — faster, smaller payloads
ck --jsonl --no-snippet --sem "pattern" src/
```

Prefer JSONL when piping results into further processing.

# Workflow Examples

Common search workflows for AI-assisted development:

```bash
# --- Codebase onboarding ---
ck --index .
ck --sem "main entry point" .
ck --sem "dependency injection" src/
ck --sem "API endpoints" src/

# --- Code review ---
ck --lex "TODO" .
ck --lex "FIXME" .
ck --sem --threshold 0.7 "error handling" src/
ck --sem "input validation" src/

# --- Refactoring ---
ck --sem "user validation" src/
ck --sem --threshold 0.8 "duplicate logic" src/

# --- Security audit ---
ck --sem "SQL queries" src/
ck --sem "authentication logic" src/
ck --sem "secret or credential" src/
```

# Using ck in This Agent

When asked to search the codebase, prefer ck over grep for conceptual queries.

Steps:
1. If no index exists yet, run `ck --index .` first (once per session).
2. Choose the search mode based on the query (see Choosing a Search Mode).
3. Start with `--threshold 0.7 --limit 20` for focused results.
4. If too few results, lower threshold to 0.5; if too many, raise to 0.8.
5. Use near-miss hints from ck to adjust threshold when no results are found.
Run ck commands using the bash tool.
