# Guía de Pruebas Unitarias

Este documento describe la estrategia y ejecución de pruebas unitarias para el proyecto THtracker, organizadas por capa según la Clean Architecture.

## 1. Capa de Dominio (Domain Layer)

La capa de Dominio es el núcleo de la aplicación y contiene la lógica de negocio pura, entidades, objetos de valor y reglas de negocio. Las pruebas en esta capa deben ser rápidas y no depender de infraestructura externa (base de datos, red, etc.).

### Ubicación
Las pruebas de dominio se encuentran en: `THtracker.Tests/Unit/Domain`

### Qué probar
- **Entidades**: Verificar constructores, métodos de negocio, validaciones y cambios de estado.
- **Value Objects**: Verificar igualdad y validación de formato.
- **Domain Services**: Verificar lógica de negocio compleja que involucra múltiples entidades.

### Cómo ejecutar las pruebas
Para ejecutar solo las pruebas de la capa de Dominio, utiliza el siguiente comando:

```bash
dotnet test --filter "FullyQualifiedName~Domain"
```

### Reglas y Mejores Prácticas
1.  **Aislamiento**: Las pruebas no deben tener dependencias externas.
2.  **Validación de Excepciones**: Usar excepciones específicas (`ArgumentException`, `InvalidOperationException`) en lugar de `Exception` genérica. Verificar el mensaje de error o el parámetro.
3.  **Naming Convention**: `Metodo_Escenario_ResultadoEsperado`.
    *   Ejemplo: `GetDurationInInterval_ShouldReturnZeroMinutes_WhenLogIsOutsideInterval`
4.  **Asssertions**: Usar `FluentAssertions` para comprobaciones expresivas.

### Estado Actual
- **Cobertura**: Entidades `ActivityLog`, `User`, `Role`.
- **Total de pruebas**: 17 (Todas pasando).
- **Mejoras Realizadas**:
    - Refactorización de `ActivityLog` para usar excepciones específicas (`ArgumentException`).
    - Expansión de pruebas en `User` para cubrir gestión de roles y logins.
    - Creación de pruebas para `Role` y gestión de permisos.
