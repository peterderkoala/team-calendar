# PLAN

Goal: <describe the product or feature in 3-5 sentences>

Tasks are written by the product-owner subagent. Format:

### T-001 Example: project skeleton and test setup
- status: todo
- needs-design: no
- depends_on: -
- files: pyproject.toml, src/__init__.py, tests/test_smoke.py
- seams: the test runner entry point
- acceptance:
  - `pytest` runs and passes with one smoke test
  - linter config exists and passes
