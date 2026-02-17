# Ejecutar la API THtracker en modo desarrollo
# Uso: .\scripts\run-api.ps1

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$ApiProject = Join-Path $Root 'THtracker.API\THtracker.API.csproj'

if (-not (Test-Path $ApiProject)) {
    Write-Error "No se encontró el proyecto API: $ApiProject"
    exit 1
}

Set-Location $Root
Write-Host "Iniciando THtracker.API..." -ForegroundColor Cyan
dotnet run --project $ApiProject
