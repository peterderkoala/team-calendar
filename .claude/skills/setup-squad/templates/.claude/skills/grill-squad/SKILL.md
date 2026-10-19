---
name: grill-squad
description: Grill the user about a software idea until the squad can run unattended, then write .mysquad/SPEC.md, .mysquad/PLAN.md goal and OPEN QUESTION rule. Use before .mysquad/ralph.sh or on 'grill squad'.
---

Interview the user relentlessly until the squad can run unattended. Nobody answers questions at night, so every gap left open becomes a guess. Close the gaps now.

Interview in the user's language. Write files in English.

## 1. Recon

Facts are your job, never the user's. Before the first question, read the repo: `.mysquad/SPEC.md`, `.mysquad/PLAN.md`, `.mysquad/STATUS.md`, `.mysquad/LEARNINGS.md`, `CLAUDE.md`, `.claude/agents/`, test/lint/build commands, git state. For anything broader, dispatch a sub-agent and ask the unblocked questions meanwhile.

## 2. Grill

Map the idea as a **design tree**: every decision branches into the decisions hanging off it. The **frontier** is every decision whose prerequisites are settled. Ask the whole frontier in one round, numbered, each with your recommended answer, then wait. Settled answers push the frontier outward; recompute and ask the next round.

```
❓ **Q1** - **<title>**: <question, with choices where useful>

➡️ <your recommended answer>
```

Branches to cover:

- **Problem and user**: who needs what, and why now.
- **v1 slice**: the smallest version that is useful.
- **Non-goals**: what v1 excludes.
- **Stack and constraints**: language, frameworks, hosting, licences, existing code to respect.
- **Gates**: the exact commands for tests, linter, build. A missing gate becomes task T-001.
- **Acceptance**: per feature, each criterion checkable by a test.
- **Seams**: per feature, the public interface the developer tests at (endpoint, function, screen). The developer tests nowhere else, so an unnamed seam blocks the task.
- **Hot spots**: parts that need an architect design note before coding.
- **Autonomy**: which decisions the squad makes alone, which become an `OPEN QUESTION` in `.mysquad/STATUS.md`.
- **Guardrails**: protected files, services, data; actions that need the human.
- **Run budget**: max rounds, max parallel developers.

A branch is visited when its decisions are settled or consciously parked as an `OPEN QUESTION`.

## 3. Confirm

When the frontier is empty, summarise the shared understanding in at most ten lines and wait for the user's explicit confirmation. Write nothing before it.

## 4. Write

After confirmation:

- `.mysquad/SPEC.md`, one page, sections: Problem, Users, v1 Scope, Non-goals, Stack and constraints, Gates (exact commands), Acceptance (testable), Seams, Autonomy rules.
- `.mysquad/decisions.md`: one line per settled question as `D-n | date | decision | reason`.
- `.mysquad/PLAN.md`: replace the Goal line with 3-5 sentences plus `Source of truth: .mysquad/SPEC.md`. Leave tasks to the product-owner.
- `CLAUDE.md`: ensure this rule exists, add it if missing: `On ambiguity read .mysquad/SPEC.md. If still unclear, log an OPEN QUESTION in .mysquad/STATUS.md and continue with other tasks.`
- `.mysquad/STATUS.md`: ensure an `## Open questions` section exists.

## 5. Done when

- `.mysquad/SPEC.md` has every section filled, with no TBD.
- Every acceptance criterion is checkable by a test.
- Every gate names a command.
- The user confirmed the summary.

Then hand off (.mysquad/SPEC.md is optional for the loop, but it is what the squad treats as source of truth when present) with these two steps:

1. Run the product-owner alone on `.mysquad/SPEC.md` and review `.mysquad/PLAN.md`.
2. Start the loop with `.mysquad/ralph.sh 10`.
