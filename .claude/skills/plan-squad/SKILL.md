---
name: plan-squad
argument-hint: "<your plan>"
description: Reformat the user's raw plan into .mysquad/PLAN.md in the squad task format. Format and wording only, no questions, no added decisions. Use on '/plan-squad' or when the user pastes a plan for the squad.
---

Transcribe the user's plan into `.mysquad/PLAN.md` in the squad format. Every fact in the output comes from the user's words, the repo, or a gap marker. Decisions, scope and design stay with the user and the product-owner.

Work in the user's language for chat; write `.mysquad/PLAN.md` in English.

## Input

Call: `/plan-squad "<plan>"`. The plan is the argument:

$ARGUMENTS

Any shape works: prose, bullets, voice dump, issue list, or a file path (read the file). With an empty argument, ask once: "Paste your plan in any shape." Then proceed.

## Steps

1. **Read** the plan, the existing `.mysquad/PLAN.md` (keep its tasks and continue the numbering) and the repo file tree (to resolve file names the user mentioned).
2. **Goal**: 3-5 sentences in the user's own words. Replace the placeholder line. If `.mysquad/SPEC.md` exists, add `Source of truth: .mysquad/SPEC.md`.
3. **Tasks**: one task per item the user listed, in the user's order, with the user's terms. Split an item only along parts the user enumerated. Name each concept with one term across all tasks.
4. **Fields**: fill each from the user's words, use a gap marker where the user gave nothing.
5. **Write** `.mysquad/PLAN.md`, then report (see Report).

## Task format

```
### T-007 <imperative verb + object, max 8 words>
- status: todo
- needs-design: yes | no | not given
- depends_on: T-003 | - | not given
- files: path/a.py, path/b.py | unknown
- seams: public interface to test at | not given
- acceptance:
  - <one observable statement per line>
```

## Wording rules

- Title: imperative verb first ("Add CSV export to report page").
- Acceptance: one observable, testable statement per line, present tense, stating the target behaviour ("Export button downloads a .csv with one row per order"). Keep the user's meaning; change only the form.
- A user criterion that cannot be observed ("works well") stays verbatim with the suffix `(vague)`.
- `files:` lists paths the user named or that match a name the user gave in the file tree. No guessing beyond that.
- `seams:` lists public interfaces the user named (an endpoint, a function, a screen). No guessing.
- `needs-design: yes` only when the user said design is needed; `depends_on: -` only when the user said there is none.
- Statuses of existing tasks stay as they are.

## Gap markers

`unknown`, `not given` and `(vague)` mark what the user left open. The product-owner fills them; the squad lead runs a task with any marker sequentially, after the product-owner has filled it.

## Report

End with a short list: number of tasks written, then one line per gap (`T-003 files unknown`, `T-005 acceptance vague`). Name the next step: product-owner fills the markers, `/grill-squad` settles the decisions behind them.

## Done when

- Every item of the user's plan appears in exactly one task.
- The output contains nothing the user did not say.
- Every missing field carries a marker.
- `.mysquad/PLAN.md` matches the task format and has no placeholder Goal.
