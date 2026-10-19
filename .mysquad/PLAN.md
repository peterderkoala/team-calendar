# PLAN

Goal: A minimal self-hosted team calendar for one small trusted team: shared events and day notes in a month view and a 14-day upcoming list. Built on .NET 10 with an ASP.NET Core API hosting a Blazor WebAssembly client (MudBlazor, DE/EN), PostgreSQL, run via Docker Compose behind the user's reverse proxy. Members log in with ASP.NET Core Identity and optional TOTP 2FA; the admin is created on first start with a logged one-time token and then manages members. Event changes and upcoming starts are announced through a Discord webhook configured by the admin in the app.
Source of truth: .mysquad/SPEC.md

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
