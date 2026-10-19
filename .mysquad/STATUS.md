# STATUS

## Current batch
- T-001 | waiting on B-1 (environment), no code written yet

## Done
(none)

## Blocked
<!-- format: B-n | T-id | category | with architect / resolved by C-n -->
B-1 | T-001 | missing-dependency (environment) | with human, see Q-3. Not a design question, so it was not sent to the architect

## Open questions
<!-- format: Q-n: question | assumption made | affected tasks. The human answers by appending `| ANSWER: <text>` to the line; the squad processes answers at the start of each round. -->
Q-1: May an admin delete their own account? SPEC is silent | Assumed no: DELETE on self returns 400 so the instance never loses its last admin (safety guard, beyond SPEC) | T-007
Q-2: The plan has 24 tasks, but the budget is 10 rounds × 2 developers. Cut scope or extend the budget? | Assumed: keep all 24 and pair small UI tasks (T-018..T-024) in the same round. Raise the round budget if needed | all
Q-3: Agents cannot run the stack's tools. `.claude/settings.json` allows only python/node tools, so `dotnet` and `docker` need approval and are refused in this non-interactive run. In isolated worktrees, the rtk hook rewrites `git` to `rtk git`, which worktree isolation refuses | Nothing assumed. The lead did not widen its own permissions. Proposed fix: add `Bash(dotnet *)`, `Bash(docker build*)`, `Bash(docker info*)`, `Bash(docker compose*config*)` to permissions.allow, and exempt git from the rtk rewrite in worktrees (or allow `rtk git *`) | T-001 and every later task

<!-- The squad lead writes "ALL DONE" as the very last line when every task is done. -->
