# LEARNINGS

Short retro notes. The squad lead appends here after tasks that needed 2+ fix iterations.
Format: `- T-007: what went wrong -> rule for next time`

## Rules learned
- T-001: the developer was dispatched before anyone checked that the squad may run the stack's tools (dotnet, docker, git in a worktree) -> before the first developer of a new stack, the lead runs each gate command once (`dotnet --version`, `docker info`) and stops if any needs approval
- Design round: the architect's notes surfaced a gate that locked out logout (T-004) and a missing DTO file (T-008) before any code existed -> while the environment blocks developers, use the round for design notes; they catch plan gaps cheaply
- T-001: the agent worktree branched from `bc01e04 Initial commit` instead of dev, so the developer missed the tracked CLAUDE.md and created its own -> right after a developer reports, check `git -C <wt> log -1` against dev and rebase before review; tell developers to edit tracked files, never recreate them
- T-001: a non-blocking reviewer note ("`GET /` seam untested") hid a real bug: adding `nonfile` made `/` return 404 -> every seam listed in a task gets at least one test, even the obvious ones
- T-001: a lead commit with pathspec-less `git commit` swept in the human's pre-staged deletions -> the lead always commits with an explicit pathspec (`git commit -- <files>`) and stashes human WIP around merges

## Ideas (out of scope, parked)
- Add `.claude/worktrees/` to .gitignore (it shows up as untracked in the main checkout)
