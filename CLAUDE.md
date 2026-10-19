# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Team calendar: ASP.NET Core API hosting a Blazor WebAssembly client (MudBlazor), PostgreSQL via EF Core. Squad rules live in `.mysquad/`.

## Commands (repo root, SDK pinned in `global.json`)
- Build: `dotnet build -warnaserror`
- Test: `dotnet test` (needs a running Docker daemon: Testcontainers starts `postgres:17`)
- Format check: `dotnet format --verify-no-changes`
- Docker: `docker build .` (Dockerfile arrives in T-002)

## Layout
- `TeamCalendar.slnx`
- `src/TeamCalendar.Api`: host, controllers, EF Core `AppDbContext`, migrations in `Data/Migrations/` (applied at startup)
- `src/TeamCalendar.Client`: Blazor WASM, served by the Api
- `src/TeamCalendar.Shared`: request/response DTOs
- `tests/TeamCalendar.Tests`: xUnit, `WebApplicationFactory<Program>` via `Infrastructure/ApiFactory` (fresh database per instance)

## Config
- `ConnectionStrings:Default` (Postgres), `TZ` (instance time zone)

## Ambiguity

On ambiguity read .mysquad/SPEC.md. If still unclear, log an OPEN QUESTION in .mysquad/STATUS.md and continue with other tasks.

## Branches

Work happens on `dev`; `main` is the PR target.

<!-- squad:start -->
# Squad

This repo is worked on by a squad: the main session runs as the `lead` agent (`.claude/agents/lead.md`) and delegates to the subagents product-owner, architect, developer, tester and reviewer. Each role's own rules are in its agent file; the rules below apply to every role.

## State
Squad state lives in `.mysquad/` (commit it): `team.md` roster, `routing.md` who handles what, `decisions.md` binding decisions, `SPEC.md` (optional, source of truth when present), `PLAN.md` backlog, `STATUS.md` progress and open questions, `LEARNINGS.md` retro notes.
- Read `decisions.md` before you start; it binds every role.
- In a git worktree, `.mysquad/` is a stale copy. Read squad state from the `TEAM_ROOT` given in your prompt.

## Ponytail (always full)
- Development, design and review run under `/ponytail full`: reuse before writing, minimum that works.
- The reviewer runs `/ponytail-review` on every diff, in addition to its own correctness and security checks.
- Ponytail never overrides validation at trust boundaries, error handling, security measures or accessibility basics.

## Code graph (one truth about the codebase)
- `code-review-graph` indexes the repo in `.code-review-graph/` and installs four skills into `.claude/skills/`: `explore-codebase`, `review-changes`, `debug-issue`, `refactor-safely`. Structure questions go to the graph first, then to grep.
- Keep it current: `code-review-graph update --brief` after every commit and before every review. Each worktree has its own graph; build it with `code-review-graph build` when missing.
- The graph shows structure and blast radius. Behaviour is verified by tests.
- Graph and files disagree: update the graph, then trust the files.

## Hard rules
- Small tasks beat clever tasks. Split if a task touches more than ~5 files.
- No scope creep: ideas go to .mysquad/LEARNINGS.md under "Ideas", not into the diff.
- Never force-push, never touch main directly, never delete data.
<!-- squad:end -->

<!-- code-review-graph MCP tools -->
## MCP Tools: code-review-graph

**This project has a knowledge graph. Start with the code-review-graph
MCP tools to narrow scope, then read the source.** The graph is cheaper than scanning files and
gives you structural context (callers, dependents, test coverage) that file search cannot.

### When to use graph tools FIRST

- **Exploring code**: `semantic_search_nodes_tool` or `query_graph_tool` instead of Grep
- **Understanding impact**: `get_impact_radius_tool` instead of manually tracing imports
- **Code review**: `detect_changes_tool` + `get_review_context_tool` instead of reading entire files
- **Finding relationships**: `query_graph_tool` with callers_of/callees_of/imports_of/tests_for
- **Architecture questions**: `get_architecture_overview_tool` + `list_communities_tool`

### Verify in the source

- Narrow scope with the graph, then read the source. Do not change code from graph output alone.
- For any non-trivial change, read the implementation and the relevant tests before concluding.
- Verify the exact source when touching behavior, database logic, migrations, retries, fallbacks,
  recovery, or compatibility code.
- When the graph and the source disagree, the source wins. The graph may be stale or may not
  model that relationship.
- An empty graph result can mean "not indexed" or "not statically visible", not "does not exist".

### Key Tools

| Tool | Use when |
| ------ | ---------- |
| `detect_changes_tool` | Reviewing code changes — gives risk-scored analysis |
| `get_review_context_tool` | Need source snippets for review — token-efficient |
| `get_impact_radius_tool` | Understanding blast radius of a change |
| `get_affected_flows_tool` | Finding which execution paths are impacted |
| `query_graph_tool` | Tracing callers, callees, imports, tests, dependencies |
| `semantic_search_nodes_tool` | Finding functions/classes by name or keyword |
| `get_architecture_overview_tool` | Understanding high-level codebase structure |
| `refactor_tool` | Planning renames, finding dead code |

### Workflow

1. The graph auto-updates on file changes (via hooks).
2. Use `detect_changes_tool` for code review.
3. Use `get_affected_flows_tool` to understand impact.
4. Use `query_graph_tool` pattern="tests_for" to check coverage.
<!-- /code-review-graph MCP tools -->
