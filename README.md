# Timesheet Management

Internal employee timesheet app: employees log weekly hours against Projects/Tasks, managers
approve or reject, admins manage master data and branding. Built to be portable across
customers through configuration only — see `docs/architecture-workflow.html` for the
architecture and workflow diagrams this was built from.

**Stack:** ASP.NET Core 8 Web API (layered: Controllers → Services → Repositories, EF Core
code-first) · React + TypeScript + Bootstrap 5 · SQL Server · pluggable Local/Entra ID auth.

## Solution layout

```
src/
  TimesheetManagement.Domain          entities, enums — no dependencies
  TimesheetManagement.Application     DTOs, service interfaces + business rules, repository interfaces
  TimesheetManagement.Infrastructure  EF Core DbContext/migrations, repositories, auth (Local JWT + Entra JIT)
  TimesheetManagement.Api             Controllers, Program.cs, appsettings, middleware
tests/
  TimesheetManagement.Tests           xUnit — approval workflow state-machine tests
client/                               React + TypeScript + Bootstrap 5 SPA (Vite)
docs/
  architecture-workflow.html          architecture & workflow diagrams (published as an Artifact)
```

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- A reachable SQL Server instance (local, Docker, or Azure SQL) with a login that can create tables
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

## Backend setup

1. **Configure the connection string.** `src/TimesheetManagement.Api/appsettings.Development.json`
   already has `ConnectionStrings:TimesheetDb` pointing at
   `Server=localhost;Database=TimesheetManagementDb;Trusted_Connection=True;TrustServerCertificate=True;`
   — Windows Authentication against a local instance. Change the server there if yours differs.

   If your SQL Server instance uses **SQL logins** instead (Mixed Mode auth), use a connection
   string with `User Id=...` instead of `Trusted_Connection=True`, and layer the password in via
   user-secrets instead of committing it:

   ```
   dotnet user-secrets set "Db:Password" "<your SQL login password>" --project src/TimesheetManagement.Api
   ```

   (`DependencyInjection.cs` in Infrastructure reads `Db:Password` and merges it into the
   connection string at startup — see `BuildConnectionString`. If the server is Windows-Auth-only,
   this key can simply stay unset.)

2. **Set the local JWT signing key** (never committed — see `dotnet user-secrets`), from
   `src/TimesheetManagement.Api`:

   ```
   dotnet user-secrets set "Auth:Local:SigningKey" "<any long random string>"
   ```

   (This repo's dev session already has this set for the machine it was built on. On a fresh
   clone or a new machine, run the command above first.)

3. **Run the API.** Migrations apply automatically on startup in Development (see `Program.cs`),
   including the seed data — no separate `dotnet ef database update` needed for local dev:

   ```
   dotnet run --project src/TimesheetManagement.Api
   ```

   Swagger UI opens at the HTTPS URL printed in the console (`/swagger`). To apply migrations
   without running the app (e.g. before a deploy): `dotnet ef database update --project src/TimesheetManagement.Infrastructure --startup-project src/TimesheetManagement.Api`.

4. **Seeded users** (Local auth mode — pick one from the login dropdown, no password needed):

   | Name          | Role     | Email                  |
   |---------------|----------|-------------------------|
   | Ava Admin     | Admin    | admin@timesheet.local   |
   | Mia Manager   | Manager  | manager@timesheet.local |
   | Eli Employee  | Employee | employee@timesheet.local (reports to Mia Manager) |

   Two sample Projects (Internal Tools, Client Website Revamp) with four Tasks are seeded too.

## Frontend setup

```
cd client
npm install
npm run dev
```

Opens at `http://localhost:5173`. `.env.development` is checked in with sane defaults
(`VITE_API_BASE_URL=https://localhost:7059`, `VITE_AUTH_PROVIDER=Local`) — adjust the API URL if
your `dotnet run` printed a different port.

## Demo mode (no SQL Server)

For demoing on a machine without a SQL Server instance available, the API can run against a
local SQLite file instead — same seeded users, same features, zero install beyond the .NET SDK.
`Db:Provider=Sqlite` (set in `appsettings.Demo.json`) builds the schema straight from the current
EF Core model via `EnsureCreated` rather than migrations, since this path is a demo-only fallback,
not the source of truth schema.

1. **Set the JWT signing key** (same requirement as normal dev — see step 2 of Backend setup
   above), from `src/TimesheetManagement.Api`:

   ```
   dotnet user-secrets set "Auth:Local:SigningKey" "<any long random string>"
   ```

2. **Run the API in Demo mode**, still from `src/TimesheetManagement.Api` — PowerShell:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Demo"
   dotnet run --no-launch-profile --urls "http://localhost:5080"
   ```

   or Git Bash / WSL:

   ```bash
   export ASPNETCORE_ENVIRONMENT=Demo
   dotnet run --no-launch-profile --urls "http://localhost:5080"
   ```

   `--no-launch-profile` skips `launchSettings.json`, which would otherwise force `Development`
   + HTTPS with a dev cert the machine may not have trusted yet — plain HTTP sidesteps that.
   First run creates `timesheet-demo.db` next to the project and seeds it with the same three
   demo users below — no `dotnet ef database update` needed.

3. **Point the client at that URL.** `client/.env.development` defaults to
   `https://localhost:7059`; override it locally with a gitignored `client/.env.local`
   (matches the `*.local` pattern in `client/.gitignore`, so it never gets committed):

   ```
   VITE_API_BASE_URL=http://localhost:5080
   ```

   From a terminal, in PowerShell (VS Code's default on Windows):

   ```powershell
   "VITE_API_BASE_URL=http://localhost:5080" | Out-File -Encoding utf8 client/.env.local
   ```

   or from Git Bash / WSL:

   ```bash
   echo "VITE_API_BASE_URL=http://localhost:5080" > client/.env.local
   ```

4. **Run the frontend** as in Frontend setup above (`npm install && npm run dev`), then sign in
   from the dropdown with any of the seeded users — no password needed.

## Tests

```
dotnet test
```

Covers the approval workflow state machine (Draft → Submitted → Approved/Rejected → resubmit),
authorization edges (wrong manager, wrong status), and the soft hour-validation warnings.

## Switching to Entra ID mode

Everything below is config-only — no code changes. Per Section 1 of the build brief, this is the
last step in the build sequence, done after the rest of the app works end-to-end in Local mode.

**API** (`appsettings.json` or environment variables in the target environment):
```
Auth:Provider = EntraId
Auth:EntraId:TenantId = <customer tenant id>
Auth:EntraId:ClientId = <app registration client id>
Auth:EntraId:Audience = api://<client id>            (optional — defaults to this)
```
First login from a new Entra identity JIT-provisions a local `Users` row (default role
`Employee`, unless an app role/group claim maps to Admin/Manager) — promote them via the Admin
Users screen afterwards, same as any other user.

**Client** (`client/.env.production` or your deploy's env):
```
VITE_AUTH_PROVIDER=EntraId
VITE_MSAL_CLIENT_ID=<app registration client id>
VITE_MSAL_TENANT_ID=<customer tenant id>
VITE_MSAL_API_SCOPE=api://<client id>/access_as_user
```

## Branding

Admin → Branding Config is frontend-only: company name, logo, and two colors are stored in
`localStorage` and applied live via CSS variables plus the document title/favicon. No API or
database table — each deployment sets its own branding once, in the browser.

## Out of scope for v1

Notifications, multi-level/delegate approval chains, reporting/export, and true multi-tenancy
(each deployment is single-tenant; config externalization is what makes it portable across
customers, not shared-instance multi-tenancy). See Section 8 of the build brief for the full list.
