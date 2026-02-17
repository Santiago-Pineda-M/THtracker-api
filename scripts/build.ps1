# Build del proyecto THtracker
# Uso: .\scripts\build.ps1 [-Configuration Release]

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$Sln = Join-Path $Root 'THtracker.sln'

Set-Location $Root
Write-Host "Compilando solución ($Configuration)..." -ForegroundColor Cyan
dotnet build $Sln -c $Configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build completado." -ForegroundColor Green
