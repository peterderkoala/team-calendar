#!/usr/bin/env bash
# Autonomous outer loop. Fresh context each round, state lives in files.
# Usage: .mysquad/ralph.sh [max_rounds]   (default 10)
# Stop gracefully: touch .mysquad/stop   (the current round finishes first)
# A failed round without progress (usage/rate limit, API error) is retried with backoff
# (5, 10, 20, 40, 60, 60... minutes) and does not count as a round; SQUAD_MAX_FAILS (default 8) in a row end the run.
set -uo pipefail

cd "$(git rev-parse --show-toplevel 2>/dev/null)" || { echo "Not a git repo."; exit 1; }
mkdir -p .mysquad/log

MAX="${1:-10}"
export PONYTAIL_DEFAULT_MODE=full
STALE=0
FAILS=0
MAX_FAILS="${SQUAD_MAX_FAILS:-8}"
STOP=.mysquad/stop

# Human-in-the-loop: notify when the squad needs you.
# Set SQUAD_NOTIFY_CMD to a command that uses "$1" as the message, for example:
#   export SQUAD_NOTIFY_CMD='curl -s -H "Title: Squad" -d "$1" https://ntfy.sh/your-private-topic'
notify() {
  echo "[notify $(date +%H:%M)] $1" | tee -a .mysquad/log/notify.log
  if [ -n "${SQUAD_NOTIFY_CMD:-}" ]; then bash -c "$SQUAD_NOTIFY_CMD" _ "$1" >/dev/null 2>&1 || true; fi
}
open_questions() {
  awk '/^## Open questions/{f=1;next} /^## /{f=0} f && /Q-[0-9]+/ && !/ANSWER:/' .mysquad/STATUS.md
}
count_lines() { printf '%s' "$1" | grep -c . || true; }
finish() { notify "$1"; exit "$2"; }
check_stop() { if [ -e "$STOP" ]; then rm -f "$STOP"; finish "Squad stopped by $STOP. Open questions: $N_OLD." 3; fi; }
wait_min() { for _ in $(seq 1 "$1"); do check_stop; sleep 60; done; }
N_OLD="$(count_lines "$(open_questions)")"

# .mysquad/SPEC.md is optional. Without it the goal in .mysquad/PLAN.md is the only input.
if [ -s .mysquad/SPEC.md ]; then
  SPEC_NOTE="Read .mysquad/SPEC.md first (source of truth)."
else
  SPEC_NOTE="There is no .mysquad/SPEC.md. Use the Goal in .mysquad/PLAN.md as input, work autonomously, record every assumption as an OPEN QUESTION in .mysquad/STATUS.md."
  grep -q '<describe' .mysquad/PLAN.md && { echo "No .mysquad/SPEC.md and .mysquad/PLAN.md has no goal. Run /grill-squad or fill the Goal."; exit 1; }
fi

# Code graph is required: one truth about the codebase
command -v code-review-graph >/dev/null || { echo "Install: pip install code-review-graph"; exit 1; }
[ -d .claude/skills/review-changes ] || { echo "Graph skills missing. Run once: code-review-graph install --platform claude-code"; exit 1; }
[ -d .code-review-graph ] || code-review-graph build

# Never work on main
git rev-parse --git-dir >/dev/null 2>&1 || { echo "Not a git repo."; exit 1; }
BRANCH="$(git rev-parse --abbrev-ref HEAD)"
if [ "$BRANCH" = "main" ] || [ "$BRANCH" = "master" ]; then
  git checkout -b squad/work
fi

i=0
while [ "$i" -lt "$MAX" ]; do
  check_stop
  if tail -n 1 .mysquad/STATUS.md | grep -qE "^[[:space:]]*ALL DONE[[:space:]]*$"; then
    finish "Squad done after $i rounds. Open questions: $N_OLD." 0
  fi

  code-review-graph update --brief || true
  BEFORE="$(git rev-parse HEAD)$(md5sum .mysquad/STATUS.md .mysquad/PLAN.md | md5sum)"
  echo "=== Round $((i+1))/$MAX ($(date +%H:%M)) ==="

  timeout 90m claude -p "You are the squad lead. Follow your agent rules. $SPEC_NOTE Read .mysquad/PLAN.md, .mysquad/STATUS.md, .mysquad/LEARNINGS.md. First process answered open questions (lines with ANSWER:). \
If .mysquad/PLAN.md has no tasks, use the product-owner first. Otherwise take the next batch of open tasks, \
run them through developer -> tester -> reviewer, merge what passes all gates, and update .mysquad/STATUS.md. \
Stop after one batch." \
    --agent lead \
    --permission-mode acceptEdits \
    --max-turns 150 \
    | tee -a .mysquad/log/squad.log
  RC=$?

  OQ="$(open_questions)"; N_NEW="$(count_lines "$OQ")"
  if [ "$N_NEW" -gt "$N_OLD" ]; then
    notify "Squad round $((i+1)): $((N_NEW-N_OLD)) new open question(s), $N_NEW in total. Answer by appending '| ANSWER: ...' in .mysquad/STATUS.md. $(printf '%s' "$OQ" | tail -n $((N_NEW-N_OLD)) | cut -c1-200 | tr '\n' ' ')"
  fi
  N_OLD="$N_NEW"
  AFTER="$(git rev-parse HEAD)$(md5sum .mysquad/STATUS.md .mysquad/PLAN.md | md5sum)"

  if [ "$BEFORE" != "$AFTER" ]; then
    i=$((i+1)); STALE=0; FAILS=0
  elif [ "$RC" -ne 0 ]; then
    # Failed without progress: usually a usage/rate limit. Back off, don't burn a round.
    FAILS=$((FAILS+1))
    [ "$FAILS" -ge "$MAX_FAILS" ] && finish "Squad stopped: $FAILS failed rounds in a row (last exit $RC). See .mysquad/log/squad.log." 4
    WAIT=$((5 * 2 ** (FAILS-1))); [ "$WAIT" -gt 60 ] && WAIT=60
    echo "Round failed (exit $RC) without progress. Retry $FAILS/$MAX_FAILS in ${WAIT}m."
    wait_min "$WAIT"
  else
    i=$((i+1)); STALE=$((STALE+1)); FAILS=0
    if [ "$STALE" -ge 2 ]; then
      finish "Squad stopped: no progress for 2 rounds. Open questions: $N_OLD. See .mysquad/STATUS.md." 2
    fi
  fi
done

finish "Squad reached max rounds ($MAX). Open questions: $N_OLD. See .mysquad/STATUS.md." 1
