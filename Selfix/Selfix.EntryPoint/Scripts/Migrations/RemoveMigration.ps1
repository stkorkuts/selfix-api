# RemoveMigration.ps1 - Removes latest migration
# Usage: .\RemoveMigration.ps1

# Get script directory and repository paths
$scriptDir = $PSScriptRoot
$repoRoot = (Get-Item $scriptDir).Parent.Parent.Parent.Parent.FullName
$solutionDir = Join-Path $repoRoot "Selfix"
$infrastructureProject = Join-Path $solutionDir "Selfix.Infrastructure\Selfix.Infrastructure.csproj"
$entrypointProject = Join-Path $solutionDir "Selfix.EntryPoint\Selfix.EntryPoint.csproj"

# Show paths for debugging
Write-Host "Using Infrastructure project: $infrastructureProject"
Write-Host "Using EntryPoint project: $entrypointProject"

# Restore tools from manifest
dotnet tool restore

# Remove the latest migration
dotnet ef migrations remove `
    --project $infrastructureProject `
    --startup-project $entrypointProject