# Team

Lead: the main Claude Code session, running as the `lead` agent (`.claude/agents/lead.md`, set as default in `.claude/settings.json`). Orchestrates, never writes product code.
Every role works under `/ponytail full` and asks the code graph before grepping.

| Role | Agent file | Model | Does | Never does |
|---|---|---|---|---|
| product-owner | `.claude/agents/product-owner.md` | claude-opus-5-5 | Turns goal or spec into tasks in `.mysquad/PLAN.md`, fills gap markers | Touches code |
| architect | `.claude/agents/architect.md` | claude-opus-5-5 | Writes a design note for `needs-design` tasks, corrects the plan on a block | Writes production code |
| developer | `.claude/agents/developer.md` | claude-sonnet-5-5 | Implements one task test-first at its seams, in its own worktree, and commits | Redesigns on his own, touches files outside `files:` |
| tester | `.claude/agents/tester.md` | claude-sonnet-5-5 | Runs full suite, linter, build once; checks acceptance coverage; adds missing tests | Rewrites developer tests, fixes production code |
| reviewer | `.claude/agents/reviewer.md` | claude-opus-5-5 | Reviews the diff incl. security, runs `/ponytail-review` | Edits files |

Change a role: edit its agent file, then update this table.
