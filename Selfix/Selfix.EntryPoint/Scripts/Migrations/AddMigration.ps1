# add-migration.ps1 - Creates a new migration
# Usage: .\add-migration.ps1 MigrationName

# Get script directory and repository paths
$scriptDir = $PSScriptRoot
$repoRoot = (Get-Item $scriptDir).Parent.Parent.Parent.Parent.FullName
$solutionDir = Join-Path $repoRoot "Selfix"
$infrastructureProject = Join-Path $solutionDir "Selfix.Infrastructure\Selfix.Infrastructure.csproj"
$entrypointProject = Join-Path $solutionDir "Selfix.EntryPoint\Selfix.EntryPoint.csproj"

# Check if migration name was provided
if (-not $args[0]) {
    Write-Host "Error: Migration name is required" -ForegroundColor Red
    Write-Host "Usage: .\AddMigration.ps1 MigrationName"
    exit 1
}

# Show paths for debugging
Write-Host "Using Infrastructure project: $infrastructureProject"
Write-Host "Using EntryPoint project: $entrypointProject"

# Restore tools from manifest
dotnet tool restore

# Create migration in Infrastructure project
dotnet ef migrations add $args[0] `
    --project $infrastructureProject `
    --startup-project $entrypointProject `
    --output-dir "Database/Migrations"