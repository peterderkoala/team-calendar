# SPEC: team-calendar v1

## Problem
A small trusted team needs one self-hosted place to share events and day notes, and to be pinged in Discord when the calendar changes or an event is about to start.

## Users
One team of up to ~20 members per installation. Two roles: **admin** (user management, settings) and **member**. No multi-tenancy.

## v1 Scope
- **First-start setup**: while no user exists, the app logs a one-time setup token at startup; the setup page creates the admin only with that token. After that, setup is closed for good.
- **Accounts**: admin creates members with an initial password; the member must change it at first login. Admin can reset a member's password and 2FA, and delete members. No self-registration, no email.
- **Auth**: ASP.NET Core Identity, cookie auth (same origin). Optional per-user TOTP 2FA (authenticator app, QR code, recovery codes). Identity defaults for lockout.
- **Events**: title, start, end, all-day flag, optional location, optional description (the event's note). No recurrence. Every member can create, edit and delete any event; created-by and last-changed-by are stored and shown.
- **Day notes**: free text attached to a calendar date, editable by every member, shown in the month cell.
- **Views**: month view (events and day notes per day, click opens a dialog) and an "upcoming" list of the next 14 days.
- **Discord**: admin settings page stored in Postgres: webhook URL (encrypted with Data Protection, shown masked), enabled switch, reminder minutes (default 30), message language (DE/EN, default EN), "send test message" button. Messages on event create, update, delete, and one reminder per event N minutes before start. Changing an event's start re-arms its reminder. A reminder missed during downtime is sent only if the event has not started yet. No webhook URL or disabled = nothing is sent.
- **Localization**: UI in English and German. Language per user profile; before login or when unset, the browser language (anything other than German falls back to English). Changing language reloads the app. Dates follow the culture.
- **Time**: stored in UTC; display and Discord messages use the instance time zone from `TZ`.
- **Hosting**: one app container (ASP.NET API hosting the Blazor WASM client, HTTP on 8080, forwarded headers honoured) plus `postgres:17` with a named volume, via `docker-compose.yml`. EF Core migrations applied at startup. Secrets from `.env`; only `.env.example` is committed.

## Non-goals
Recurrence, multiple calendars or teams, email, iCal export/import/sync, file attachments, roles beyond admin/member, SSO/OAuth, mobile app, week/day views, standalone pinboard notes, languages beyond DE/EN, TLS termination in the container.

## Stack and constraints
.NET 10 (SDK 10.0.112), ASP.NET Core API + Blazor WebAssembly (hosted by the API), MudBlazor, EF Core with Npgsql, ASP.NET Core Identity, Data Protection keys in Postgres, `.resx` + `IStringLocalizer`. Tests: xUnit, `Microsoft.AspNetCore.Mvc.Testing`, Testcontainers.PostgreSql. Docker + Compose. Month grid: squad decides (own MudBlazor grid or a package).

## Gates (run from repo root; Docker must be running)
1. `dotnet build -warnaserror`
2. `dotnet test`
3. `dotnet format --verify-no-changes`
4. `docker build .`

## Acceptance (each checked by an API test against Testcontainers Postgres unless noted)
- **Setup**: with zero users, `POST /api/setup` with the logged token creates an admin; with a wrong token it returns 403; once any user exists it returns 404/409 and creates nothing. The token appears in the startup log (test reads the logger).
- **Login**: valid credentials return an auth cookie; wrong password returns 401; repeated failures trigger lockout. A member with `MustChangePassword` gets 403 on every API call except change-password until changed.
- **2FA**: enabling requires a valid TOTP code for the generated key; afterwards login without a code does not grant access, login with a valid code or an unused recovery code does; a used recovery code fails the second time. Admin reset disables 2FA for that user.
- **Users**: only admin can create, list, delete members and reset passwords/2FA; a member gets 403 on these endpoints.
- **Events**: authenticated CRUD round-trips all fields; end before start returns 400; unauthenticated requests return 401; created-by/changed-by are set from the caller; `GET` by date range returns only overlapping events.
- **Day notes**: authenticated create/update/delete per date; `GET` by date range returns them.
- **Upcoming**: returns events starting within the next 14 days, ordered by start.
- **Discord settings**: only admin can read/write; the stored URL is not plaintext in the database; reads return it masked; the test button posts one message to the configured URL (fake HTTP handler).
- **Discord messages**: create/update/delete of an event each post exactly one message in the configured language; nothing is posted when disabled or no URL is set.
- **Reminders**: one reminder tick (invoked directly, injected clock) posts a reminder once for events starting within N minutes; a second tick does not repeat it; moving the start re-arms it; events already started are skipped.
- **Localization**: a user's language preference persists via the profile endpoint; resource files for `de` and `en` contain the same keys (unit test).
- **Hosting**: `docker build .` succeeds; the app applies pending migrations at startup (integration test against an empty Testcontainers database).

## Seams
- HTTP API under `/api/*` via `WebApplicationFactory`: `setup`, `auth` (login, logout, change-password, 2fa), `users`, `events`, `notes`, `upcoming`, `profile`, `settings/discord`.
- Discord sender behind an `HttpClient` whose handler tests replace.
- Reminder logic as a single tick method on the background service, with an injectable `TimeProvider`.
- Blazor UI stays thin: it only calls the API; no UI tests in v1.

## Autonomy rules
- **Squad decides alone**: naming, project layout, UI layout, month-grid approach, well-maintained free open-source packages.
- **OPEN QUESTION in `.mysquad/STATUS.md`** (then continue with other tasks): unsure about a package, data model changes beyond this spec, anything contradicting this spec.
- **Needs an architect design note first**: auth and 2FA, setup token, Data Protection and webhook encryption, reminder service, localization setup.
- **Never**: push to or touch `main`, force-push, publish Docker images, commit secrets or real webhook URLs, run `docker compose up` against real data.
- **Budget**: max 10 loop rounds, max 2 developers in parallel.
