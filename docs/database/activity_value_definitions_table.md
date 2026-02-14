# Tabla: activity_value_definitions

## Resumen
Define campos personalizados (metadata) que pueden ser asociados a los logs de una actividad específica (ej. "Km recorridos", "Estado de ánimo").

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `ActivityValueDefinition` en `THtracker.Domain.Entities`.
- **Tipado**: Define el tipo de dato esperado (`ValueType`) y si es obligatorio para el log.

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. |
| `activity_id` | UUID | Sí | - | - | Actividad a la que pertenece esta definición. |
| `name` | TEXT | Sí | - | - | Nombre del campo (Máx 100 caracteres en código). |
| `value_type` | TEXT | Sí | - | - | Tipo de dato (e.g., Number, Text, Boolean). |
| `is_required` | BOOLEAN | Sí | `FALSE` | `false` | Si el valor es obligatorio en cada log. |
| `unit` | TEXT | No | - | - | Unidad de medida opcional. |
| `min_value` | TEXT | No | - | - | Valor mínimo permitido (en formato texto para flexibilidad). |
| `max_value` | TEXT | No | - | - | Valor máximo permitido. |
| `created_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de creación. |
| `updated_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de última modificación. |

## Mapeo de Infraestructura (EF Core)
Configuración en `ActivityValueDefinitionConfiguration.cs`:
- **Nombre de Tabla**: `activity_value_definitions`
- **Clave Primaria**: `Id`

## Repositorio
Gestionado por `ActivityValueDefinitionRepository.cs`.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **FK**: `activity_id` -> `activities(id)` con `ON DELETE CASCADE`.
