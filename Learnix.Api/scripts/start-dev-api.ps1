param(
    [int]$ApiPort = 5199
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiRoot = Split-Path -Parent $scriptRoot
$projectPath = Join-Path $apiRoot "Learnix.API\Learnix.API.csproj"

Write-Host "Starting Learnix PostgreSQL on localhost:55432..."
Push-Location $apiRoot
try {
    docker compose up -d learnix-postgres
}
catch {
    Write-Host "Docker is not running. Open Docker Desktop, then run this script again." -ForegroundColor Yellow
    throw
}
finally {
    Pop-Location
}

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:LEARNIX_DB_CONNECTION = "Host=127.0.0.1;Port=55432;Database=learnix_db;Username=postgres;Password=jara130308;SSL Mode=Disable;GSS Encryption Mode=Disable"

Write-Host "Starting Learnix API on http://0.0.0.0:$ApiPort ..."
dotnet run --project $projectPath --urls "http://0.0.0.0:$ApiPort"
