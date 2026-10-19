# PLAN

Goal: A minimal self-hosted team calendar for one small trusted team: shared events and day notes in a month view and a 14-day upcoming list. Built on .NET 10 with an ASP.NET Core API hosting a Blazor WebAssembly client (MudBlazor, DE/EN), PostgreSQL, run via Docker Compose behind the user's reverse proxy. Members log in with ASP.NET Core Identity and optional TOTP 2FA; the admin is created on first start with a logged one-time token and then manages members. Event changes and upcoming starts are announced through a Discord webhook configured by the admin in the app.
Source of truth: .mysquad/SPEC.md

Tasks are written by the product-owner subagent. Format:

### T-nnn Short title
- status: todo
- needs-design: yes|no
- depends_on: T-xxx
- files: concrete paths
- seams: public interfaces tests hit
- acceptance:
  - observable, testable criterion

Conventions for all tasks (fixed here so tasks stay disjoint):
- Layout: `TeamCalendar.slnx`; `src/TeamCalendar.Api` (ASP.NET Core, MVC controllers auto-discovered via `MapControllers()`, so a new controller does not touch `Program.cs`); `src/TeamCalendar.Client` (Blazor WASM, hosted by Api); `src/TeamCalendar.Shared` (request/response DTOs, one file per feature); `tests/TeamCalendar.Tests` (xUnit, `WebApplicationFactory<Program>`, Testcontainers.PostgreSql).
- Paths below abbreviate: `Api/` = `src/TeamCalendar.Api/`, `Client/` = `src/TeamCalendar.Client/`, `Shared/` = `src/TeamCalendar.Shared/`, `Tests/` = `tests/TeamCalendar.Tests/`.
- Migrations live in `Api/Data/Migrations/`. Every migration touches `AppDbContextModelSnapshot.cs`, so migration tasks never run in parallel. A migration counts as one file (migration + designer + snapshot).
- A `.resx` pair (`X.resx` + `X.de.resx`) counts as one file.
- Only role is `admin`; a member is a user without a role.
- Every task passes all four gates (D-19). Exception: `docker build .` passes from T-002 on.
- UI tasks have no UI tests (D-20). Their acceptance: gates pass and the page calls the named endpoint(s).

