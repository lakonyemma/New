# Workwise · ASP.NET Core MVC + Razor Views + MySQL

A **new full-stack, server-rendered** implementation of the Khan Associates
Intelligent Task Allocation and Workload Management System.

**No React, no Flutter, no separate JavaScript frontend framework.**

## Stack

| Layer | Technology |
|---|---|
| User interface | ASP.NET Core MVC Razor Views (`.cshtml`), CSS, tiny vanilla JS navigation helper |
| Server | C# with ASP.NET Core MVC 10 controllers, validation and role authorization |
| Database | MySQL 8+ / Entity Framework Core (Oracle `MySql.EntityFrameworkCore`) |
| Authentication | ASP.NET Core cookie authentication, ASP.NET password hashing, rate-limited login |
| Roles | Administrator, Supervisor, Employee |
| Allocation | Matching required skills with employee capacity; supervisor makes final choice |

This is independent of the existing Workwise-Frontend / WorkWise-Backend code.
It is implemented in the **`lakonyemma/New` GitHub repository**, so the older
application remains untouched.

## Run from Visual Studio / VS Code

Prerequisites: .NET 10 SDK, MySQL Server 8.0+ and a code editor.

1. Create a **new, empty** MySQL database, for example run
   `sql/create_database.sql` in MySQL Workbench.
2. Create a dedicated MySQL user with permission to create and manage tables in
   that database. Don't run the application under the MySQL root account.
3. Supply credentials **locally through environment variables**, not committed
   `appsettings.json`.

**PowerShell (Windows):**
```powershell
$env:ConnectionStrings__Workwise = "Server=127.0.0.1;Port=3306;Database=WorkwiseDB;User ID=workwise_app;Password=<mysql-password>;SslMode=Preferred"
$env:Bootstrap__Password = "<a-new-admin-password-with-at-least-12-characters>"
dotnet restore
dotnet run --project Workwise.Mvc.csproj
```

**Bash (Ubuntu/Linux):**
```bash
export ConnectionStrings__Workwise='Server=127.0.0.1;Port=3306;Database=WorkwiseDB;User ID=workwise_app;Password=<mysql-password>;SslMode=Preferred'
export Bootstrap__Password='<a-new-admin-password-with-at-least-12-characters>'
dotnet restore
dotnet run --project Workwise.Mvc.csproj
```

Use the URL displayed in the terminal (or launch VS with HTTPS), then sign in
with the username **`admin`** and your chosen initial admin password.
The app creates the initial administrator **only if the user table is empty**;
do not reuse the bootstrap password for other accounts.

**Never commit a real password.** Use User Secrets locally or a hosted secret
manager when deploying. If using environment variables manually, clear them
when the session ends.

## Implemented pages and workflows

- **Sign in / sign out**: server-validated credentials, secure HTTP-only cookie,
  account-active check and login-rate limiting.
- **Overview**: role-filtered task counts, overdue work and shortcuts.
- **Employees**: team directory, skill profiles and weekly capacity;
  administrators create employee login accounts.
- **Engagements**: client work with service line, stage, due date and notes;
  employees only see engagements linked to assigned tasks.
- **Tasks**: create/edit work, link to engagements, view requirements and progress;
  employees only see their own work.
- **Recommendations**: compare requested skill tags with employee skills and
  calculate indicative projected workload; supervisors choose the assignee.
- **Human confirmation**: over-capacity assignment requires an explicit reason.
- **Execution and approval**: employees update progress and submit at 100%;
  supervisor approves completion or returns with feedback.
- **Activity**: basic task update history and audit record creation.
- **Responsive Razor UI**: server-rendered HTML with a custom navy/orange design.

## Security and deployment

**Production deployment still requires review and configuration.** HTTPS must be
enabled, MySQL access restricted to trusted hosts, and TLS certificates validated.
Use persistent backup storage for MySQL and ASP.NET Core Data Protection keys,
monitor log-in attempts, and review business/data-retention requirements.

`EnsureCreatedAsync` initializes the tables for an **empty** database; it is
**not a migration engine**. Use EF Core migrations or a reviewed migration plan
before changing a database that already contains company records.

### Transfer the database to a company server using a USB flash drive

Yes, you can transfer a database using a flash drive; transfer a **logical
backup**, not raw MySQL engine database directories while MySQL is running.

1. On the source computer, export using MySQL Workbench **Data Export** or
   `mysqldump` to a `.sql` file.
2. Store the backup securely; use an encrypted USB device if company records
   or credentials are involved.
3. Install MySQL on the destination server, create the database and authorized
   database user, then import with MySQL Workbench **Data Import** or MySQL CLI.
4. Set the MVC server's `ConnectionStrings__Workwise` securely to the destination
   MySQL server; test the app and verify row counts/relationships.
5. Protect and verify scheduled backups after the move. Import into a fresh
   database first; don't overwrite the original before verification.

## Continuous integration

GitHub Actions runs a .NET 10 build (including Razor compilation) and browser-
independent HTTP smoke tests against a disposable **MySQL 8.4** database.
See `.github/workflows/ci.yml` and `tests/smoke.py`.

## Current limitations

This is a substantial **first MVC implementation**, not a full replacement for
every endpoint in the existing Workwise API. Not yet included: file upload/
download, password change/recovery, multifactor login, notifications, leave
requests, advanced workload forecasting and production-grade schema migrations.
Those should be implemented before claiming full parity with the older system.
