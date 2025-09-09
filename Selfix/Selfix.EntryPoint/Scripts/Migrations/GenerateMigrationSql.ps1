# GenerateMigrationSql.ps1 - Generates SQL migration script
# Usage: .\GenerateMigrationSql.ps1 [FromMigration] [ToMigration]

# Get script directory and repository paths
$scriptDir = $PSScriptRoot
$repoRoot = (Get-Item $scriptDir).Parent.Parent.Parent.Parent.FullName
$solutionDir = Join-Path $repoRoot "Selfix"
$infrastructureProject = Join-Path $solutionDir "Selfix.Infrastructure\Selfix.Infrastructure.csproj"
$entrypointProject = Join-Path $solutionDir "Selfix.EntryPoint\Selfix.EntryPoint.csproj"
$migrationScriptsDir = Join-Path $repoRoot "MigrationScripts"

# Show paths for debugging
Write-Host "Using Infrastructure project: $infrastructureProject"
Write-Host "Using EntryPoint project: $entrypointProject"
Write-Host "Output directory: $migrationScriptsDir"

# Restore tools from manifest
dotnet tool restore

# Create migrations directory if it doesn't exist
if (-not (Test-Path $migrationScriptsDir)) {
    New-Item -ItemType Directory -Path $migrationScriptsDir | Out-Null
}

# Generate SQL script based on parameters provided
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$outputFile = Join-Path $migrationScriptsDir "migration-$timestamp.sql"

if (-not $args[0] -or -not $args[1]) {
    Write-Host "Generating script for all migrations..."
    $result = dotnet ef migrations script `
        --project $infrastructureProject `
        --startup-project $entrypointProject `
        --idempotent `
        --output $outputFile
} else {
    Write-Host "Generating script from '$($args[0])' to '$($args[1])'..."
    $result = dotnet ef migrations script $args[0] $args[1] `
        --project $infrastructureProject `
        --startup-project $entrypointProject `
        --idempotent `
        --output $outputFile
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration script generated successfully in $migrationScriptsDir" -ForegroundColor Green
} else {
    Write-Host "Error generating migration script" -ForegroundColor Red
    exit 1
}