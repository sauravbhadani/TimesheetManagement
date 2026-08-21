# Build Prompt: Internal Employee Timesheet Module

## Context
Build an internal employee timesheet application. It should be built generically enough to
be reused across customers/organizations — start in a personal GitHub repo for local
development, and later be pointed at a specific customer's environment (SSO tenant, database,
hosting) purely through configuration, with no code rewrite.

Employees log weekly hours against Projects and Tasks (with CapEx/OpEx and billable
classification). Managers approve or reject submitted timesheets. Admins manage master data
(Projects, Tasks, Users/Roles) and app branding.

---

## 1. Tech Stack

- **Backend:** ASP.NET Core 8 Web API, layered architecture (Controllers → Services →
  Repositories), EF Core (code-first, migrations checked in).
- **Frontend:** React + TypeScript, **Bootstrap 5** for all UI (no heavy component library —
  keep it simple, form- and table-driven).
- **Database:** SQL Server (works locally via LocalDB/Docker; same schema deploys to Azure SQL
  or any customer's SQL Server later).
- **Auth:** Must be **pluggable between two modes**, switched via config (e.g.
  `Auth:Provider = "Local"` or `"EntraId"` in `appsettings.json` / `appsettings.Development.json`),
  so the app runs standalone on a local machine today and can be pointed at a customer's Entra
  tenant later with a config change only — no rewrite.
  - **Local mode (dev, default for now):** No Entra ID dependency. Use a simple dev auth
    scheme — e.g. a login screen with a dropdown/select of seeded test users (one Admin, one
    Manager, one Employee) that issues a local JWT (or a cookie) asserting that user's Id and
    Role. All downstream `[Authorize(Roles=...)]` checks work identically to production.
  - **Entra ID mode (customer deployment):** MSAL on the React side, JWT bearer validation on
    the API side. Roles resolved via Entra ID group claims/app roles, mapped to the local
    `Users` table on first login (JIT provisioning).
  - Implementation approach: abstract auth behind a single API middleware/service
    (e.g. `IAuthProvider` or two separate ASP.NET Core auth schemes selected at startup based
    on config) so controllers and the React auth context don't need to know which mode is active.
- **Hosting target:** Design so it can run on any standard Azure App Service (API + static
  React build, or split into two App Services) — avoid hard-coding any customer-specific
  infrastructure names.
- **Logging:** Azure Application Insights, wired via the `Microsoft.ApplicationInsights.AspNetCore`
  SDK — request/dependency tracing, exception tracking, and structured `ILogger` logs routed
  to App Insights. Instrumentation key/connection string should come from config, so it can be
  swapped per customer/environment.

---

## 2. Roles & Permissions

| Role | Permissions |
|---|---|
| **Admin** | Create/edit/deactivate Projects and Tasks; set CapEx/OpEx and billable flags on Tasks; manage Users and role assignments; manage Manager↔Employee mapping; configure app branding; view all timesheets (read-only, all employees). |
| **Manager** | View timesheets submitted by employees who report to them; approve or reject (with comments) submitted timesheets; view team timesheet history/status. Cannot edit hours themselves. |
| **Employee** | Fill in own weekly timesheet against active Projects/Tasks; save as draft; submit for approval; view own submission history and status (Draft / Submitted / Approved / Rejected); edit and resubmit rejected timesheets. |

A user can only ever be assigned one primary role for this app, but the data model should
allow a Manager to also submit their own timesheet as an Employee (i.e., Manager is a superset,
not a separate person type).

---

## 3. Data Model (EF Core entities)

- **User**
  `Id, ExternalAuthId, FullName, Email, Role (Admin/Manager/Employee), ManagerId (self-FK, nullable), IsActive, CreatedAt`
  *(`ExternalAuthId` holds the Entra Object Id in Entra mode, or the seeded local user id in local mode.)*

- **Project**
  `Id, Name, Code, Description, IsActive, CreatedBy, CreatedAt`

- **ProjectTask** (Task is a reserved word — name the entity `ProjectTask`)
  `Id, ProjectId (FK), Name, Description, Classification (enum: CapEx / OpEx), IsBillable (bool), IsActive, CreatedAt`

- **TimesheetWeek** (header — one per employee per week)
  `Id, UserId (FK), WeekStartDate, WeekEndDate, Status (enum: Draft / Submitted / Approved / Rejected), SubmittedAt, ApprovedBy (FK User, nullable), ApprovedAt, RejectionComment, TotalHours (computed/denormalized)`

- **TimesheetEntry** (one row per Project+Task per week; daily hours as columns or a child table)
  `Id, TimesheetWeekId (FK), ProjectId (FK), ProjectTaskId (FK), MonHours, TueHours, WedHours, ThuHours, FriHours, SatHours, SunHours, Notes`
  *(Flat day columns keep the weekly grid simple to build and query — avoid a separate
  per-day child table unless you specifically need it later.)*

- **ApprovalHistory** (optional but recommended for audit trail)
  `Id, TimesheetWeekId (FK), Action (Submitted/Approved/Rejected), ActionBy (FK User), ActionAt, Comment`

*(No branding table — see Section 5, branding is handled entirely on the frontend, no API.)*

---

## 4. Business Rules

- Weekly timesheet = Monday–Sunday (adjust if a given customer's fiscal week differs — keep
  this configurable rather than hard-coded).
- Employee can only log hours against **active** Projects/Tasks.
- Draft timesheets are freely editable. Once **Submitted**, the timesheet is locked for the
  employee until the manager acts on it.
- Manager **Approve** → status becomes Approved, timesheet becomes fully read-only for
  everyone except Admin.
- Manager **Reject** → requires a comment; status becomes Rejected; employee can edit and
  resubmit (goes back to Submitted).
- Total hours per day/week should be validated against a configurable max (e.g., no more than
  24 hours/day, warn above 8–10 hours/day — keep this a soft validation, not a hard block,
  unless a customer wants a hard cap).
- Admin can deactivate a Project/Task; deactivated items remain visible on historical
  timesheets but can't be selected for new entries.

---

## 5. Screens

### Employee
- **My Timesheet** — weekly grid: rows = Project + Task (with CapEx/OpEx and billable badges),
  columns = Mon–Sun, row/column/grand totals, Save Draft / Submit buttons.
- **My History** — list of past weeks with status badges (Draft/Submitted/Approved/Rejected),
  click to view/edit.

### Manager
- **Team Timesheets** — list of direct reports' submitted timesheets pending action, filterable
  by status/week.
- **Review Timesheet** — read-only weekly grid view of an employee's timesheet with
  Approve / Reject (+ comment) actions.

### Admin
- **Projects** — CRUD grid for Projects (name, code, active flag).
- **Tasks** — CRUD grid for Tasks under a Project (name, CapEx/OpEx, billable, active flag).
- **Users** — list/manage users, assign Role and Manager.
- **Branding Config** — a single frontend-only settings page, **no backend API/DB table**.
  Fields: Company Name (text), Logo (file upload or URL — store as a data URL or file path),
  Primary Color, Secondary Color (color pickers). Live preview panel showing header/button
  colors and logo as you change them. On Save, persist to `localStorage` (or a small local
  JSON config file the React app reads on load) and apply immediately app-wide via CSS
  variables + update `<title>`/favicon. This keeps it simple and portable across customers —
  each deployment just sets its own branding through this page; no code change needed. If a
  customer later wants branding centrally managed (same settings pushed to every user), it can
  be swapped for a proper API-backed table without changing the rest of the app.

All screens: simple Bootstrap tables/forms/cards, no heavy custom styling — prioritize
clarity and speed to build over visual polish.

---

## 6. API Endpoints (suggested)

```
Auth: token required on all endpoints below (except health check) — Local dev token or
      Entra ID token, depending on active auth mode (see Section 1)

GET    /api/projects
POST   /api/projects                          [Admin]
PUT    /api/projects/{id}                     [Admin]
GET    /api/projects/{id}/tasks
POST   /api/tasks                             [Admin]
PUT    /api/tasks/{id}                        [Admin]

GET    /api/users
PUT    /api/users/{id}/role                   [Admin]
PUT    /api/users/{id}/manager                [Admin]

GET    /api/timesheets/mine?week={date}       [Employee]
POST   /api/timesheets                        [Employee]  (create/update draft)
POST   /api/timesheets/{id}/submit            [Employee]
GET    /api/timesheets/mine/history           [Employee]

GET    /api/timesheets/team?status=Submitted  [Manager]
POST   /api/timesheets/{id}/approve           [Manager]
POST   /api/timesheets/{id}/reject            [Manager]  (body: comment)

GET    /api/timesheets/all                    [Admin, read-only]
```

---

## 7. Non-Functional Requirements

- Server-side validation on every write endpoint (don't rely on client-side only).
- Role checks enforced via `[Authorize(Roles=...)]` or policy-based auth, not just hidden UI.
- Global error handling middleware, consistent error response shape.
- Basic unit tests for the approval workflow state transitions (Draft→Submitted→Approved/Rejected).
- EF Core migrations checked in; seed script for initial Admin user and a couple of sample
  Projects/Tasks.
- Keep all customer-specific values (connection strings, Entra tenant/app registration IDs,
  App Insights keys, base URLs) in config (`appsettings.json` / environment variables), never
  hard-coded, so the same codebase can be deployed per customer.
- README with setup steps (connection string, optional Entra app registration values,
  `dotnet ef database update`, `npm install && npm start`).

---

## 8. Explicitly Out of Scope for v1

- Email/Teams notifications on submit/approve/reject
- Multi-level or delegate approval chains
- Reporting/export (Excel/PDF) — flag as a fast-follow
- Multi-tenant support (multiple customers sharing one running instance/database) — each
  deployment is single-tenant for now; keeping config externalized (per Section 7) is what
  makes it portable across customers, not true multi-tenancy

---

## Deliverable

A working end-to-end app: ASP.NET Core Web API + React (Bootstrap 5) frontend + SQL Server
schema via EF Core migrations, with pluggable Local/Entra ID auth for all three roles, the
weekly timesheet grid, the manager approval flow, Admin master-data screens, and the Branding
Config page described above. Build incrementally (schema → API → local auth → Employee grid →
Manager approval → Admin CRUD → Branding → Entra ID mode), verifying each layer before moving
to the next.
