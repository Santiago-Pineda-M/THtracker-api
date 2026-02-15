# Ruta de uso del usuario – THtracker

Este documento describe el recorrido del usuario desde el **registro** hasta **consultar sus actividades con sus detalles**, pasando por **registrar** y **editar** actividades y sus logs. Se basa en el esquema SQL de referencia y en los controladores y casos de uso del proyecto.

---

## 1. Esquema SQL y modelo de datos (resumen)

El archivo `docs/database/db.sql` define las tablas principales y sus relaciones:

| Tabla | Propósito | Relación |
|-------|-----------|----------|
| **users** | Usuarios del sistema (id, name, email, timestamps) | Raíz |
| **user_sessions** | Sesiones activas (session_token, user_id, expires_at) | FK → users |
| **categories** | Categorías del usuario (name) | FK → users |
| **activities** | Actividades (name, allow_overlap, category_id) | FK → users, categories |
| **activity_value_definitions** | Definiciones de valores medibles por actividad (nombre, tipo, unidad, mín/máx) | FK → activities |
| **activity_logs** | Registros de “cuándo” se hizo la actividad (started_at, ended_at) | FK → activities |
| **activity_log_values** | Valores concretos por registro (value_definition_id + value) | FK → activity_logs, activity_value_definitions |
| **roles** / **user_roles** | Roles y asignación a usuarios | FK → users, roles |

**Flujo de datos:**

- Un **usuario** tiene **categorías** y **actividades** (cada actividad pertenece a una categoría).
- Cada **actividad** puede tener **definiciones de valor** (ej. “Cantidad”, “Notas”).
- Cada vez que el usuario “hace” la actividad se crea un **activity_log** (inicio/fin).
- En cada **activity_log** se pueden guardar **activity_log_values** (un valor por definición, ej. cantidad = 5, notas = "bien").

En el código C# se usan además tablas de autenticación (por ejemplo `user_logins`, `refresh_tokens`) que extienden este modelo para login con contraseña y JWT.

---

## 2. Ruta de uso completa (paso a paso)

A continuación se describe la **ruta del usuario** en el orden lógico de uso, con los endpoints de la API que existen hoy.

### 2.1 Registro

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 1 | Registrar usuario | `POST /api/v1/auth/register` | Body: `{ "name", "email", "password" }`. Crea el usuario en `users` (y en el modelo real también contraseña hasheada). Respuesta: `{ "userId": "guid" }`. |

- **SQL:** Se inserta una fila en `users`.
- No hay sesión aún; el usuario debe hacer **login** para obtener tokens.

---

### 2.2 Autenticación (login)

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 2 | Iniciar sesión | `POST /api/v1/auth/login` | Body: email, contraseña e info de dispositivo. Respuesta: `TokenResponse` (access token, refresh token, fechas de expiración). |
| (opcional) | Renovar token | `POST /api/v1/auth/refresh` | Body: `{ "refreshToken": "..." }`. Devuelve nuevos access + refresh token. |
| (opcional) | Login social | `POST /api/v1/auth/social-login` | Autenticación con proveedor externo (Google, etc.). |

- A partir de aquí, **todas** las peticiones a categorías, actividades, definiciones y activity-logs deben enviar el **access token** en el header:  
  `Authorization: Bearer <access_token>`.
- El `AuthorizedControllerBase` obtiene el `userId` del JWT (claim `NameIdentifier`) para filtrar recursos por dueño.

---

### 2.3 Categorías (necesarias para crear actividades)

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 3 | Listar categorías | `GET /api/v1/categories` | Lista las categorías del usuario autenticado. |
| 4 | Crear categoría | `POST /api/v1/categories` | Body: `{ "name": "..." }`. Crea una categoría asociada al usuario. |
| (opcional) | Ver una categoría | `GET /api/v1/categories/{id}` | Solo si es del usuario. |
| (opcional) | Editar / eliminar categoría | `PUT /api/v1/categories/{id}`, `DELETE /api/v1/categories/{id}` | Actualizar nombre o borrar (solo dueño). |

- **SQL:** Inserciones/actualizaciones en `categories` con `user_id`.

---

### 2.4 Actividades (crear y editar)

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 5 | Listar actividades | `GET /api/v1/activities` | Lista las actividades del usuario (con `userId`, `categoryId`, `name`, `allowOverlap`). |
| 6 | Crear actividad | `POST /api/v1/activities` | Body: `{ "name", "categoryId", "allowOverlap" }`. La actividad queda ligada al usuario y a la categoría. Respuesta: `ActivityResponse` y header `Location` a `GET .../activities/{id}`. |
| 7 | Ver una actividad | `GET /api/v1/activities/{id}` | Solo si es del usuario; si no, 403. |
| 8 | Editar actividad | `PUT /api/v1/activities/{id}` | Body: `{ "name", "allowOverlap" }`. Solo dueño. |
| (opcional) | Eliminar actividad | `DELETE /api/v1/activities/{id}` | Solo dueño. |

- **SQL:** Inserciones/actualizaciones en `activities` (`user_id`, `category_id`, `name`, `allow_overlap`).

---

