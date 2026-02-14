# THtracker.Tests

Capa de tests alineada con [Clean Architecture](../../docs/CLEAN_ARCHITECTURE_DEFINITIONS.md).

## Estructura

```
THtracker.Tests/
├── Helpers/                    # Builders y utilidades compartidas
│   └── UserTestBuilder.cs
├── Unit/
│   ├── Domain/                  # Unit tests del núcleo (entidades, reglas)
│   │   └── Entities/
│   ├── Application/            # Tests de casos de uso y validadores
│   │   └── Users/
│   ├── Infrastructure/          # Tests de repositorios (InMemory)
│   │   └── Repositories/
│   └── Presentation/            # Tests de controladores
│       └── Controllers/
└── THtracker.Tests.csproj
```

## Clasificación (CLEAN_ARCHITECTURE_DEFINITIONS.md §12)

| Capa           | Tipo de test   | Ubicación              |
|----------------|----------------|------------------------|
| Domain         | Unit Tests     | `Unit/Domain/`         |
| Application    | Use Case Tests | `Unit/Application/`    |
| Infrastructure | Integration    | `Unit/Infrastructure/` |
| Presentation   | API/UI Tests   | `Unit/Presentation/`   |

## Convenciones

- **Nombres de tests**: `Method_ShouldResult_WhenCondition` o `Should_ExpectedBehavior_WhenCondition`.
- **Traits**: `[Trait("Category", "Unit")]` y `[Trait("Layer", "Application")]` para filtrar por capa o categoría.
- **Arrange/Act/Assert**: Una sola idea por test; datos mínimos necesarios.
- **Helpers**: Objetos con estado controlado (p. ej. `UserTestBuilder`) en `Helpers/` para evitar reflexión repetida.

## Ejecución

```bash
dotnet test THtracker.Tests/THtracker.Tests.csproj
# Por capa:
dotnet test --filter "Layer=Application"
```
