# Database and authentication setup
The application uses ASP.NET Core Identity with EF Core MySQL (Pomelo). No public registration or hardcoded administrator password is provided.

1. Configure a strong MySQL password using environment variables; do not commit .env.
2. Generate and review the initial migration using .NET 8 SDK and dotnet-ef: `dotnet ef migrations add InitialCreate --project Workwise.Web`.
3. Apply migrations from a trusted deployment step: `dotnet ef database update --project Workwise.Web`. A reachable MySQL connection is required.
4. To initialize roles and create the first admin, set `Database__ApplyMigrationsOnStartup=true`, `BootstrapAdmin__Enabled=true`, `BootstrapAdmin__Email=...`, `BootstrapAdmin__Password=...` for a controlled first startup. Turn off both switches immediately afterward. No bootstrap admin is created if an account with the email already exists.
5. Login via /Account/Login and use role-protected dashboards. Passwords are hashed by Identity.

WARNING: Startup migrations require a committed migration. Until then, do not enable ApplyMigrationsOnStartup. This change adds the initialization pathway, NOT a generated migration. Avoid enabling migrations simultaneously on multiple app replicas.

The 27 business entities and Identity tables are mapped, including explicit restrictive foreign keys. Migrations, production deployment, and integration tests have not been executed here. Review foreign-key choices before applying to existing databases.