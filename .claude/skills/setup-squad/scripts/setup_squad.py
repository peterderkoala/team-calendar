#!/usr/bin/env python3
"""Doctor and idempotent installer for the squad structure.

  setup_squad.py --check            report only, write nothing
  setup_squad.py --apply            report, then create/merge the squad files
  setup_squad.py --upgrade          like --apply, but also overwrite squad-owned files that differ
                                    (agents, squad skills, ralph.sh); squad state (.mysquad/*.md) is never touched
  --repo PATH                       target repo (default: current directory)
"""
import argparse, json, os, shutil, stat, subprocess, sys
from pathlib import Path

HERE = Path(__file__).resolve().parent.parent
TEMPLATES = HERE / "templates"
START, END = "<!-- squad:start -->", "<!-- squad:end -->"
SPECIAL = {"CLAUDE.squad.md", "gitignore.squad"}


def is_state(rel):
    """Squad state the team writes; templates only seed it."""
    return rel.parts[0] == ".mysquad" and rel.suffix == ".md"


def run(cmd, cwd):
    try:
        r = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True, timeout=20)
        return r.returncode, (r.stdout + r.stderr).strip()
    except Exception as e:  # missing binary, timeout
        return 127, str(e)


def have(cmd):
    return shutil.which(cmd) is not None


def doctor(repo):
    rows = []  # (status, name, detail)

    def add(ok, name, detail="", hint=""):
        rows.append(("ok" if ok else "MISSING", name, detail if ok else (hint or detail)))

    rc, _ = run(["git", "rev-parse", "--git-dir"], repo)
    add(rc == 0, "git repository", "", "run: git init")
    if rc == 0:
        _, br = run(["git", "symbolic-ref", "--short", "HEAD"], repo)
        rows.append(("info", "git branch", br + (" (ralph.sh will switch to squad/work)" if br in ("main", "master") else "")))
        rc2, _ = run(["git", "rev-parse", "HEAD"], repo)
        add(rc2 == 0, "initial commit", "", "make one (worktrees and ralph.sh need it)")
    _, gv = run(["git", "--version"], repo)
    add(have("git"), "git (worktrees)", gv, "install git >= 2.5")
    _, cv = run(["claude", "--version"], repo) if have("claude") else (1, "")
    add(have("claude"), "claude CLI", cv, "install Claude Code")
    add(have("node"), "node (ponytail hooks)", run(["node", "--version"], repo)[1] if have("node") else "", "install Node.js")
    add(have("python3"), "python3", "", "install Python 3")
    add(have("uvx") or have("uv"), "uvx (MCP server launcher)", "", "install uv: pip install uv")
    add(have("code-review-graph"), "code-review-graph", "", "pip install code-review-graph")
    add((repo / ".claude/skills/review-changes").is_dir(), "graph skills in .claude/skills",
        "", "run once: code-review-graph install --platform claude-code")
    add((repo / ".code-review-graph").is_dir(), "graph built", "", "run: code-review-graph build")

    pony = False
    for base in (Path.home() / ".claude" / "plugins", repo / ".claude" / "plugins"):
        if base.is_dir() and any("ponytail" in p.name.lower() for p in base.rglob("*") if p.is_dir()):
            pony = True
            break
    add(pony, "ponytail plugin", "", "in Claude Code run: /plugin marketplace add DietrichGebert/ponytail  then  /plugin install ponytail@ponytail")

    rows.append(("ok" if os.environ.get("SQUAD_NOTIFY_CMD") else "info", "notifications (SQUAD_NOTIFY_CMD)", "set" if os.environ.get("SQUAD_NOTIFY_CMD") else "not set: open questions are only visible in .mysquad/STATUS.md; see the comment in .mysquad/ralph.sh"))

    gates = []
    if (repo / "package.json").exists():
        try:
            scripts = json.loads((repo / "package.json").read_text()).get("scripts", {})
            gates += [f"npm run {k}" for k in ("test", "lint", "build") if k in scripts]
        except Exception:
            pass
    if (repo / "pyproject.toml").exists() or (repo / "pytest.ini").exists():
        gates.append("pytest")
    if (repo / "Makefile").exists():
        gates.append("make (see Makefile)")
    rows.append(("info", "test/lint/build gates", ", ".join(gates) if gates else "none found (first task becomes T-001: set up gates)"))
    return rows


