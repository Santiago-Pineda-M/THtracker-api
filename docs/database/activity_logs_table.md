# Tabla: activity_logs

## Resumen
La tabla `activity_logs` registra las instancias reales de ejecución de una actividad por parte de un usuario.

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `ActivityLog` en `THtracker.Domain.Entities`.
- **Rango Temporal**: Define el inicio (`StartedAt`) y el fin (`EndedAt`) opcional de una actividad.

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. |
| `activity_id` | UUID | Sí | - | - | Actividad a la que pertenece este log. |
| `started_at` | TIMESTAMP | Sí | `NOW()` | - | Fecha y hora de inicio. |
| `ended_at` | TIMESTAMP | No | - | - | Fecha y hora de fin. Puede ser nulo si la actividad sigue en curso. |

## Mapeo de Infraestructura (EF Core)
Configuración en `ActivityLogConfiguration.cs`:
- **Nombre de Tabla**: `activity_logs`
- **Clave Primaria**: `Id`

## Repositorio
Gestionado por `ActivityLogRepository.cs`. Incluye lógica para calcular duraciones y filtrar por intervalos.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **FK**: `activity_id` -> `activities(id)` con `ON DELETE CASCADE`.
