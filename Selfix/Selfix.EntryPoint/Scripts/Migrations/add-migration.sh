#!/bin/bash
# add-migration.sh - Creates a new migration
# Usage: ./add-migration.sh MigrationName

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
REPO_ROOT="$( cd "${SCRIPT_DIR}/../../../../" &> /dev/null && pwd )"
SOLUTION_DIR="${REPO_ROOT}/Selfix"
INFRASTRUCTURE_PROJECT="${SOLUTION_DIR}/Selfix.Infrastructure/Selfix.Infrastructure.csproj"
ENTRYPOINT_PROJECT="${SOLUTION_DIR}/Selfix.EntryPoint/Selfix.EntryPoint.csproj"

# Check if migration name was provided
if [ -z "$1" ]; then
  echo "Error: Migration name is required"
  echo "Usage: ./add-migration.sh MigrationName"
  exit 1
fi

# Show paths for debugging
echo "Using Infrastructure project: ${INFRASTRUCTURE_PROJECT}"
echo "Using EntryPoint project: ${ENTRYPOINT_PROJECT}"

# Restore tools from manifest
dotnet tool restore 

# Create migration in Infrastructure project
dotnet ef migrations add "$1" \
  --project "${INFRASTRUCTURE_PROJECT}" \
  --startup-project "${ENTRYPOINT_PROJECT}" \
  --output-dir "Database/Migrations"