# LEARNINGS

Short retro notes. The squad lead appends here after tasks that needed 2+ fix iterations.
Format: `- T-007: what went wrong -> rule for next time`

## Rules learned
- T-001: the developer was dispatched before anyone checked that the squad may run the stack's tools (dotnet, docker, git in a worktree) -> before the first developer of a new stack, the lead runs each gate command once (`dotnet --version`, `docker info`) and stops if any needs approval
- Design round: the architect's notes surfaced a gate that locked out logout (T-004) and a missing DTO file (T-008) before any code existed -> while the environment blocks developers, use the round for design notes; they catch plan gaps cheaply
- T-001: the agent worktree branched from `bc01e04 Initial commit` instead of dev, so the developer missed the tracked CLAUDE.md and created its own -> right after a developer reports, check `git -C <wt> log -1` against dev and rebase before review; tell developers to edit tracked files, never recreate them
- T-001: a non-blocking reviewer note ("`GET /` seam untested") hid a real bug: adding `nonfile` made `/` return 404 -> every seam listed in a task gets at least one test, even the obvious ones
- T-001: a lead commit with pathspec-less `git commit` swept in the human's pre-staged deletions -> the lead always commits with an explicit pathspec (`git commit -- <files>`) and stashes human WIP around merges
- Round 2: the `developer` agent's auto-worktree sandboxes writes to its own path (and may branch from an old commit), and background agents can't get approval prompts -> the lead creates `t-NNN` worktrees from dev, runs the developer role as a non-isolated agent pinned to that path, and gives every subagent the exact single-command Bash forms (`dotnet <verb> <abs>/TeamCalendar.slnx`, `docker build <abs>`); no cd/&&/pipes, files via Read/Write
- T-002: a design risk ("the 10.0.112 tag must exist") was left for the tester instead of being checked before dispatch -> when a design pins an external artifact (image tag, package version), the lead verifies it exists before the developer starts
- Lead: `Read(.env*)` deny rule also blocks writing `.env.example` -> check deny rules against every file a task's `files:` lists before dispatch

## Ideas (out of scope, parked)
- Add `.claude/worktrees/` to .gitignore (it shows up as untracked in the main checkout)
