#!/bin/bash
# generate-migration-sql.sh - Generates SQL migration script
# Usage: ./generate-migration-sql.sh [FromMigration] [ToMigration]

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
REPO_ROOT="$( cd "${SCRIPT_DIR}/../../../../" &> /dev/null && pwd )"
SOLUTION_DIR="${REPO_ROOT}/Selfix"
INFRASTRUCTURE_PROJECT="${SOLUTION_DIR}/Selfix.Infrastructure/Selfix.Infrastructure.csproj"
ENTRYPOINT_PROJECT="${SOLUTION_DIR}/Selfix.EntryPoint/Selfix.EntryPoint.csproj"
MIGRATION_SCRIPTS_DIR="${REPO_ROOT}/MigrationScripts"

# Show paths for debugging
echo "Using Infrastructure project: ${INFRASTRUCTURE_PROJECT}"
echo "Using EntryPoint project: ${ENTRYPOINT_PROJECT}"
echo "Output directory: ${MIGRATION_SCRIPTS_DIR}"

# Restore tools from manifest
dotnet tool restore

# Create migrations directory if it doesn't exist
mkdir -p "${MIGRATION_SCRIPTS_DIR}"

# Generate SQL script
if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Generating script for all migrations..."
  dotnet ef migrations script \
    --project "${INFRASTRUCTURE_PROJECT}" \
    --startup-project "${ENTRYPOINT_PROJECT}" \
    --idempotent \
    --output "${MIGRATION_SCRIPTS_DIR}/migration-$(date +%Y%m%d%H%M%S).sql"
else
  echo "Generating script from '$1' to '$2'..."
  dotnet ef migrations script "$1" "$2" \
    --project "${INFRASTRUCTURE_PROJECT}" \
    --startup-project "${ENTRYPOINT_PROJECT}" \
    --idempotent \
    --output "${MIGRATION_SCRIPTS_DIR}/migration-$(date +%Y%m%d%H%M%S).sql"
fi

if [ $? -eq 0 ]; then
  echo "Migration script generated successfully in ${MIGRATION_SCRIPTS_DIR}"
else
  echo "Error generating migration script"
  exit 1
fi