# Decisions

Shared decisions that bind the whole squad. One line each: `D-n | date | decision | reason`.

D-1 | 2026-10-19 | Single team (<=20) per installation, no multi-tenancy | Small trusted team; keeps model simple
D-2 | 2026-10-19 | ASP.NET Core API hosts the Blazor WASM client in one container, same origin | No CORS, cookie auth possible
D-3 | 2026-10-19 | PostgreSQL (postgres:17) via EF Core/Npgsql, migrations applied at startup | User choice
D-4 | 2026-10-19 | First-start admin setup page guarded by a one-time token printed to the log | Prevents admin takeover of a fresh internet-reachable instance
D-5 | 2026-10-19 | Admin creates members; must-change-password at first login; admin resets password/2FA; no email | No SMTP; trusted team
D-6 | 2026-10-19 | ASP.NET Core Identity + cookie auth, optional TOTP 2FA with QR (server-generated) and recovery codes | Built-in, well tested
D-7 | 2026-10-19 | Events: title, start, end, all-day, location, description; no recurrence; all members edit all; created/changed-by tracked | Minimal v1, trusted team
D-8 | 2026-10-19 | Notes = day notes plus event description; no pinboard | Minimal v1
D-9 | 2026-10-19 | Views: month view + upcoming 14 days only | Minimal v1
D-10 | 2026-10-19 | Discord on event create/update/delete plus one reminder N min before start (default 30), re-armed on start change, missed reminders sent only before start | User request
D-11 | 2026-10-19 | Discord settings (URL, enabled, reminder minutes, language, test button) in Postgres, admin-only page | User choice: config in Postgres
D-12 | 2026-10-19 | Webhook URL encrypted with Data Protection; DP keys stored in Postgres | Secret at rest; keeps cookies valid across restarts
D-13 | 2026-10-19 | Store UTC; display and Discord use instance TZ env var | Single-team instance
D-14 | 2026-10-19 | UI: MudBlazor; squad decides month-grid approach | User choice
D-15 | 2026-10-19 | UI languages DE/EN via .resx + IStringLocalizer; per-user preference, browser fallback, non-German -> English; switch reloads app | User choice
D-16 | 2026-10-19 | Discord message language is an instance setting (DE/EN, default EN) | Shared channel
D-17 | 2026-10-19 | Container serves HTTP on 8080 behind user's reverse proxy, forwarded headers on; Dockerfile + docker-compose.yml; .env.example only | Self-hosting with existing proxy
D-18 | 2026-10-19 | Non-goals: recurrence, multi-calendar/team, email, iCal, attachments, extra roles, SSO, mobile, week/day views, other languages | Keep v1 small
D-19 | 2026-10-19 | Gates: dotnet build -warnaserror; dotnet test; dotnet format --verify-no-changes; docker build . | Exact checks for every task
D-20 | 2026-10-19 | Tests: xUnit + WebApplicationFactory + Testcontainers.PostgreSql at API seams; no UI tests | Real DB dialect; UI stays thin
D-21 | 2026-10-19 | Squad may add well-maintained free OSS packages; if unsure -> OPEN QUESTION | User choice
D-22 | 2026-10-19 | Data model changes beyond SPEC -> OPEN QUESTION | Protect scope
D-23 | 2026-10-19 | May push dev and task branches; never push to or touch main, never force-push, never publish images, commit secrets/webhook URLs, or compose up against real data | Guardrails
D-24 | 2026-10-19 | Run budget: max 10 rounds, max 2 parallel developers | User choice
D-25 | 2026-10-19 | Every controller endpoint requires authentication via a global `AuthorizeFilter`; anonymous endpoints opt out with `[AllowAnonymous]`; security stamp validated on every request (ValidationInterval = 0), so stamp-changing calls must `RefreshSignInAsync` | Secure by default; deleted or reset users lose their sessions immediately
D-26 | 2026-10-19 | Client localization uses `ResourcesPath = "Resources"`; a resx path mirrors its type's namespace below the project root (`Client/Resources/Pages/X.resx` for `Pages/X.razor`); only `X.resx` + `X.de.resx` | One lookup rule for all UI tasks; parity test globs it
D-27 | 2026-10-19 | Run budget raised to 20 rounds (still max 2 parallel developers); all 24 tasks stay in scope | Human answer to Q-2; supersedes the round count in D-24
D-28 | 2026-10-19 | Squad may run `dotnet *`, `docker build*`, `docker info*`, `docker compose*config*` and `rtk *`; extra deny rules guard pushes to main/master, force-push and secret reads | Human answer to Q-3 (settings.json updated); resolves B-1
D-29 | 2026-10-19 | The SDK version lives in two places: global.json (`rollForward: disable`) and the Dockerfile build tag `mcr.microsoft.com/dotnet/sdk:<same version>`; bump both in one commit | A floating SDK tag breaks `docker build .` against the pinned global.json
D-30 | 2026-10-19 | Supersedes D-29: global.json sets the minimum SDK (`version` 10.0.112, `rollForward: latestFeature`); the Dockerfile build stage uses the floating `mcr.microsoft.com/dotnet/sdk:10.0` tag. Raise the floor when the local SDK moves; never set rollForward to disable while the build image floats | MCR has no image for the distro's 1xx SDK (B-3, T-002 correction C-3); a floor keeps local and container builds under one global.json rule
