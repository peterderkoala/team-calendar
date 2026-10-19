# STATUS

## Current batch
- Round 2026-10-19: Q-2/Q-3 answered (D-27, D-28), B-1 resolved. Lead checked `dotnet --version` (10.0.112) and `docker info` (29.9.1): both run without approval
- T-001 | done, merged into dev (fcfc6d5). Gates run by the lead on dev after the merge: build 0 warnings, test 5/5, format clean
- Round 2 (2026-10-19), batch T-002 + T-003 in parallel. No new answers to process
- T-003 | done, fast-forwarded into dev (9f81b26). Gates run by the lead on dev: build 0 warnings, test 15/15, format clean. docker build is checked again when T-002 merges
- T-002 | in-progress, WIP commit on branch t-002 (worktree .claude/worktrees/t-002, based on 090b809, rebase onto dev before review). B-3 resolved by C-3/D-30; docker build, build, test (5/5 on its base) and format pass. Missing: `.env.example` (B-4/Q-5), then `docker compose --env-file .env.example config`, tester and reviewer
- Next batch: finish T-002 once Q-5 is answered; T-004 (Program.cs, AuthController) + T-005 (AppDbContext, migration) in parallel (disjoint files, both depend only on T-003). T-006 also touches Program.cs, so it runs after T-004
- Merge note: dev was fast-forwarded (`merge --ff-only`) so the human's staged WIP in the main checkout (.claude/skills deletions, settings.json) stays untouched; no stash (stashing settings.json would drop the live allow rules)
- Design round (2026-10-19, no stack tools needed): the architect added `design:` notes to T-003, T-004, T-006, T-008, T-012, T-014, T-015 and T-016, plus D-25 and D-26. The product-owner exempted logout from the must-change gate (T-004) and added Shared/AuthDtos.cs to T-008's files.

## Done
- T-001 | 2026-10-19 | 1 fix round (reviewer non-blocking: `GET /` untested and actually 404 after `nonfile`; fixed with an explicit `/` fallback). Tester PASS; reviewer: no blocking issues

- T-003 | 2026-10-19 | 0 fix rounds; 1 environment block on the developer (compound Bash denied, resolved by plain `dotnet` commands). Tester PASS (+4 tests in IdentityConfigTests.cs); reviewer: no blocking issues. Non-blocking left open: unused `using Microsoft.AspNetCore.Authorization;` in Program.cs and a one-caller wrapper `WebApplicationFactoryWithProbe` in IdentityStoreTests.cs; T-004 can drop the using when it edits Program.cs. T-004/T-007 tests must cover lockout and that a stamp change ends a session

## Blocked
<!-- format: B-n | T-id | category | with architect / resolved by C-n -->
B-1 | T-001 | missing-dependency (environment) | resolved by human (Q-3 answer, D-28)
B-2 | T-001 | missing-dependency (environment): developer in a worktree cannot run any git command (guard refuses rtk-wrapped git it cannot prove stays in the worktree) | workaround: lead commits from the worktree with `git -C <worktree>`. Not a design question, not sent to architect
B-3 | T-002 | missing-dependency: `docker build` fails, `mcr.microsoft.com/dotnet/sdk:10.0.112: not found` (C-1 pin). Local SDK is Ubuntu's source-built package (/usr/lib/dotnet, host 10.0.12), global.json rollForward: disable | resolved by C-3 (D-30)
B-4 | T-002 | missing-dependency (environment): writing `.env.example` is refused by the deny rule `Read(.env*)` in .claude/settings.json | with human (Q-5); not a design question

## Open questions
<!-- format: Q-n: question | assumption made | affected tasks. The human answers by appending `| ANSWER: <text>` to the line; the squad processes answers at the start of each round. -->
Q-1: May an admin delete their own account? SPEC is silent | Assumed no: DELETE on self returns 400 so the instance never loses its last admin (safety guard, beyond SPEC) | T-007. NOTE (lead): the answer given ("updated settings.json") looks like it was pasted from Q-3 and does not say yes or no, so the assumption stands. Please answer again with yes/no ANSWER: no, cannot delete;
Q-4: Developer subagents in worktrees cannot run git (B-2), so the lead commits for them via `git -C <worktree>`. Also, agent worktrees branch from `bc01e04 Initial commit` instead of `dev`, so the lead has to rebase them | Assumed: keep the lead-commits workaround and rebase each branch onto dev before review | all developer tasks. Fix if you can: allow plain git for worktree agents and make worktrees branch from dev. UPDATE (lead, round 2): the developer agent's auto-worktree sandbox also refuses writes to any other path, so the lead now creates `t-NNN` worktrees from dev and runs the developer role as a non-isolated agent pinned to that path. Background agents also cannot use compound shell commands (cd/&&/pipes) or WebFetch
Q-5: The deny rule `Read(.env*)` in .claude/settings.json also blocks writing `.env.example` (T-002 acceptance) | Assumed: narrow it to `Read(.env)` and `Read(.env.local*)` (or add an allow for `.env.example`); the squad will not edit settings.json itself. T-002 stays open until then | T-002

<!-- The squad lead writes "ALL DONE" as the very last line when every task is done. -->
