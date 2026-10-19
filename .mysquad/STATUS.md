# STATUS

## Current batch
- T-001 | in progress | developer (worktree)

## Done
(none)

## Blocked
(none)  <!-- format: B-n | T-id | category | with architect / resolved by C-n -->

## Open questions
<!-- format: Q-n: question | assumption made | affected tasks. The human answers by appending `| ANSWER: <text>` to the line; the squad processes answers at the start of each round. -->
Q-1: May an admin delete their own account? SPEC is silent | Assumed no: DELETE on self returns 400 so the instance never loses its last admin (safety guard, beyond SPEC) | T-007
Q-2: The plan has 24 tasks, but the budget is 10 rounds × 2 developers. Cut scope or extend the budget? | Assumed: keep all 24 and pair small UI tasks (T-018..T-024) in the same round. Raise the round budget if needed | all

<!-- The squad lead writes "ALL DONE" as the very last line when every task is done. -->
