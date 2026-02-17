# Ejecutar tests con cobertura de código y opcionalmente generar reporte HTML
# Uso: .\scripts\coverage.ps1 [-OpenReport]
# Requiere: dotnet tool install -g dotnet-reportgenerator-globaltool (para -OpenReport)

param(
    [switch]$OpenReport
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$Sln = Join-Path $Root 'THtracker.sln'
$ResultsDir = Join-Path $Root 'TestResults'
$ReportDir = Join-Path $Root 'CoverageReport'

Set-Location $Root

# Limpiar resultados previos
if (Test-Path $ResultsDir) { Remove-Item $ResultsDir -Recurse -Force }
if (Test-Path $ReportDir)  { Remove-Item $ReportDir -Recurse -Force }

Write-Host "Ejecutando tests con cobertura..." -ForegroundColor Cyan
dotnet test $Sln --collect:"XPlat Code Coverage" --results-directory $ResultsDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$CoverageFile = Get-ChildItem -Path $ResultsDir -Recurse -Filter 'coverage.cobertura.xml' -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $CoverageFile) {
    Write-Host "No se encontró archivo de cobertura. Asegúrate de tener coverlet.collector en el proyecto de tests." -ForegroundColor Yellow
    exit 0
}

if ($OpenReport) {
    $ReportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue
    if (-not $ReportGenerator) {
        Write-Host "ReportGenerator no está instalado. Instálalo con: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
        Write-Host "Los datos de cobertura están en: $($CoverageFile.FullName)" -ForegroundColor Gray
        exit 0
    }
    Write-Host "Generando reporte HTML..." -ForegroundColor Cyan
    & reportgenerator -reports:"$($CoverageFile.FullName)" -targetdir:$ReportDir -reporttypes:Html
    $IndexPath = Join-Path $ReportDir 'index.html'
    Write-Host "Reporte generado: $IndexPath" -ForegroundColor Green
    Start-Process $IndexPath
} else {
    Write-Host "Cobertura generada en: $($CoverageFile.FullName)" -ForegroundColor Green
    Write-Host "Para reporte HTML ejecuta: .\scripts\coverage.ps1 -OpenReport" -ForegroundColor Gray
}
