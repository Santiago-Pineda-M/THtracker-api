# Alineación base de datos, entidades y API – THtracker

Este documento describe la relación entre el esquema SQL de referencia (`docs/database/db.sql`), las entidades del dominio (C#) y lo que exponen los controladores (DTOs). Se indican diferencias y criterios de consistencia.

---

## 1. Esquema SQL de referencia vs persistencia real

El archivo `docs/database/db.sql` define un esquema **PostgreSQL** con tablas mínimas para el dominio de negocio. La aplicación usa **Entity Framework Core** con **SQLite** (u otro proveedor según configuración); las tablas y columnas reales vienen de las **migraciones** en `THtracker.Infrastructure/Migrations/` y de las **configuraciones** en `Persistence/Configurations/`.

### Tablas que coinciden con el SQL de referencia

| Tabla SQL        | Entidad C#        | Observaciones |
|------------------|-------------------|----------------|
| users            | User              | En el modelo real se añaden `password_hash`, `security_stamp` (auth). |
| categories       | Category          | Coincide (id, user_id, name, created_at, updated_at). |
| activities       | Activity          | Coincide. |
| activity_value_definitions | ActivityValueDefinition | Coincide. |
| activity_logs    | ActivityLog       | Coincide. |
| activity_log_values | ActivityLogValue | Coincide. |
| roles            | Role              | Coincide (id, name). |
| user_roles       | UserRole          | Coincide. |

### Tablas/columnas solo en el modelo C# (no en db.sql)

- **user_sessions** (UserSession): sesiones activas con token y expiración.
- **user_logins** (UserLogin): logins externos (proveedor, provider_key, etc.) para login social.
- **refresh_tokens** (RefreshToken): tokens de refresco JWT.
- En **users**: columnas para contraseña hasheada y security stamp.

El SQL de referencia no incluye autenticación JWT/sesiones; la API sí las usa. Para un despliegue PostgreSQL, el esquema de referencia puede extenderse con estas tablas/columnas o generarse desde las migraciones de EF.

---

## 2. Entidades vs DTOs expuestos por la API

Los controladores **no** devuelven entidades del dominio directamente; usan DTOs de respuesta para no exponer datos internos (ej. permisos, colecciones navegables) y para un contrato estable.

| Recurso      | Entidad      | DTO de respuesta        | Campos expuestos |
|-------------|--------------|--------------------------|-------------------|
| Usuario     | User         | UserDto                  | Id, Name, Email (sin PasswordHash, SecurityStamp, Roles/Logins en el DTO estándar). |
| Categoría   | Category     | CategoryResponse         | Id, UserId, Name (sin CreatedAt/UpdatedAt si no se definen en el DTO). |
| Actividad   | Activity     | ActivityResponse         | Id, UserId, CategoryId, Name, AllowOverlap. |
| Rol         | Role         | RoleResponse             | Id, Name (sin Permissions). |
| Activity log| ActivityLog  | ActivityLogResponse      | Id, ActivityId, StartedAt, EndedAt, DurationMinutes. |
| Value def.  | ActivityValueDefinition | ActivityValueDefinitionResponse | Id, ActivityId, Name, ValueType, IsRequired, Unit, MinValue, MaxValue. |

Criterio: los DTOs alinean con lo que el cliente necesita; las entidades pueden tener más campos para reglas de negocio y persistencia.

---

## 3. Atributos que no se exponen en la API

- **users**: `PasswordHash`, `SecurityStamp` (nunca en respuestas). Roles/Logins/RefreshTokens solo si hay endpoints específicos que los devuelvan en forma controlada.
- **categories / activities**: `CreatedAt`, `UpdatedAt` no están en los DTOs mostrados; pueden añadirse si se desea en el contrato.
- **roles**: `Permissions` (colección) no se expone en `RoleResponse`; solo Id y Name.

No hay inconsistencia de “base de datos vs API” por omitir estos campos: es intencional por seguridad y simplicidad del contrato.

---

## 4. Consistencia de nombres y tipos

- **IDs**: Guid en C# ↔ UUID en SQL; coherente en toda la API.
- **Nombres de tablas**: en SQL snake_case (ej. `activity_logs`); en C# PascalCase (ej. `ActivityLog`). EF los mapea mediante configuraciones (ToTable).
- **Propiedades JSON**: la API usa PascalCase por defecto (ej. `Message` en `ApiErrorResponse`); el SQL no define contrato JSON.

---

## 5. Resumen

- El **SQL de referencia** describe el núcleo del dominio; la **persistencia real** añade tablas/columnas de autenticación (users ampliado, user_sessions, user_logins, refresh_tokens).
- Las **entidades** reflejan el modelo de dominio; los **controladores** exponen **DTOs** que alinean con el esquema lógico (recursos REST) sin filtrar por columnas de BD que no deban verse.
- No hay desalineación crítica: las diferencias son ampliación del modelo (auth) y ocultación voluntaria de datos sensibles o internos en la API.
