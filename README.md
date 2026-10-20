# Workwise — ASP.NET Core MVC
Three role-based areas: Admin, Supervisor, Employee. MySQL via Pomelo EF Core, ASP.NET Core Identity and Docker Compose.

## Local setup
1. Install .NET 8 SDK, Docker, and VS Code C# Dev Kit.
2. Copy `.env.example` to `.env`, replace both passwords.
3. Run `docker compose up --build` to start app and MySQL.
4. Generate migrations from the host with `dotnet tool install --global dotnet-ef --version 8.*`, then `dotnet ef migrations add InitialCreate --project Workwise.Web` and `dotnet ef database update --project Workwise.Web`. When running migrations from the host, override the connection string to point to a reachable MySQL port; the default compose database is not exposed to the host. Alternatively run migrations inside a container with the SDK and configured connection string.
5. Seed roles and an initial admin securely before logging in. No default credentials are provided.

## Current implementation
- MVC foundation, Identity, initial database entities and allocation scoring
- Role-protected dashboards
- Admin role assignment UI
- Supervisor task creation and allocation suggestions
- Employee task list and submission endpoint

## Not yet complete
The 32-table schema, migrations, initial admin setup, end-to-end authentication provisioning, full CRUD, allocation concurrency control, detailed review workflows, and automated tests are not complete. Do not deploy to production yet.
