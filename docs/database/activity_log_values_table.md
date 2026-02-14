# Tabla: activity_log_values

## Resumen
Almacena los valores específicos registrados para los campos personalizados definidos en `activity_value_definitions` para un log dato.

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `ActivityLogValue` en `THtracker.Domain.Entities`.
- **Unicidad**: Solo puede existir un valor para cada combinación de log y definición de campo.

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. |
| `activity_log_id` | UUID | Sí | - | - | Log de actividad asociado. |
| `value_definition_id` | UUID | Sí | - | - | Definición del campo asociado. |
| `value` | TEXT | Sí | - | - | El valor registrado (en formato texto). |

## Mapeo de Infraestructura (EF Core)
Configuración en `ActivityLogValueConfiguration.cs`:
- **Nombre de Tabla**: `activity_log_values`
- **Clave Primaria**: `Id`

## Repositorio
Gestionado por `ActivityLogValueRepository.cs`.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **UQ**: `UNIQUE (activity_log_id, value_definition_id)` - Evita duplicidad de campos en un mismo log.
- **FK**: `activity_log_id` -> `activity_logs(id)` con `ON DELETE CASCADE`.
- **FK**: `value_definition_id` -> `activity_value_definitions(id)` con `ON DELETE RESTRICT`.
