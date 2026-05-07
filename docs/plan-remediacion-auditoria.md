# Plan de remediación — auditoría API THtracker

Documento de trabajo para cerrar los hallazgos de la auditoría (validación, paginación, DELETE/HTTP, seguridad HTTP, JWT/CORS, contratos API, deuda menor).

**Orden recomendado:** respetar la secuencia 1 → 7 para reducir re-trabajo y conflictos en tests.

---

## 1. Pipeline de validación (MediatR + FluentValidation)

**Objetivo:** Que todo `AbstractValidator<T>` registrado en Application se ejecute automáticamente antes de cada `IRequest` manejado por MediatR.

**Tareas:**

1. Crear `THtracker.Application/Common/Behaviors/ValidationBehavior.cs` que implemente `IPipelineBehavior<TRequest, TResponse>`:
   - Inyectar `IEnumerable<IValidator<TRequest>>`.
   - Si no hay validators, delegar al siguiente.
   - Si hay errores de validación, devolver fallo coherente con el patrón `Result` del dominio (no lanzar para flujo esperado) **o** lanzar `ValidationException` si el proyecto ya mapea eso en middleware — **decidir una sola estrategia** y usarla en todos los handlers.
2. Registrar el behavior en `THtracker.Application/DependencyInjection.cs` dentro de `AddMediatR` (`cfg.AddOpenBehavior(typeof(ValidationBehavior<,>))` o registro genérico según versión de MediatR).
3. Verificar que exista validator por command/query crítico; como mínimo completar los que hoy faltan (p. ej. `RefreshTokenCommand` si aplica reglas de negocio además de formato).
4. Añadir/ajustar tests unitarios del behavior (caso válido, caso inválido).

**Criterio de hecho:** Enviar un command inválido vía `ISender` en test de integración o unitario y comprobar que **no** entra al handler y la respuesta es 400 con el contrato de error acordado.

---

## 2. Paginación en listados

**Objetivo:** Toda colección expuesta vía API con `pageNumber`, `pageSize` (default 20, máximo 100), `totalCount`, `totalPages`.

**Tareas:**

1. Definir en Application un record común, p. ej. `PaginatedResponse<T>` (o nombre alineado al proyecto, **sin** sufijo `Dto`).
2. Por cada query de listado usada en controladores:
   - `GetAllCategories`, `GetAllActivities`, `GetAllTaskLists`, `GetActivityLogs`, `GetActiveActivityLogs`, `GetAllTasksByTaskList`, `GetUserSessions`, `GetAllRoles`, etc.
   - Extender la query con `PageNumber` y `PageSize` (o un record `PaginationRequest` si se superan 3 parámetros en la firma).
3. En repositorios correspondientes, aplicar `Skip`/`Take` y una consulta de conteo (o patrón eficiente con EF) **sin** exponer `IQueryable` fuera del repositorio.
4. Actualizar controladores: parámetros `[FromQuery]` para paginación donde hoy solo hay `GetAll`.
5. Actualizar tests y contratos OpenAPI (`ProducesResponseType`).

**Criterio de hecho:** Ningún `GET` de lista devuelve colección ilimitada sin paginación; límites y totales documentados.

---

## 3. DELETE → 204 NoContent

**Objetivo:** Éxito en borrado sin cuerpo; alineado con `[ProducesResponseType(204)]`.

**Tareas:**

1. Revisar `THtracker.API/Extensions/ResultExtensions.cs`:
   - Añadir sobrecarga o lógica: si `T` es `Unit` (o un tipo sentinela interno) y `IsSuccess`, devolver `NoContentResult`.
   - Alternativa: no usar `ToActionResult` para DELETE y en cada acción `if (result.IsSuccess) return NoContent();`.
2. Afectados típicos: `UsersController.Delete`, `CategoriesController.Delete`, `ActivitiesController.Delete`, `RolesController.Delete`, y cualquier otro DELETE que hoy mapee `Result<Unit>` a 200.
3. Mantener coherencia con acciones que ya hacen `NoContent()` explícito (`TaskLists`, `Tasks`, `UserSessions`, etc.).

