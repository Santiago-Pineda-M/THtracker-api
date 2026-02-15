# Documentación de controladores – THtracker API

Resumen de los controladores de la API, rutas, métodos HTTP, autorización y respuestas.

---

## 1. AuthController

**Ruta base:** `api/v1/auth`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| POST | `/api/v1/auth/register` | No | Registro de usuario | 200 + `{ UserId }`, 400 |
| POST | `/api/v1/auth/login` | No | Login (email, password) | 200 + TokenResponse, 401 |
| POST | `/api/v1/auth/refresh` | No | Renovar access token (body JSON: `{ "refreshToken": "..." }`) | 200 + TokenResponse, 400 |
| POST | `/api/v1/auth/social-login` | No | Login social (por definir) | 200, 400 |

**Notas:**
- Inyección por constructor. Errores unificados con `ApiErrorResponse`.
- Login devuelve 401 cuando las credenciales son inválidas.

---

## 2. UsersController

**Ruta base:** `api/v1/users`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/users/me` | Usuario | Usuario autenticado | 200, 401, 404 |
| PUT | `/api/v1/users/me` | Usuario | Actualizar usuario autenticado | 200, 401, 404 |
| GET | `/api/v1/users` | Admin | Listar usuarios | 200, 403 |
| GET | `/api/v1/users/{id}` | Admin | Usuario por ID | 200, 403, 404 |
| POST | `/api/v1/users` | Admin | Crear usuario | 201 + Location, 400, 403 |
| PUT | `/api/v1/users/{id}` | Admin | Actualizar usuario | 200, 403, 404 |
| DELETE | `/api/v1/users/{id}` | Admin | Eliminar usuario | 204, 403, 404 |

**Notas:**
- CRUD consistente: GET colección, GET por id, POST, PUT por id, DELETE por id.
- Create devuelve `CreatedAtAction(GetById, id)` y el recurso en el body.

---

## 3. RolesController

**Ruta base:** `api/v1/roles`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/roles` | Admin | Listar roles | 200, 401, 403 |
| GET | `/api/v1/roles/{id}` | Admin | Rol por ID (guid) | 200, 401, 403, 404 |
| GET | `/api/v1/roles/by-name/{name}` | Admin | Rol por nombre (conveniencia) | 200, 401, 403, 404 |
| POST | `/api/v1/roles` | Admin | Crear rol | 201 + RoleResponse en body + Location a GET por id, 400, 401, 403 |
| DELETE | `/api/v1/roles/{id}` | Admin | Eliminar rol por ID | 204, 401, 403, 404 |

**Notas:**
- Recurso identificado por ID; GET por nombre en `by-name/{name}` para evitar conflicto de rutas.
- Create devuelve `RoleResponse` (Id, Name) y `Location` a `GET /api/v1/roles/{id}`. Errores con `ApiErrorResponse`.

---

## 4. UserRolesController

**Ruta base:** `api/v1/users/{userId}/roles`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/users/{userId}/roles` | Admin | Roles del usuario | 200, 403 |
| POST | `/api/v1/users/{userId}/roles/{roleId}` | Admin | Asignar rol | 204, 403 |
| DELETE | `/api/v1/users/{userId}/roles/{roleId}` | Admin | Quitar rol | 204, 403 |
| PUT | `/api/v1/users/{userId}/roles` | Admin | Reemplazar roles por uno (body: nombre) | 204, 403, 404 |

**Notas:**
- PUT recibe body JSON: `SetUserRolesRequest` con `RoleNames` (lista de nombres). Reemplaza todos los roles del usuario de forma atómica. 404 si algún rol no existe.

---

## 5. CategoriesController

**Ruta base:** `api/v1/categories`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/categories` | Usuario | Listar categorías del usuario | 200, 401 |
| GET | `/api/v1/categories/{id}` | Usuario | Categoría por ID (solo dueño) | 200, 401, 403, 404 |
| POST | `/api/v1/categories` | Usuario | Crear categoría | 201 + Location, 400, 401 |
| PUT | `/api/v1/categories/{id}` | Usuario | Actualizar (solo dueño) | 200, 401, 403, 404 |
| DELETE | `/api/v1/categories/{id}` | Usuario | Eliminar (solo dueño) | 204, 401, 403, 404 |

