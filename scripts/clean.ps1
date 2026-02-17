# Limpiar artefactos de compilación, tests y cobertura
# Uso: .\scripts\clean.ps1

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$Sln = Join-Path $Root 'THtracker.sln'

Set-Location $Root

Write-Host "Limpiando solución..." -ForegroundColor Cyan
dotnet clean $Sln -nologo -v q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$Dirs = @(
    (Join-Path $Root 'TestResults'),
    (Join-Path $Root 'CoverageReport')
)

foreach ($d in $Dirs) {
    if (Test-Path $d) {
        Remove-Item $d -Recurse -Force
        Write-Host "Eliminado: $d" -ForegroundColor Gray
    }
}

Get-ChildItem -Path $Root -Recurse -Directory -Filter 'bin' -ErrorAction SilentlyContinue | ForEach-Object {
    Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Eliminado: $($_.FullName)" -ForegroundColor Gray
}
Get-ChildItem -Path $Root -Recurse -Directory -Filter 'obj' -ErrorAction SilentlyContinue | ForEach-Object {
    Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Eliminado: $($_.FullName)" -ForegroundColor Gray
}

Write-Host "Limpieza completada." -ForegroundColor Green