### 2.5 Definiciones de valores (opcional, para “detalles” por registro)

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 9 | Listar definiciones de una actividad | `GET /api/v1/activities/{activityId}/definitions` | Devuelve las definiciones de valores (nombre, tipo, unidad, obligatorio, mín/máx) de esa actividad. Solo si el usuario es dueño de la actividad. |
| 10 | Crear definición de valor | `POST /api/v1/activities/{activityId}/definitions` | Body: nombre, tipo (ej. Number, Boolean, Time), isRequired, unit, minValue, maxValue. Crea una definición asociada a la actividad. |

- **SQL:** Tabla `activity_value_definitions` con `activity_id`. Estos IDs se usarán después al guardar valores en cada log.

---

### 2.6 Registrar la actividad (iniciar y detener logs)

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 11 | Iniciar registro (empezar a “hacer” la actividad) | `POST /api/v1/activity-logs/start` | Body: `{ "activityId": "guid" }`. Crea un nuevo `activity_log` con `started_at = now`, `ended_at = null`. Respuesta: `ActivityLogResponse` (id, activityId, startedAt, endedAt) y `Location: /api/v1/activity-logs/{id}`. Se aplican reglas de solapamiento según `allow_overlap`. |
| 12 | Detener registro | `POST /api/v1/activity-logs/{id}/stop` | Fija `ended_at = now` en ese log. Respuesta incluye duración en minutos. Solo si el log es de una actividad del usuario. |

- **SQL:** Inserciones en `activity_logs`; actualización de `ended_at` al parar.

---

### 2.7 Editar un registro y añadir “detalles” (valores)

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 13 | Actualizar período del log | `PUT /api/v1/activity-logs/{id}` | Body: `startedAt`, `endedAt`. Corrige fechas de inicio/fin del registro. Solo si el log pertenece a una actividad del usuario. |
| 14 | Añadir valores al registro | `POST /api/v1/activity-logs/{id}/values` | Body: array de `{ "valueDefinitionId", "value" }`. Crea filas en `activity_log_values` (una por definición). Los tipos se validan según la definición (Number, Boolean, Time, etc.). |

- **SQL:** Actualización de `activity_logs`; inserciones en `activity_log_values` (`activity_log_id`, `value_definition_id`, `value`).

---

### 2.8 Consultar actividades con sus detalles

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 15 | Consultar mis actividades | `GET /api/v1/activities` | Lista todas las actividades del usuario (con categoryId, name, allowOverlap). |
| 16 | Consultar una actividad | `GET /api/v1/activities/{id}` | Detalle de una actividad (misma estructura). |
| 17 | Consultar definiciones de la actividad | `GET /api/v1/activities/{activityId}/definitions` | Lista de “qué” se puede medir en esa actividad (nombre, tipo, unidad, etc.). |

| Paso | Acción | Endpoint | Descripción |
|------|--------|----------|-------------|
| 18 | Listar registros de una actividad | `GET /api/v1/activity-logs?activityId={guid}` | Lista los logs de esa actividad (solo si es dueño). |
| 19 | Obtener un registro por ID | `GET /api/v1/activity-logs/{id}` | Un log concreto con fechas y duración (solo si pertenece a una actividad del usuario). |

La ruta “consultar sus actividades con sus detalles” queda cubierta: actividades, definiciones de valor y registros (logs) con sus fechas; los valores concretos por log se gestionan con `POST .../values` y pueden ampliarse en el futuro con un DTO que incluya valores en el GET del log si se desea.

---

## 3. Diagrama de flujo resumido

```
Registro → Login → [Refresh si hace falta]
    ↓
Categorías: GET lista → POST crear (si no hay)
    ↓
Actividades: GET lista → POST crear (con categoryId) → GET por id / PUT editar
    ↓
Definiciones (opcional): GET por activityId → POST crear (nombre, tipo, unidad…)
    ↓
Registrar actividad:
    POST activity-logs/start { activityId }
    → POST activity-logs/{id}/stop
    → PUT activity-logs/{id} (editar startedAt/endedAt)
    → POST activity-logs/{id}/values (añadir cantidad, notas, etc.)
    ↓
Consultar:
    GET activities (+ GET activities/{id})
    GET activities/{activityId}/definitions (+ GET .../definitions/{definitionId})
    GET activity-logs?activityId=... y GET activity-logs/{id}
```

---

## 4. Resumen por tablas SQL implicadas en la ruta

| Fase | Tablas tocadas |
|------|-----------------|
| Registro | `users` (y en el modelo real, almacén de contraseña) |
| Login | `users`, `user_sessions`, `refresh_tokens` (o equivalente) |
| Categorías | `categories` |
| Actividades | `activities` |
| Definiciones | `activity_value_definitions` |
| Registrar / editar uso | `activity_logs`, `activity_log_values` |
| Consultar actividades | Lectura de `activities`, `categories`, `activity_value_definitions`; consulta de logs pendiente de implementar en la API |

La API expone GET de activity-logs (lista por `activityId` y por `id`), por lo que la ruta queda cerrada desde el registro hasta “consultar sus actividades con sus detalles” (incluyendo registros).