**Criterio de hecho:** Tests de integración o unitarios de API esperan **204** en DELETE exitoso.

---

## 4. HSTS y cabeceras de seguridad

**Objetivo:** Cumplir política mínima: HTTPS reforzado y cabeceras OWASP básicas.

**Tareas:**

1. En `THtracker.API/Program.cs`:
   - `builder.Services.AddHsts(options => { ... });` con `Preload`, `IncludeSubDomains` y `MaxAge` acorde a política del producto.
   - Tras `UseHttpsRedirection()`, `app.UseHsts()` en entornos no-Development (o según estándar del equipo).
2. Añadir middleware propio o usar `app.Use(async (ctx, next) => { ... })` / política centralizada para:
   - `X-Content-Type-Options: nosniff`
   - `X-Frame-Options` o CSP frame-ancestors
   - `Content-Security-Policy` acorde al front (ajustar en iteración con el cliente web).
3. Verificar que no rompe Swagger en Development.

**Criterio de hecho:** Respuesta de cualquier endpoint en staging incluye HSTS (donde aplique) y cabeceras acordadas.

---

## 5. JWT, health y red

**Tareas:**

1. **TTL:** Alinear `AccessTokenExpirationMinutes` con política (p. ej. 15) en `.env` / `appsettings.Production.json`; no dejar 60 en producción si el estándar es 15.
2. **JwtProvider:** Migrar a `IOptions<JwtOptions>` en Infrastructure; registrar opciones en `DependencyInjection` de Infrastructure.
3. **Health:** Restringir `GET /health/details` (autenticación admin, red interna, o deshabilitar en producción pública).
4. **CORS:** Sustituir `AllowAnyHeader`/`AllowAnyMethod` por lista explícita si el front es fijo.
5. **AllowedHosts:** Valor explícito en producción (no `*`).

---

## 6. Contratos HTTP unificados

**Tareas:**

1. **Register vs Create user:** Decidir un contrato único para creación de usuario (201 + `CreatedAtAction` vs 201 + URL en header `Location`); alinear `AuthController.Register` con `UsersController.Create` en semántica.
2. **Refresh:** Sustituir `[FromBody] string refreshToken` por un record en body, p. ej. `RefreshTokenRequest` con propiedad `Token` (nombre final según convención del proyecto **sin** sufijo prohibido si aplica la regla estricta — usar nombre intencional tipo `RefreshTokenBody` solo si las reglas lo permiten; preferible un record de comando dedicado solo para el body del controlador o reutilizar estructura mínima).

---

## 7. ICurrentUserService y limpieza

**Tareas:**

1. Definir `ICurrentUserService` en Application (`GetUserId()`, `GetSessionId()` o equivalente).
2. Implementación en API con `IHttpContextAccessor`.
3. Sustituir usos directos de `GetUserId()` del `AuthorizedControllerBase` por el servicio **en handlers** donde sea viable, o mantener el base como fachada delgada que delega al servicio (una sola fuente de verdad).
4. Resolver `UsersController.Get` (TODO): implementar `GetAllUsersQuery` paginado o eliminar el endpoint hasta estar listo.

---

## Checklist de merge

- [ ] Behaviors de validación activos y probados  
- [ ] Listados paginados  
- [ ] DELETE → 204  
- [ ] HSTS + cabeceras mínimas  
- [ ] JWT/options + health/CORS/hosts revisados  
- [ ] Register/Refresh alineados con el resto de la API  
- [ ] `ICurrentUserService` o excepción documentada  

---

## Notas

- Este plan asume **.NET 10** y la estructura actual de capas (`API` / `Application` / `Infrastructure` / `Domain`).
- Tras cada bloque, ejecutar `dotnet test` y, si existe, pruebas de integración de controladores.
