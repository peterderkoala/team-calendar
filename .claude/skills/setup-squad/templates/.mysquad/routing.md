# Routing

| Work | Goes to |
|---|---|
| Goal or spec to tasks | product-owner |
| Gap markers in `PLAN.md` (`unknown`, `not given`, `(vague)`) | product-owner, before the task runs |
| Task with `needs-design: yes` | architect, then developer |
| Implement one task | developer, own worktree, max 3 in parallel |
| Tests, linter, build | tester |
| Diff review, security | reviewer, must answer "no blocking issues" |
| Block (`BLOCKED:`), finding tagged `[plan]`, or the same finding returning after a fix round | architect corrects, lead hands the correction to the developer |
| Open question | human, answers by editing `.mysquad/STATUS.md` |
| Cross-cutting decision | lead, recorded in `.mysquad/decisions.md` |
