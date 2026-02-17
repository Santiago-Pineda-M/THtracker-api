# Scripts utilitarios – THtracker

Scripts PowerShell para compilar, probar y ejecutar el proyecto desde la raíz del repositorio.

| Script | Descripción |
|--------|-------------|
| **build.ps1** | Compila la solución. `-Configuration Release` para release. |
| **test.ps1** | Ejecuta todos los tests. `-Filter "FullyQualifiedName~Domain"` para filtrar. `-NoBuild` para no compilar antes. |
| **coverage.ps1** | Ejecuta tests con cobertura (Coverlet). `-OpenReport` genera y abre el reporte HTML (requiere `reportgenerator`). |
| **clean.ps1** | Limpia bin/obj, TestResults y CoverageReport. |
| **run-api.ps1** | Arranca la API en modo desarrollo. |

## Uso desde la raíz del proyecto

```powershell
.\scripts\build.ps1
.\scripts\test.ps1
.\scripts\coverage.ps1 -OpenReport
.\scripts\clean.ps1
.\scripts\run-api.ps1
```

## Reporte de cobertura

Para ver el porcentaje de cobertura en HTML:

1. Instalar ReportGenerator (una vez):  
   `dotnet tool install -g dotnet-reportgenerator-globaltool`
2. Ejecutar:  
   `.\scripts\coverage.ps1 -OpenReport`
