# Tabla: Users

## Resumen
La tabla `users` almacena la información de los usuarios registrados en el sistema. Es la entidad central para la autenticación y la asociación de datos personales.

## Relación con la Lógica de Negocio
- **Entidad de Dominio**: Representada por la clase `User` en `THtracker.Domain.Entities`.
- **Identidad**: Cada usuario se identifica por un `Id` único (UUID) generado por la base de datos por defecto (`uuid_generate_v4()`).
- **Validaciones**:
  - `Name`: Obligatorio, longitud máxima de 100 caracteres.
  - `Email`: Obligatorio, único en el sistema, longitud máxima de 150 caracteres.

## Estructura de la Tabla (SQL)

| Columna | Tipo de Dato | Obligatorio | Por Defecto (SQL) | Generación (Código) | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `id` | UUID | Sí | `uuid_generate_v4()` | `Guid.NewGuid()` | Identificador único primario. Generado en el constructor de `User`. |
| `name` | TEXT | Sí | - | - | Nombre completo o nombre de usuario. |
| `email` | TEXT | Sí | - | - | Correo electrónico único del usuario. |
| `created_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de creación. Asignada en el constructor. |
| `updated_at` | TIMESTAMP | Sí | `NOW()` | `DateTime.UtcNow` | Fecha de actualización. Gestionada en el código. |

## Mapeo de Infraestructura (EF Core)
La configuración de la entidad se encuentra en `UserConfiguration.cs`:
- **Nombre de Tabla**: `users`
- **Clave Primaria**: `Id`
- **Índices**: Único sobre `Email`.
- **Relaciones**:
  - `HasMany(Logins)`: Relación uno a muchos con eliminaciones en cascada.
  - `HasMany(RefreshTokens)`: Relación uno a muchos con eliminaciones en cascada.
  - `HasMany(Roles)`: Relación muchos a muchos mediante la tabla `user_roles`.

## Repositorio
La gestión de datos se realiza a través de `UserRepository.cs`, el cual implementa `IUserRepository`.
- **Operaciones clave**: `GetByEmailAsync`, `ExistsByEmailAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
- **Carga de Datos**: Incluye automáticamente los roles y permisos asociados al recuperar un usuario por ID o Email.

## Índices (Base de Datos)
- **PK**: `PRIMARY KEY (id)`
- **UQ**: `UNIQUE (email)`