### T-001 Solution skeleton, hosted Blazor WASM, Postgres, migrations at startup, test harness
- status: todo
- needs-design: no
- depends_on: -
- files: global.json, TeamCalendar.slnx, Directory.Build.props, .editorconfig, CLAUDE.md, Api/TeamCalendar.Api.csproj, Api/Program.cs, Api/appsettings.json, Api/Data/AppDbContext.cs, Api/Data/Migrations/*_Initial (migration), Client/TeamCalendar.Client.csproj, Client/Program.cs, Client/App.razor, Client/_Imports.razor, Client/Layout/MainLayout.razor, Client/Pages/Home.razor, Client/wwwroot/index.html, Shared/TeamCalendar.Shared.csproj, Tests/TeamCalendar.Tests.csproj, Tests/Infrastructure/ApiFactory.cs, Tests/HostingTests.cs
- note: scaffolding exception to the ~5 files rule (mostly template output). Riskiest assumption: .NET 10 SDK 10.0.112 + hosted WASM + MudBlazor + Npgsql + Testcontainers build and test together.
- seams: GET / (client fallback), GET /api/* (unknown route), app startup against `ConnectionStrings:Default`
- acceptance:
  - `dotnet build -warnaserror`, `dotnet test`, `dotnet format --verify-no-changes` pass from repo root
  - global.json pins SDK 10.0.112; Client references MudBlazor; Api references Client and serves it (`UseBlazorFrameworkFiles`, `MapFallbackToFile("index.html")`)
  - test: GET /some/client/route returns 200 with the client index.html
  - test: GET /api/does-not-exist returns 404 (the client fallback does not swallow /api)
  - test: starting the app against a fresh, empty Testcontainers database applies all migrations (`GetPendingMigrationsAsync()` is empty, `__EFMigrationsHistory` contains `Initial`)
  - ApiFactory can start an app instance on a fresh, uniquely named database (later tests that need zero users rely on this)
  - Program.cs enables forwarded headers (X-Forwarded-For, X-Forwarded-Proto; known networks/proxies cleared) and registers `AddControllers`/`MapControllers`
  - Tests csproj references xUnit, Microsoft.AspNetCore.Mvc.Testing, Testcontainers.PostgreSql, Microsoft.Extensions.TimeProvider.Testing
  - CLAUDE.md lists the build, test, format and docker commands and the project layout

### T-002 Dockerfile, docker-compose.yml, .env.example
- status: todo
- needs-design: no
- depends_on: T-001
- files: Dockerfile, .dockerignore, docker-compose.yml, .env.example
- seams: `docker build .`, `docker compose config`
- acceptance:
  - `docker build .` succeeds from repo root (multi-stage: sdk 10.0 build/publish, aspnet 10.0 runtime, non-root, HTTP on 8080)
  - `docker compose --env-file .env.example config` exits 0 and shows service `app` (built from `.`, port 8080) and service `db` with image `postgres:17` and a named volume for its data
  - app service gets `ConnectionStrings__Default` and `TZ` from the env file; db service gets `POSTGRES_PASSWORD` from the env file
  - `.env.example` contains only placeholder values; `git check-ignore .env` reports `.env` as ignored

### T-003 Identity store, admin role and Data Protection keys in Postgres
- status: todo
- needs-design: yes
- depends_on: T-001
- files: Api/TeamCalendar.Api.csproj, Api/Program.cs, Api/Data/AppUser.cs, Api/Auth/Roles.cs, Api/Data/AppDbContext.cs, Api/Data/Migrations/*_Identity (migration), Tests/IdentityStoreTests.cs
- seams: `UserManager<AppUser>`, `IDataProtectionProvider` resolved from the app's service provider
- acceptance:
  - `AppUser : IdentityUser` with `MustChangePassword` (bool) and `Language` (nullable string); `AppDbContext` is an `IdentityDbContext<AppUser>` and implements `IDataProtectionKeyContext`
  - Identity is registered with roles, Identity default lockout options and cookie auth; for `/api/*` an unauthenticated request yields 401 and a forbidden one 403 (no redirects)
  - test: a user created via `UserManager` round-trips `MustChangePassword` and `Language`
  - test: after protecting a payload, the `DataProtectionKeys` table has at least one row, and a second app instance on the same database unprotects the payload
  - migration applies on an empty database (T-001 hosting test stays green)

### T-004 Login, logout, me and must-change-password gate
- status: todo
- needs-design: yes
- depends_on: T-003
- files: Api/Controllers/AuthController.cs, Api/Auth/MustChangePasswordMiddleware.cs, Api/Program.cs, Shared/AuthDtos.cs, Tests/Infrastructure/TestUsers.cs, Tests/AuthTests.cs
- seams: POST /api/auth/login, POST /api/auth/logout, GET /api/auth/me, POST /api/auth/change-password
- acceptance:
  - login with valid credentials returns 200, sets the auth cookie and returns user name, isAdmin and mustChangePassword
  - login with a wrong password or unknown user returns 401
  - after 5 failed logins (Identity default) the correct password also returns 401 and the user is locked out
  - GET /api/auth/me returns 401 without cookie and 200 with cookie; after logout it returns 401
  - login with header `X-Forwarded-Proto: https` sets a cookie with the `secure` attribute
  - a user with `MustChangePassword` gets 403 on GET /api/auth/me and every other authenticated /api endpoint except POST /api/auth/change-password
  - change-password with the correct current password and a valid new one returns 204, clears `MustChangePassword`, and GET /api/auth/me then returns 200; wrong current password or a password failing Identity validators returns 400
  - TestUsers helper creates a user (optionally admin, optionally must-change) and returns a logged-in HttpClient

### T-005 Calendar data model: events, day notes, Discord settings
- status: todo
- needs-design: no
- depends_on: T-003
- files: Api/Data/CalendarEvent.cs, Api/Data/DayNote.cs, Api/Data/DiscordSettings.cs, Api/Data/AppDbContext.cs, Api/Data/Migrations/*_Calendar (migration), Tests/DataModelTests.cs
- seams: `AppDbContext` against Testcontainers Postgres
- acceptance:
  - `CalendarEvent`: Id (Guid), Title, StartUtc, EndUtc, AllDay, Location?, Description?, CreatedBy, ChangedBy (user names as strings, no FK, so deleting a member keeps events), ReminderSentForStartUtc? (start time the reminder was sent for; differs from StartUtc = re-armed)
  - `DayNote`: Date (DateOnly, primary key), Text
  - `DiscordSettings`: single row (Id = 1), ProtectedWebhookUrl?, Enabled (default false), ReminderMinutes (default 30), Language (default "en")
  - test: each entity round-trips all fields through a fresh DbContext; DateTime values come back as UTC
  - migration applies on an empty database

### T-006 First-start setup token and admin creation
- status: todo
- needs-design: yes
- depends_on: T-003
- files: Api/Setup/SetupToken.cs, Api/Controllers/SetupController.cs, Api/Program.cs, Shared/SetupDtos.cs, Tests/Infrastructure/CapturingLoggerProvider.cs, Tests/SetupTests.cs
- seams: GET /api/setup, POST /api/setup, startup log
- acceptance:
  - with zero users, startup logs a one-time setup token; the test reads it from a capturing logger provider
  - GET /api/setup returns `{ required: true }` with zero users and `{ required: false }` otherwise
  - with zero users, POST /api/setup with the logged token, user name and password returns 201 and creates a user in role `admin` with `MustChangePassword = false`
  - with a wrong token it returns 403 and creates nothing
  - once any user exists it returns 409 and creates nothing, even with the correct token
  - token comparison is constant-time

### T-007 Users admin API
- status: todo
- needs-design: no
- depends_on: T-004
- files: Api/Controllers/UsersController.cs, Shared/UserDtos.cs, Tests/UsersTests.cs
- seams: GET /api/users, POST /api/users, DELETE /api/users/{id}, POST /api/users/{id}/reset-password, POST /api/users/{id}/reset-2fa
- acceptance:
  - admin: GET lists users (id, userName, isAdmin, twoFactorEnabled, mustChangePassword)
  - admin: POST with userName and initial password returns 201; the new user has no role and `MustChangePassword = true`; duplicate name or invalid password returns 400
  - admin: DELETE returns 204 and the user is gone; deleting oneself returns 400
  - admin: reset-password returns 204, the new password works and `MustChangePassword` is true again
  - admin: reset-2fa on a user with 2FA enabled returns 204; afterwards `TwoFactorEnabled` is false and the authenticator key is reset
  - a member gets 403 on all five endpoints; unauthenticated gets 401

### T-008 TOTP two-factor authentication
- status: todo
- needs-design: yes
- depends_on: T-004
- files: Api/Controllers/AuthController.cs, Api/Controllers/TwoFactorController.cs, Api/TeamCalendar.Api.csproj, Shared/TwoFactorDtos.cs, Tests/TeamCalendar.Tests.csproj, Tests/TwoFactorTests.cs
- note: adds QRCoder (server-side QR as SVG, D-6) to Api and Otp.NET (TOTP codes in tests) to Tests.
- seams: GET /api/auth/2fa, POST /api/auth/2fa/enable, POST /api/auth/2fa/disable, POST /api/auth/login (with twoFactorCode or recoveryCode)
- acceptance:
  - GET /api/auth/2fa returns enabled flag, shared key, otpauth URI and QR code SVG for the caller
  - enable with an invalid code returns 400 and 2FA stays off; with a valid TOTP for the generated key returns 200 with 10 recovery codes
  - with 2FA on, login with only password returns 401 with `requiresTwoFactor: true` and GET /api/auth/me stays 401
  - login with password plus valid TOTP returns 200; with password plus an unused recovery code returns 200; the same recovery code a second time returns 401
  - disable returns 204 and password-only login works again

### T-009 Events API and instance time zone
- status: todo
- needs-design: no
- depends_on: T-004, T-005
- files: Api/Controllers/EventsController.cs, Api/Time/InstanceTimeZone.cs, Api/Program.cs, Shared/EventDtos.cs, Tests/EventsTests.cs
- seams: GET /api/events?from=&to=, GET /api/events/{id}, POST /api/events, PUT /api/events/{id}, DELETE /api/events/{id}
- acceptance:
  - POST then GET round-trips title, start, end, allDay, location, description; returns createdBy and changedBy
  - createdBy is the caller's user name; after PUT by another user changedBy is that user and createdBy is unchanged
  - end before start returns 400; missing title or title > 200 chars returns 400; location > 200 or description > 4000 chars returns 400
  - unauthenticated requests to every events endpoint return 401; unknown id returns 404; DELETE returns 204
  - GET by range returns exactly the events overlapping [from, to) (start < to and end > from)
  - times are stored in UTC; responses are `DateTimeOffset` in the instance zone from config key `TZ` (unset = UTC): with `TZ=Europe/Berlin` an event at 2026-07-01T10:00Z is returned as 12:00+02:00
  - `TimeProvider.System` and the instance `TimeZoneInfo` are registered in DI

### T-010 Day notes API
- status: todo
- needs-design: no
- depends_on: T-004, T-005
- files: Api/Controllers/NotesController.cs, Shared/NoteDtos.cs, Tests/NotesTests.cs
- seams: GET /api/notes?from=&to=, PUT /api/notes/{date}, DELETE /api/notes/{date}
- acceptance:
  - PUT /api/notes/2026-11-03 with text creates the note (200); a second PUT updates it
  - GET by inclusive date range returns only notes in range
  - DELETE returns 204 and the note is gone; DELETE of a missing note returns 404
  - empty text or text > 4000 chars returns 400; invalid date returns 400 or 404
  - unauthenticated requests return 401

### T-011 Upcoming API
- status: todo
- needs-design: no
- depends_on: T-009
- files: Api/Controllers/UpcomingController.cs, Tests/UpcomingTests.cs
- seams: GET /api/upcoming
- acceptance:
  - with a FakeTimeProvider, returns events whose start is in [now, now + 14 days), ordered by start
  - events starting before now or at/after now + 14 days are excluded
  - unauthenticated returns 401

### T-012 Discord settings, encrypted webhook URL, test message
- status: todo
- needs-design: yes
- depends_on: T-004, T-005
- files: Api/Controllers/DiscordSettingsController.cs, Api/Discord/DiscordClient.cs, Api/Program.cs, Shared/DiscordSettingsDtos.cs, Tests/Infrastructure/FakeHttpHandler.cs, Tests/DiscordSettingsTests.cs
- seams: GET /api/settings/discord, PUT /api/settings/discord, POST /api/settings/discord/test, `DiscordClient` typed HttpClient (handler replaced in tests)
- acceptance:
  - GET returns enabled (default false), reminderMinutes (default 30), language (default "en") and the webhook URL masked (never the full URL)
  - PUT stores the settings; the `ProtectedWebhookUrl` column does not contain the plaintext URL; omitting webhookUrl keeps the stored one, empty string clears it
  - a webhook URL that is not an absolute https URL returns 400; reminderMinutes outside 1..1440 returns 400; language other than "en"/"de" returns 400
  - POST test with a configured URL sends exactly one POST to that URL (fake handler) and returns 204; without a URL returns 400 and sends nothing
  - a member gets 403 and unauthenticated gets 401 on all three endpoints

### T-013 Discord messages on event create, update, delete
- status: todo
- needs-design: no
- depends_on: T-009, T-012
- files: Api/Discord/DiscordNotifier.cs, Api/Resources/DiscordMessages.resx (+ .de.resx pair), Api/Controllers/EventsController.cs, Api/Program.cs, Tests/DiscordMessagesTests.cs
- seams: POST/PUT/DELETE /api/events, fake handler on `DiscordClient`
- acceptance:
  - with Discord enabled and a URL set, each of create, update, delete posts exactly one message containing the event title and the start time in the instance zone
  - the message uses the configured language: with "de" the text matches the `DiscordMessages.de.resx` template, with "en" the base template
  - when disabled or no URL is set, nothing is posted
  - when the webhook returns 500 or times out, the event request still succeeds (201/200/204)

### T-014 Reminder service
- status: todo
- needs-design: yes
- depends_on: T-013
- files: Api/Discord/ReminderService.cs, Api/Discord/DiscordNotifier.cs, Api/Resources/DiscordMessages.resx (+ .de.resx pair), Api/Program.cs, Tests/ReminderTests.cs
- seams: `ReminderService.TickAsync(CancellationToken)` resolved from DI, injected FakeTimeProvider, fake handler on `DiscordClient`
- acceptance:
  - one tick posts one reminder for each event starting within the next ReminderMinutes and not yet reminded for its current start
  - a second tick at the same or a later time posts nothing more for those events
  - after moving an event's start via PUT /api/events/{id}, a tick inside the new window posts the reminder again
  - events whose start is at or before now are skipped (a reminder missed during downtime is sent only if the event has not started)
  - nothing is posted when Discord is disabled or no URL is set
  - the hosted service calls `TickAsync` once per minute via the injected `TimeProvider`

### T-015 Profile language API
- status: todo
- needs-design: yes
- depends_on: T-004
- files: Api/Controllers/ProfileController.cs, Shared/ProfileDtos.cs, Tests/ProfileTests.cs
- seams: GET /api/profile, PUT /api/profile
- acceptance:
  - GET returns userName and language (null when unset)
  - PUT with language "de" or "en" returns 204 and the value persists across a new login; null resets to unset
  - any other language value returns 400
  - unauthenticated returns 401

### T-016 Client localization setup and resource parity test
- status: todo
- needs-design: yes
- depends_on: T-015
- files: Client/TeamCalendar.Client.csproj, Client/Program.cs, Client/SharedResource.cs, Client/Resources/SharedResource.resx (+ .de.resx pair), Shared/CultureChoice.cs, Tests/LocalizationTests.cs
- seams: `CultureChoice.Resolve(profileLanguage, browserCulture)`, resource files on disk
- acceptance:
  - unit test: profile "de" -> de; profile "en" -> en; unset and browser de-AT -> de; unset and browser fr-FR -> en; unset and browser en-US -> en
  - unit test: every `*.resx` under `src/` has a `*.de.resx` partner with exactly the same keys, and vice versa
  - Client loads all globalization data, calls GET /api/profile at startup (401/403 = unset), sets default culture and UI culture from `CultureChoice.Resolve` before the app renders, and registers `AddLocalization`

### T-017 Client shell: session, layout, login, change password
- status: todo
- needs-design: no
- depends_on: T-004, T-006, T-016
- files: Client/Services/Session.cs, Client/Program.cs, Client/Layout/MainLayout.razor, Client/Pages/Login.razor, Client/Pages/ChangePassword.razor, Client/Resources/SharedResource.resx (+ .de.resx pair)
- seams: GET /api/setup, POST /api/auth/login, GET /api/auth/me, POST /api/auth/logout, POST /api/auth/change-password
- acceptance:
  - gates pass; no UI tests
  - on start the shell calls GET /api/setup and navigates to /setup when required, else GET /api/auth/me and navigates to /login on 401
  - Login page posts to /api/auth/login; on `mustChangePassword` it navigates to /account/password, which posts to /api/auth/change-password
  - MainLayout has a logout action (POST /api/auth/logout) and nav links for all v1 routes: /, /upcoming, /profile, /account/2fa, plus /admin/users and /admin/discord shown only to admins
  - all new UI strings are in the SharedResource pair (parity test green)

### T-018 Setup page
- status: todo
- needs-design: no
- depends_on: T-006, T-017
- files: Client/Pages/Setup.razor, Client/Resources/Pages/Setup.resx (+ .de.resx pair)
- seams: POST /api/setup
- acceptance:
  - gates pass; no UI tests
  - /setup form (token, user name, password) posts to /api/setup and navigates to /login on 201; shows an error on 403/409

### T-019 Two-factor page and login code field
- status: todo
- needs-design: no
- depends_on: T-008, T-017
- files: Client/Pages/TwoFactor.razor, Client/Resources/Pages/TwoFactor.resx (+ .de.resx pair), Client/Pages/Login.razor, Client/Resources/SharedResource.resx (+ .de.resx pair)
- seams: GET /api/auth/2fa, POST /api/auth/2fa/enable, POST /api/auth/2fa/disable, POST /api/auth/login
- acceptance:
  - gates pass; no UI tests
  - /account/2fa shows the QR SVG and shared key from GET /api/auth/2fa, enables via POST .../enable and shows the returned recovery codes, disables via POST .../disable
  - Login shows a code field (TOTP or recovery code) when login returns `requiresTwoFactor` and re-posts with it

### T-020 Users admin page
- status: todo
- needs-design: no
- depends_on: T-007, T-017
- files: Client/Pages/Users.razor, Client/Resources/Pages/Users.resx (+ .de.resx pair)
- seams: GET/POST /api/users, DELETE /api/users/{id}, POST /api/users/{id}/reset-password, POST /api/users/{id}/reset-2fa
- acceptance:
  - gates pass; no UI tests
  - /admin/users lists users and calls each of the five endpoints from its actions

### T-021 Month view with event and day-note dialogs
- status: todo
- needs-design: no
- depends_on: T-009, T-010, T-017
- files: Client/Pages/Month.razor, Client/Components/EventDialog.razor, Client/Components/NoteDialog.razor, Client/Pages/Home.razor (deleted), Client/Resources/Pages/Month.resx (+ .de.resx pair)
- note: own month grid built from MudBlazor components (7 columns, weeks start per culture), no extra package.
- seams: GET /api/events?from=&to=, POST/PUT/DELETE /api/events, GET /api/notes?from=&to=, PUT/DELETE /api/notes/{date}
- acceptance:
  - gates pass; no UI tests
  - / shows the month grid; on month change it calls GET /api/events and GET /api/notes for the visible range
  - clicking an event opens EventDialog (shows createdBy/changedBy; edit via PUT, delete via DELETE); clicking an empty day opens create (POST)
  - day notes appear in the cell; NoteDialog saves via PUT and deletes via DELETE /api/notes/{date}
  - dates and times are formatted with the current culture; Home.razor is removed

### T-022 Upcoming page
- status: todo
- needs-design: no
- depends_on: T-011, T-017
- files: Client/Pages/Upcoming.razor, Client/Resources/Pages/Upcoming.resx (+ .de.resx pair)
- seams: GET /api/upcoming
- acceptance:
  - gates pass; no UI tests
  - /upcoming calls GET /api/upcoming and lists the events in returned order with culture-formatted dates

### T-023 Discord settings page
- status: todo
- needs-design: no
- depends_on: T-012, T-017
- files: Client/Pages/DiscordSettings.razor, Client/Resources/Pages/DiscordSettings.resx (+ .de.resx pair)
- seams: GET/PUT /api/settings/discord, POST /api/settings/discord/test
- acceptance:
  - gates pass; no UI tests
  - /admin/discord loads via GET, shows the masked URL, saves via PUT (URL field left empty = keep), and the test button calls POST .../test

### T-024 Profile page with language switch
- status: todo
- needs-design: no
- depends_on: T-015, T-017
- files: Client/Pages/Profile.razor, Client/Resources/Pages/Profile.resx (+ .de.resx pair)
- seams: GET /api/profile, PUT /api/profile
- acceptance:
  - gates pass; no UI tests
  - /profile loads via GET /api/profile, saves the language via PUT /api/profile, then reloads the app (`NavigateTo(..., forceLoad: true)`)
