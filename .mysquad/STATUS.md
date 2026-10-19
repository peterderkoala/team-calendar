# STATUS

## Current batch
- Round 2026-10-19: Q-2/Q-3 answered (D-27, D-28), B-1 resolved. Lead checked `dotnet --version` (10.0.112) and `docker info` (29.9.1): both run without approval
- T-001 | in progress (developer, sequential: every other task depends on it)
- Design round (2026-10-19, no stack tools needed): the architect added `design:` notes to T-003, T-004, T-006, T-008, T-012, T-014, T-015 and T-016, plus D-25 and D-26. The product-owner exempted logout from the must-change gate (T-004) and added Shared/AuthDtos.cs to T-008's files.

## Done
(none)

## Blocked
<!-- format: B-n | T-id | category | with architect / resolved by C-n -->
B-1 | T-001 | missing-dependency (environment) | resolved by human (Q-3 answer, D-28)

## Open questions
<!-- format: Q-n: question | assumption made | affected tasks. The human answers by appending `| ANSWER: <text>` to the line; the squad processes answers at the start of each round. -->
Q-1: May an admin delete their own account? SPEC is silent | Assumed no: DELETE on self returns 400 so the instance never loses its last admin (safety guard, beyond SPEC) | T-007. NOTE (lead): the answer given ("updated settings.json") looks like it was pasted from Q-3 and does not say yes or no, so the assumption stands. Please answer again with yes/no

<!-- The squad lead writes "ALL DONE" as the very last line when every task is done. -->
