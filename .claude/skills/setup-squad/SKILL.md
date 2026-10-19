---
name: setup-squad
argument-hint: "[check|upgrade]"
description: Check prerequisites and create the squad structure (agents, skills, settings, loop, `.mysquad/` state) in the current repo. Idempotent. Use on '/setup-squad' or 'setup squad'; argument 'check' only reports, 'upgrade' refreshes squad-owned files in a repo that already has the squad.
---

Set up the squad in the current repo. The script `scripts/setup_squad.py` in this skill's base directory does the checking and the file work; templates live in `templates/` beside it. Run the script, do not recreate its logic by hand.

Argument: `$ARGUMENTS` (`check` = report only, write nothing; `upgrade` = overwrite squad-owned files that differ).

## Steps

1. **Doctor**: run `python3 <base>/scripts/setup_squad.py --check` and show the result as a table. `<base>` is this skill's base directory.
2. **Stop on `check`**: with the argument `check`, end here.
3. **Consent**: list the `MISSING` items you can install yourself (`pip install code-review-graph`, `pip install uv`) and ask once. Install only after a yes.
4. **Structure**: run `python3 <base>/scripts/setup_squad.py --apply` (with `upgrade`: `--upgrade`). It creates what is absent, merges `.claude/settings.json` and `.mcp.json`, keeps existing files that differ, and writes the shared squad rules into a marked block of `CLAUDE.md`. The lead's rules live in `.claude/agents/lead.md`, which `settings.json` sets as the main-thread agent. Squad state (`team.md`, `routing.md`, `decisions.md`, `PLAN.md`, `STATUS.md`, `LEARNINGS.md`) and `ralph.sh` go into `.mysquad/`.
   - `--upgrade` also overwrites squad-owned files that differ (agents, squad skills, `ralph.sh`) and reports them as `upgraded`; `.mysquad/*.md` state is never touched. Show `git diff --stat` afterwards and tell the user that local edits to those files were replaced and can be restored from git.
5. **Graph**: when `code-review-graph` is present, run `code-review-graph install --platform claude-code`, then `code-review-graph build`, then `--apply` once more. The second apply restores the squad entries in `settings.json` and `.mcp.json` that the install may have rewritten.
6. **Verify**: run `--check` again and read the `Verify` section of `--apply`.
7. **Hand over** the commands only the user can run, each as a copyable line:
   - ponytail: `/plugin marketplace add DietrichGebert/ponytail`, then `/plugin install ponytail@ponytail`
   - missing initial commit or `git init`
8. **Next steps**, in this order: `/grill-squad` or `/plan-squad "<plan>"`, product-owner dry run on the result, `.mysquad/ralph.sh 10`. Suggest committing `.mysquad/` and `.claude/` so the squad travels with the repo; commit only when asked.

## Done when

- A second `--apply` reports only `same` or `unchanged`.
- `Verify` prints `ok`.
- Every remaining `MISSING` is a user action with its command shown.
