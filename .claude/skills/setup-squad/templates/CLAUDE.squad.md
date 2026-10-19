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
