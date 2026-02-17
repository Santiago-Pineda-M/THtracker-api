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

## 2. Capa de Aplicación (Application Layer)

La capa de Aplicación orquesta la lógica de negocio utilizando los elementos del Dominio. Aquí se prueban los Casos de Uso (Use Cases), Validadores y Mappers.

### Ubicación
Las pruebas de aplicación se encuentran en: `THtracker.Tests/Unit/Application`

### Qué probar
- **Use Cases**: Verificar el flujo principal (Happy Path) y los casos de error (Validación fallida, Entidad no encontrada, Conflictos).
- **Validators**: Verificar que las reglas de validación (FluentValidation) funcionan correctamente para entradas válidas e inválidas.
- **Mappers**: Verificar la correcta transformación entre Entidades y DTOs (si aplica).

### Cómo ejecutar las pruebas
Para ejecutar solo las pruebas de la capa de Aplicación, utiliza el siguiente comando:

```bash
dotnet test --filter "FullyQualifiedName~Application"
```

### Reglas y Mejores Prácticas
1.  **Mocking**: Utilizar `Moq` para simular interfaces externas (`IRepository`, `IUnitOfWork`, `IPasswordHasher`).
2.  **Validación**: Simular la salida del validador o probar el validador por separado.
3.  **Result Pattern**: Verificar que los casos de uso devuelvan `Result.Success` o `Result.Failure` con el código de error adecuado.

### Estado Actual
- **Cobertura**: Use Cases principales (`Users`, `Auth`, `ActivityLogs`, `Activities`).
- **Total de pruebas**: 68 (Todas pasando).

## 3. Capa de Infraestructura (Infrastructure Layer)

La capa de Infraestructura implementa las interfaces definidas en la capa de Dominio y Aplicación, como Repositorios y Servicios Externos.

### Ubicación
Las pruebas de infraestructura se encuentran en: `THtracker.Tests/Unit/Infrastructure`

### Qué probar
- **Repositorios**: Verificar que las consultas a la base de datos (filtro, paginación, proyecciones) funcionen como se espera. Se utiliza **InMemoryDatabase** de EF Core para estas pruebas unitarias.
- **Implementaciones de Servicios**: Verificar la lógica de adaptadores o servicios que no dependen de I/O externo real (o que se pueden simular).

### Cómo ejecutar las pruebas
Para ejecutar solo las pruebas de la capa de Infraestructura, utiliza el siguiente comando:

```bash
dotnet test --filter "FullyQualifiedName~Infrastructure"
```

### Reglas y Mejores Prácticas
1.  **InMemory Database**: Usar una base de datos en memoria única por prueba (`Guid.NewGuid().ToString()`) para evitar contaminación de estado.
2.  **No Mock Context**: Probar contra un `DbContext` real (configurado en memoria) para validar el mapeo y las consultas LINQ.
3.  **Limpieza**: Implementar `IDisposable` para asegurar que `EnsureDeleted` se llame después de cada test.

### Estado Actual
- **Cobertura**: Repositorios `UserRepository` y `ActivityLogRepository`.
- **Total de pruebas**: 10 (Todas pasando).
