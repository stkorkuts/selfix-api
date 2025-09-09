param (
    [Parameter(Mandatory=$false)]
    [string]$LocalImageName = "infrastructure-api",
    
    [Parameter(Mandatory=$false)]
    [string]$RegistryUrl = "cr.selcloud.ru",
    
    [Parameter(Mandatory=$false)]
    [string]$RegistryRepo = "selfix-beta-registry",
    
    [Parameter(Mandatory=$false)]
    [string]$ImageName = "selfix-beta-infrastructure-api-app",
    
    [Parameter(Mandatory=$false)]
    [string]$Tag = "latest",
    
    [Parameter(Mandatory=$false)]
    [string]$BuildContext = (Join-Path $PSScriptRoot ".." "Selfix")  # Default to Selfix directory
)

# Construct the full registry path
$RegistryPath = "$RegistryUrl/$RegistryRepo/$ImageName`:$Tag"

# Display execution plan
Write-Host "Deploying container with the following parameters:" -ForegroundColor Green
Write-Host "  Local Image Name: $LocalImageName" -ForegroundColor Cyan
Write-Host "  Registry URL: $RegistryUrl" -ForegroundColor Cyan
Write-Host "  Registry Repository: $RegistryRepo" -ForegroundColor Cyan
Write-Host "  Image Name: $ImageName" -ForegroundColor Cyan
Write-Host "  Tag: $Tag" -ForegroundColor Cyan
Write-Host "  Full Registry Path: $RegistryPath" -ForegroundColor Cyan
Write-Host "  Build Context: $BuildContext" -ForegroundColor Cyan

# Verify Dockerfile exists in build context
if (-not (Test-Path (Join-Path $BuildContext "Dockerfile"))) {
    Write-Host "Error: Dockerfile not found in build context: $BuildContext" -ForegroundColor Red
    exit 1
}

Write-Host "`n1. Building Docker image..." -ForegroundColor Yellow
docker build -t $RegistryPath $BuildContext
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`n2. Pushing Docker image to registry..." -ForegroundColor Yellow
docker push $RegistryPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker push failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nDeployment completed successfully!" -ForegroundColor Green