**Notas:**
- CRUD completo y consistente. Filtro por usuario en GET lista y comprobación de dueño en GetById/Update/Delete. Create/Update con try/catch y `ApiErrorResponse` en 400.
- Hereda `GetUserId()` de `AuthorizedControllerBase`.

---

## 6. ActivitiesController

**Ruta base:** `api/v1/activities`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/activities` | Usuario | Listar actividades del usuario | 200, 401 |
| GET | `/api/v1/activities/{id}` | Usuario | Actividad por ID (solo dueño) | 200, 401, 403, 404 |
| POST | `/api/v1/activities` | Usuario | Crear actividad | 201, 400, 401 |
| PUT | `/api/v1/activities/{id}` | Usuario | Actualizar (solo dueño) | 200, 400, 401, 403, 404 |
| DELETE | `/api/v1/activities/{id}` | Usuario | Eliminar (solo dueño) | 204, 401, 403, 404 |

**Notas:**
- Create y Update con try/catch y `ApiErrorResponse` en 400. CRUD consistente con resto de la API.

---

## 7. ActivityLogsController

**Ruta base:** `api/v1/activity-logs`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/activity-logs?activityId={guid}` | Usuario | Listar registros de una actividad | 200, 401 |
| GET | `/api/v1/activity-logs/{id}` | Usuario | Registro por ID (solo dueño de la actividad) | 200, 401, 404 |
| POST | `/api/v1/activity-logs/start` | Usuario | Iniciar registro de actividad | 201 + ActivityLogResponse + Location a GET por id, 400, 401 |
| POST | `/api/v1/activity-logs/{id}/stop` | Usuario | Detener registro | 200, 400, 401, 404 |
| PUT | `/api/v1/activity-logs/{id}` | Usuario | Actualizar registro (startedAt, endedAt) | 200, 400, 401, 404 |
| POST | `/api/v1/activity-logs/{id}/values` | Usuario | Guardar valores del registro | 200, 400, 401, 404 |

**Notas:**
- GET lista con query `activityId` obligatorio. GET por id comprueba que el log pertenezca a una actividad del usuario.
- Start devuelve 201 Created con cuerpo y `Location` a `GET /api/v1/activity-logs/{id}`.

---

## 8. ActivityValueDefinitionsController

**Ruta base:** `api/v1/activities/{activityId}/definitions`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/activities/{activityId}/definitions` | Usuario | Listar definiciones de la actividad | 200, 400, 401, 403 |
| GET | `/api/v1/activities/{activityId}/definitions/{definitionId}` | Usuario | Definición por ID (solo dueño de la actividad) | 200, 401, 403, 404 |
| POST | `/api/v1/activities/{activityId}/definitions` | Usuario | Crear definición | 201 + body + Location a GET por definitionId, 400, 401, 403 |

**Notas:**
- Create devuelve 201 con `Location` a `GET .../definitions/{definitionId}`. GET por id para consultar una definición concreta.

---

## 9. HealthController

**Ruta base:** `api/v1/health`

| Método | Ruta | Auth | Descripción | Respuestas |
|--------|------|------|-------------|------------|
| GET | `/api/v1/health` | No | Estado del servicio | 200 + status, service, timestamp |

**Notas:**
- Sin comprobación de dependencias (BD, etc.). Podría devolver 503 si no healthy.

---

## Resumen de patrones (actualizado)

| Aspecto | Estado |
|---------|--------|
| Versionado | Todos con `api/v1/` |
| Identificador de recurso | Roles: GET por `{id:guid}` y `by-name/{name}`; resto por `{id}` |
| Códigos HTTP | 200/201/204/400/401/403/404; ProducesResponseType documentados |
| Cuerpo de error | Unificado con `ApiErrorResponse.Message` (PascalCase) |
| Inyección | Todos por constructor |
| Documentación XML | Summary, param, returns y response code en todos los endpoints |
| GetUserId | Centralizado en `AuthorizedControllerBase` para controladores autorizados |
| REST | Create → 201 + Location al recurso creado (GetById); GET lista y GET por id donde aplica |