def print_rows(rows):
    for s, n, d in rows:
        print(f"[{s:^7}] {n}" + (f" - {d}" if d else ""))


def merge_json(src, dst):
    """Deep merge src into dst. Existing scalars win; lists are unioned."""
    if isinstance(src, dict) and isinstance(dst, dict):
        for k, v in src.items():
            dst[k] = merge_json(v, dst[k]) if k in dst else v
        return dst
    if isinstance(src, list) and isinstance(dst, list):
        return dst + [x for x in src if x not in dst]
    return dst


def apply(repo, upgrade=False):
    log = []
    for t in sorted(TEMPLATES.rglob("*")):
        if not t.is_file():
            continue
        rel = t.relative_to(TEMPLATES)
        if rel.name in SPECIAL:
            continue
        dst = repo / rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        if rel.suffix == ".json" and rel.name in ("settings.json", ".mcp.json"):
            new = json.loads(t.read_text())
            if dst.exists():
                old = json.loads(dst.read_text())
                merged = merge_json(new, old)
                if merged == json.loads(dst.read_text()):
                    log.append(("unchanged", rel))
                else:
                    dst.write_text(json.dumps(merged, indent=2) + "\n")
                    log.append(("merged", rel))
            else:
                dst.write_text(json.dumps(new, indent=2) + "\n")
                log.append(("created", rel))
            continue
        if dst.exists():
            if dst.read_bytes() == t.read_bytes():
                log.append(("same", rel))
                continue
            if not upgrade or is_state(rel):
                log.append(("kept (differs)", rel))
                continue
            log.append(("upgraded", rel))
        else:
            log.append(("created", rel))
        shutil.copyfile(t, dst)
        if rel.name.endswith(".sh"):
            dst.chmod(dst.stat().st_mode | stat.S_IXUSR | stat.S_IXGRP | stat.S_IXOTH)

    # CLAUDE.md: managed block between markers
    block = f"{START}\n{(TEMPLATES / 'CLAUDE.squad.md').read_text().strip()}\n{END}\n"
    cm = repo / "CLAUDE.md"
    if not cm.exists():
        cm.write_text(block)
        log.append(("created", Path("CLAUDE.md")))
    else:
        txt = cm.read_text()
        if START in txt and END in txt:
            pre, rest = txt.split(START, 1)
            _, post = rest.split(END, 1)
            new = pre + block.rstrip("\n") + post
        else:
            new = txt.rstrip("\n") + "\n\n" + block
        if new == txt:
            log.append(("unchanged", Path("CLAUDE.md (squad block)")))
        else:
            cm.write_text(new)
            log.append(("merged", Path("CLAUDE.md (squad block)")))

    # .gitignore: append missing lines
    gi = repo / ".gitignore"
    want = [l for l in (TEMPLATES / "gitignore.squad").read_text().splitlines() if l.strip()]
    have_lines = gi.read_text().splitlines() if gi.exists() else []
    missing = [l for l in want if l not in have_lines]
    if missing:
        gi.write_text(("\n".join(have_lines) + "\n" if have_lines else "") + "\n".join(missing) + "\n")
        log.append(("merged", Path(".gitignore")))
    else:
        log.append(("unchanged", Path(".gitignore")))
    return log


def verify(repo):
    problems = []
    for f in (".claude/settings.json", ".mcp.json"):
        try:
            json.loads((repo / f).read_text())
        except Exception as e:
            problems.append(f"{f}: invalid JSON ({e})")
    rc, out = run(["bash", "-n", ".mysquad/ralph.sh"], repo)
    if rc != 0:
        problems.append(f".mysquad/ralph.sh: {out}")
    for a in (repo / ".claude/agents").glob("*.md"):
        if not a.read_text().startswith("---\n"):
            problems.append(f"{a.name}: frontmatter missing")
    return problems


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--upgrade", action="store_true")
    ap.add_argument("--repo", default=".")
    a = ap.parse_args()
    repo = Path(a.repo).resolve()
    print("== Doctor ==")
    print_rows(doctor(repo))
    if (a.apply or a.upgrade) and not a.check:
        print("\n== Structure ==")
        for s, rel in apply(repo, a.upgrade):
            print(f"[{s:^15}] {rel}")
        probs = verify(repo)
        print("\n== Verify ==")
        print("ok" if not probs else "\n".join(probs))
        sys.exit(1 if probs else 0)


if __name__ == "__main__":
    main()
