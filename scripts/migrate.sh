#!/bin/sh
set -eu
cd /workspace
export PATH="/tmp/workwise-tools:$PATH"
dotnet tool install dotnet-ef --version 8.0.11 --tool-path /tmp/workwise-tools >/dev/null
dotnet restore Workwise.Web/Workwise.Web.csproj
dotnet build Workwise.Web/Workwise.Web.csproj --no-restore -c Debug
if ! find Workwise.Web/Migrations -maxdepth 1 -name '*_InitialCreate.cs' -print -quit 2>/dev/null | grep -q .; then
  if [ -d Workwise.Web/Migrations ] && find Workwise.Web/Migrations -maxdepth 1 -name '*_*.cs' | grep -q .; then
    echo "Existing migrations found, refusing to generate a second InitialCreate." >&2
    exit 1
  fi
  echo "Generating initial EF Core migration in Workwise.Web/Migrations..."
  dotnet ef migrations add InitialCreate --project Workwise.Web/Workwise.Web.csproj --startup-project Workwise.Web/Workwise.Web.csproj --output-dir Migrations --no-build
fi
echo "Applying pending migrations to MySQL..."
dotnet ef database update --project Workwise.Web/Workwise.Web.csproj --startup-project Workwise.Web/Workwise.Web.csproj --no-build
echo "Migration completed. Commit the generated Migrations directory to source control."
