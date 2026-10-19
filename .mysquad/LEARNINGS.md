# LEARNINGS

Short retro notes. The squad lead appends here after tasks that needed 2+ fix iterations.
Format: `- T-007: what went wrong -> rule for next time`

## Rules learned
- T-001: the developer was dispatched before anyone checked that the squad may run the stack's tools (dotnet, docker, git in a worktree) -> before the first developer of a new stack, the lead runs each gate command once (`dotnet --version`, `docker info`) and stops if any needs approval
- Design round: the architect's notes surfaced a gate that locked out logout (T-004) and a missing DTO file (T-008) before any code existed -> while the environment blocks developers, use the round for design notes; they catch plan gaps cheaply

## Ideas (out of scope, parked)
(none yet)
