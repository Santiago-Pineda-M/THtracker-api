# Tabla: user_sessions

## Resumen
La tabla `user_sessions` gestiona las sesiones activas de los usuarios en el sistema, permitiendo el rastreo de tokens de sesión y sus fechas de expiración.

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `UserSession` en `THtracker.Domain.Entities`.
- **Identidad**: Cada sesión se identifica por un `Id` único (UUID).
- **Estado**: Una sesión puede ser revocada manualmente (`IsActive = false`) o expirar naturalmente.

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. |
| `user_id` | UUID | Sí | - | - | Referencia al usuario dueño de la sesión. |
| `session_token` | TEXT | Sí | - | - | Token único de la sesión. |
| `created_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de inicio de la sesión. |
| `expires_at` | TIMESTAMP | Sí | - | - | Fecha de expiración programada. |
| `is_active` | BOOLEAN | Sí | `TRUE` | `true` | Indica si la sesión es válida. |

## Mapeo de Infraestructura (EF Core)
Configuración en `UserSessionConfiguration.cs`:
- **Nombre de Tabla**: `user_sessions`
- **Clave Primaria**: `Id`
- **Relaciones**:
  - `BelongsTo(User)`: Cada sesión pertenece a un único usuario.

## Repositorio
Gestionado por `UserSessionRepository.cs`.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **FK**: `user_id` -> `users(id)` con `ON DELETE CASCADE`.
- **UQ**: `UNIQUE (session_token)`
