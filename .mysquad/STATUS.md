# STATUS

## Current batch
- Round 2026-10-19: Q-2/Q-3 answered (D-27, D-28), B-1 resolved. Lead checked `dotnet --version` (10.0.112) and `docker info` (29.9.1): both run without approval
- T-001 | done, merged into dev (fcfc6d5). Gates run by the lead on dev after the merge: build 0 warnings, test 5/5, format clean
- Next batch: T-002 (Docker files, with corrections C-1/C-2) and T-003 (Identity, Api files). Both depend only on T-001 and their files don't overlap, so they can run in parallel (2 developers, D-24)
- Design round (2026-10-19, no stack tools needed): the architect added `design:` notes to T-003, T-004, T-006, T-008, T-012, T-014, T-015 and T-016, plus D-25 and D-26. The product-owner exempted logout from the must-change gate (T-004) and added Shared/AuthDtos.cs to T-008's files.

## Done
- T-001 | 2026-10-19 | 1 fix round (reviewer non-blocking: `GET /` untested and actually 404 after `nonfile`; fixed with an explicit `/` fallback). Tester PASS; reviewer: no blocking issues

## Blocked
<!-- format: B-n | T-id | category | with architect / resolved by C-n -->
B-1 | T-001 | missing-dependency (environment) | resolved by human (Q-3 answer, D-28)
B-2 | T-001 | missing-dependency (environment): developer in a worktree cannot run any git command (guard refuses rtk-wrapped git it cannot prove stays in the worktree) | workaround: lead commits from the worktree with `git -C <worktree>`. Not a design question, not sent to architect

## Open questions
<!-- format: Q-n: question | assumption made | affected tasks. The human answers by appending `| ANSWER: <text>` to the line; the squad processes answers at the start of each round. -->
Q-1: May an admin delete their own account? SPEC is silent | Assumed no: DELETE on self returns 400 so the instance never loses its last admin (safety guard, beyond SPEC) | T-007. NOTE (lead): the answer given ("updated settings.json") looks like it was pasted from Q-3 and does not say yes or no, so the assumption stands. Please answer again with yes/no
Q-4: Developer subagents in worktrees cannot run git (B-2), so the lead commits for them via `git -C <worktree>`. Also, agent worktrees branch from `bc01e04 Initial commit` instead of `dev`, so the lead has to rebase them | Assumed: keep the lead-commits workaround and rebase each branch onto dev before review | all developer tasks. Fix if you can: allow plain git for worktree agents and make worktrees branch from dev

<!-- The squad lead writes "ALL DONE" as the very last line when every task is done. -->
