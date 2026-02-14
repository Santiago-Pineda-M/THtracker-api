# Tabla: categories

## Resumen
La tabla `categories` permite a los usuarios organizar sus actividades en grupos lógicos (ej. Trabajo, Deporte, Ocio).

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `Category` en `THtracker.Domain.Entities`.
- **Propiedad**: Cada categoría pertenece a un usuario específico.

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. |
| `user_id` | UUID | Sí | - | - | Referencia al usuario creador. |
| `name` | TEXT | Sí | - | - | Nombre de la categoría (Máx 100 caracteres en código). |
| `created_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de creación. |
| `updated_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de última modificación. |

## Mapeo de Infraestructura (EF Core)
Configuración en `CategoryConfiguration.cs`:
- **Nombre de Tabla**: `categories`
- **Clave Primaria**: `Id`
- **Restricciones**: `Name` tiene un máximo de 100 caracteres.

## Repositorio
Gestionado por `CategoryRepository.cs`.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **FK**: `user_id` -> `users(id)` con `ON DELETE CASCADE`.
