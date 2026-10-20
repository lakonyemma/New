# Executable MySQL migrations

The Docker Compose `migrator` service uses the .NET 8 SDK and pinned dotnet-ef 8.0.11 to generate an initial EF Core migration (if missing) and apply pending migrations. It waits for a healthy MySQL 8.4 instance. The web container starts only after the migration service exits successfully. The web service then seeds the three Identity roles using its configured startup initializer.

## First-time run
1. Copy `.env.example` to `.env`.
2. Set strong distinct `MYSQL_APP_PASSWORD` and `MYSQL_ROOT_PASSWORD`.
3. Set `INITIAL_SETUP_ENABLED=true` and a random `INITIAL_SETUP_TOKEN` with at least 32 characters. Keep the token private.
4. Run `docker compose up --build` from the repository root.
5. Inspect the generated files under `Workwise.Web/Migrations/` and commit them. Generated migrations should be code reviewed before use with valuable data.
6. Visit `http://localhost:8080/setup` in local development and create the administrator using your own password. For public deployments use HTTPS and restrict access to the setup page.
7. Disable initial setup and remove its token after creating the first administrator.

## Applying future migrations
After modifying EF models, generate a named migration explicitly:
```sh
docker compose run --rm migrator sh -c 'export PATH=/tmp/workwise-tools:$PATH; dotnet tool install dotnet-ef --version 8.0.11 --tool-path /tmp/workwise-tools >/dev/null; dotnet ef migrations add DescribeChange --project Workwise.Web --startup-project Workwise.Web'
docker compose run --rm migrator
```
The initial-migration script will not regenerate the initial migration if one already exists; it applies pending migrations.

## Limitations
- The initial migration is generated on first run, not checked into this commit. Docker/.NET/MySQL execution has not been tested here.
- Never automatically generate an initial migration against an existing production schema. Back up and review before migrating.
- The Compose web build can begin before the migrator finishes, but the web service will not start until migration succeeds.
- For production use a separate controlled migration deployment step and disable app startup migrations after initialization.
- For a host-only setup, install the .NET 8 SDK and dotnet-ef 8.0.11, provide a reachable MySQL connection string, then run `dotnet ef migrations add InitialCreate --project Workwise.Web` and `dotnet ef database update --project Workwise.Web`.
