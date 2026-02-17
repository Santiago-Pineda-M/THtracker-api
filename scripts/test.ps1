# Ejecutar tests del proyecto THtracker
# Uso: .\scripts\test.ps1 [-Filter "FullyQualifiedName~Domain"]

param(
    [string]$Filter = $null,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$Sln = Join-Path $Root 'THtracker.sln'

Set-Location $Root
$Args = @('test', $Sln)
if ($NoBuild) { $Args += '--no-build' }
if ($Filter)  { $Args += '--filter', $Filter }

Write-Host "Ejecutando tests..." -ForegroundColor Cyan
& dotnet @Args
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Tests completados." -ForegroundColor Green
