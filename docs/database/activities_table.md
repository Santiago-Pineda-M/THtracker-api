# Tabla: activities

## Resumen
La tabla `activities` define los diferentes tipos de tareas o eventos que un usuario puede rastrear.

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `Activity` en `THtracker.Domain.Entities`.
- **Reglas**: Define si se permite el solapamiento de tiempos para esta actividad específica (`AllowOverlap`).

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. |
| `user_id` | UUID | Sí | - | - | Referencia al usuario dueño. |
| `category_id` | UUID | Sí | - | - | Categoría a la que pertenece la actividad. |
| `name` | TEXT | Sí | - | - | Nombre de la actividad (Máx 100 caracteres en código). |
| `allow_overlap` | BOOLEAN | Sí | `FALSE` | `false` | Indica si logs de esta actividad pueden solaparse. |
| `created_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de creación. |
| `updated_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de última modificación. |

## Mapeo de Infraestructura (EF Core)
Configuración en `ActivityConfiguration.cs`:
- **Nombre de Tabla**: `activities`
- **Clave Primaria**: `Id`
- **Restricciones**: `Name` (Máx 100).

## Repositorio
Gestionado por `ActivityRepository.cs`.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **FK**: `user_id` -> `users(id)` con `ON DELETE CASCADE`.
- **FK**: `category_id` -> `categories(id)` con `ON DELETE CASCADE`.
