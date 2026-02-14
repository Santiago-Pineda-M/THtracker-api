# Seguridad y Roles

## Tabla: roles
Almacena los diferentes roles definidos en el sistema (ej. Admin, User).

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único. |
| `name` | TEXT | Sí | - | - | Nombre del rol (Máx 50 caracteres, Único). |

## Tabla: permissions
Almacena los permisos granulares que pueden asignarse a los roles.

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | - | `Guid.NewGuid()` | Identificador único. |
| `name` | TEXT | Sí | - | - | Nombre del permiso (Máx 100 caracteres, Único). |
| `description` | TEXT | No | - | - | Descripción del propósito del permiso. |

## Tabla: user_roles (Many-to-Many)
Asocia usuarios con sus respectivos roles.

| Columna | Tipo de Dato | Obligatorio | Descripción |
| :--- | :--- | :--- | :--- |
| `user_id` | UUID | Sí | Referencia a `users(id)`. |
| `role_id` | UUID | Sí | Referencia a `roles(id)`. |

## Tabla: role_permissions (Many-to-Many)
Asocia roles con sus respectivos permisos granulares.

| Columna | Tipo de Dato | Obligatorio | Descripción |
| :--- | :--- | :--- | :--- |
| `role_id` | UUID | Sí | Referencia a `roles(id)`. |
| `permission_id` | UUID | Sí | Referencia a `permissions(id)`. |

## Infraestructura
- **Configuraciones**: `RoleConfiguration.cs`, `PermissionConfiguration.cs`, `UserConfiguration.cs`.
- **Repositorios**: `RoleRepository.cs`, `PermissionRepository.cs`, `UserRoleRepository.cs`.
