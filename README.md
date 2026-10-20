# Workwise
ASP.NET Core MVC + MySQL + Docker starting implementation.

## Run
Copy .env.example to .env and replace passwords, then run `docker compose up --build`.
Open http://localhost:8080.

## Important setup remaining
- Apply EF Core migrations (`dotnet ef migrations add InitialCreate --project Workwise.Web` and `dotnet ef database update --project Workwise.Web` with database running).
- Seed Admin, Supervisor and Employee roles and an initial admin through a secure setup process.
- Implement complete role workflows, CRUD, notifications, audit logs, allocation transaction rules and automated tests.
- Configure HTTPS and secrets for production.

This is a foundation, NOT a completed production system.