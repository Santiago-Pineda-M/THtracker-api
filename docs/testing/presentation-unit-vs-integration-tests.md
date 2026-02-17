# Tests Unitarios vs Tests de Integración - Capa de Presentación

## 📋 Resumen Ejecutivo

En este proyecto existen **DOS tipos de tests** para la capa de presentación:

1. **Tests Unitarios** (`Unit/Presentation/`) - Prueban Controllers de forma aislada con Mocks
2. **Tests de Integración** (`Integration/Presentation/`) - Prueban el flujo completo de la API

---

## 🔍 Comparación Rápida

| Aspecto | Tests Unitarios | Tests de Integración |
|---------|----------------|---------------------|
| **Ubicación** | `Unit/Presentation/Controllers/` | `Integration/Presentation/` |
| **Alcance** | Solo el Controller | Endpoint completo (HTTP → Controller → Use Case → Repository) |
| **Dependencias** | **Mocks** (Moq) | **Implementaciones reales** en memoria |
| **Velocidad** | ⚡ Muy rápido | 🚀 Rápido |
| **Aislamiento** | 🔒 Total | 🔒 Alto |
| **Realismo** | 📊 Bajo | 🎯 Alto |
| **Propósito** | Verificar lógica del Controller | Verificar integración completa |
| **HTTP** | ❌ No hay peticiones HTTP reales | ✅ Peticiones HTTP reales (simuladas) |
| **Autenticación** | ❌ No se prueba | ✅ Se prueba con TestAuthHandler |
| **Routing** | ❌ No se prueba | ✅ Se prueba |
| **Middleware** | ❌ No se ejecuta | ✅ Se ejecuta |

---

## 1️⃣ Tests Unitarios de Presentación

### 📍 Ubicación
```
THtracker.Tests/
└── Unit/
    └── Presentation/
        └── Controllers/
            └── UsersControllerTests.cs
```

### 🎯 Propósito
Verificar que el **Controller** maneja correctamente:
- Los resultados de los Use Cases (Success/Failure)
- La conversión a ActionResults apropiados (Ok, NotFound, BadRequest, etc.)
- El paso correcto de parámetros a los Use Cases

### 🔧 Características Clave

#### ✅ Usa Mocks (Moq)
```csharp
private readonly Mock<GetAllUsersUseCase> _getAllUsersMock;
private readonly Mock<GetUserByIdUseCase> _getUserByIdMock;
private readonly Mock<CreateUserUseCase> _createUserMock;
```

#### ✅ Prueba SOLO el Controller
```csharp
_controller = new UsersController(
    _getAllUsersMock.Object,
    _getUserByIdMock.Object,
    _createUserMock.Object,
    _updateUserMock.Object,
    _deleteUserMock.Object
);
```

#### ✅ No hay HTTP, no hay servidor
```csharp
// Llamada directa al método del controller
var result = await _controller.GetById(userId);

// No hay: await client.GetAsync("/api/v1/users/...")
```

### 📝 Ejemplo Completo

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using THtracker.API.Controllers.v1;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Common;
using Xunit;

namespace THtracker.Tests.Unit.Presentation.Controllers;

[Trait("Category", "Unit")]
[Trait("Layer", "Presentation")]
public class UsersControllerTests
{
    private readonly Mock<GetAllUsersUseCase> _getAllUsersMock;
    private readonly Mock<GetUserByIdUseCase> _getUserByIdMock;
    private readonly Mock<CreateUserUseCase> _createUserMock;
    private readonly Mock<UpdateUserUseCase> _updateUserMock;
    private readonly Mock<DeleteUserUseCase> _deleteUserMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        // Crear mocks de los Use Cases
        _getAllUsersMock = new Mock<GetAllUsersUseCase>(null!);
        _getUserByIdMock = new Mock<GetUserByIdUseCase>(null!);
        _createUserMock = new Mock<CreateUserUseCase>(null!, null!, null!);
        _updateUserMock = new Mock<UpdateUserUseCase>(null!, null!, null!);
        _deleteUserMock = new Mock<DeleteUserUseCase>(null!, null!);
        
        // Inyectar mocks en el controller
        _controller = new UsersController(
            _getAllUsersMock.Object,
            _getUserByIdMock.Object,
            _createUserMock.Object,
            _updateUserMock.Object,
            _deleteUserMock.Object
        );
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto(userId, "John Doe", "john@example.com");
        
        // Configurar el mock para devolver Success
        _getUserByIdMock
            .Setup(x => x.ExecuteAsync(userId))
            .ReturnsAsync(Result.Success(user));

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Configurar el mock para devolver Failure
        _getUserByIdMock
            .Setup(x => x.ExecuteAsync(userId))
            .ReturnsAsync(Result.Failure<UserDto>(
                new Error("NotFound", "User not found")
            ));

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest("Jane Smith", "jane@example.com");
        var createdUser = new UserDto(Guid.NewGuid(), "Jane Smith", "jane@example.com");
        
        _createUserMock
            .Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(createdUser));

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(UsersController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdUser.Id);
        createdResult.Value.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _deleteUserMock
            .Setup(x => x.ExecuteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
```

### 🎯 Qué se Prueba

✅ **Conversión de Result a ActionResult**
```csharp
// Result.Success → OkObjectResult
// Result.Failure (NotFound) → NotFoundObjectResult
// Result.Failure (ValidationError) → BadRequestObjectResult
// Result.Success (Create) → CreatedAtActionResult
// Result.Success (Delete) → NoContentResult
```

✅ **Paso correcto de parámetros**
```csharp
// Verifica que el controller llama al Use Case con los parámetros correctos
_getUserByIdMock.Verify(x => x.ExecuteAsync(userId), Times.Once);
```

✅ **Manejo de errores**
```csharp
// Verifica que diferentes tipos de errores se convierten correctamente
Error("NotFound", "...") → 404 NotFound
Error("ValidationError", "...") → 400 BadRequest
Error("Forbidden", "...") → 403 Forbidden
```

### ❌ Qué NO se Prueba

❌ Autenticación/Autorización (no hay middleware)  
❌ Routing (no hay peticiones HTTP)  
❌ Model Binding (no hay deserialización)  
❌ Validación de FluentValidation (no se ejecuta el pipeline)  
❌ Integración con Use Cases reales  
❌ Integración con Repositorios  

---

## 2️⃣ Tests de Integración de Presentación

### 📍 Ubicación
```
THtracker.Tests/
└── Integration/
    └── Presentation/
        ├── Support/
        │   ├── ApiWebApplicationFactory.cs
        │   └── TestAuthHandler.cs
        ├── CategoriesControllerIntegrationTests.cs
        ├── ActivityLogsControllerIntegrationTests.cs
        └── ... (más tests)
```

### 🎯 Propósito
Verificar el **flujo completo** de la API:
- Peticiones HTTP reales (simuladas)
- Autenticación y autorización
- Routing
- Middleware pipeline
- Controllers → Use Cases → Repositories
- Respuestas HTTP

### 🔧 Características Clave

#### ✅ Usa WebApplicationFactory
```csharp
private readonly ApiWebApplicationFactory _factory = new();
var client = _factory.CreateClient();
```

#### ✅ Peticiones HTTP reales (simuladas)
```csharp
var response = await client.GetAsync("/api/v1/categories");
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

#### ✅ Autenticación simulada
```csharp
client.DefaultRequestHeaders.Add(
    TestAuthHandler.HeaderUserId, 
    userId.ToString()
);
```

#### ✅ Repositorios en memoria
```csharp
// No mocks, sino implementaciones reales en memoria
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    // ... implementación real
}
```

### 📝 Ejemplo Completo

```csharp
using System.Net;
using FluentAssertions;
using Xunit;
using THtracker.Domain.Entities;
using THtracker.Tests.Integration.Presentation.Support;

namespace THtracker.Tests.Integration.Presentation;

[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class CategoriesControllerIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenNoAuth()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/v1/categories");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnForbidden_WhenNotOwner()
    {
        // Arrange
        var client = _factory.CreateClient();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        // Preparar datos en el repositorio en memoria
        var repo = _factory.Services.GetService(typeof(ICategoryRepository)) 
            as ICategoryRepository;
        var category = new Category(ownerId, "Cat 1");
        await repo!.AddAsync(category);
        
        // Autenticarse como otro usuario
        client.DefaultRequestHeaders.Add(
            TestAuthHandler.HeaderUserId, 
            otherUserId.ToString()
        );
        
        // Act
        var response = await client.GetAsync($"/api/v1/categories/{category.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOwner()
    {
        // Arrange
        var client = _factory.CreateClient();
        var ownerId = Guid.NewGuid();
        
        var repo = _factory.Services.GetService(typeof(ICategoryRepository)) 
            as ICategoryRepository;
        var category = new Category(ownerId, "Cat 1");
        await repo!.AddAsync(category);
        
        // Autenticarse como propietario
        client.DefaultRequestHeaders.Add(
            TestAuthHandler.HeaderUserId, 
            ownerId.ToString()
        );
        
        // Act
        var response = await client.GetAsync($"/api/v1/categories/{category.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### 🎯 Qué se Prueba

✅ **Autenticación**
```csharp
// Sin header → 401 Unauthorized
// Con header → Autenticado
```

✅ **Autorización**
```csharp
// Usuario no autorizado → 403 Forbidden
// Usuario autorizado → 200 OK
```

✅ **Routing**
```csharp
// URL correcta → Controller correcto
// URL incorrecta → 404 Not Found
```

✅ **Flujo completo**
```csharp
// HTTP Request → Middleware → Controller → Use Case → Repository → Response
```

✅ **Códigos de estado HTTP**
```csharp
// 200 OK, 201 Created, 204 No Content
// 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
```

---

## 🔄 Flujo Visual: Unitario vs Integración

### Tests Unitarios
```
┌─────────────────────────────────────────────┐
│  TEST UNITARIO                              │
├─────────────────────────────────────────────┤
│                                             │
│  Mock<GetUserByIdUseCase>                   │
│         │                                   │
│         │ .Setup(x => x.ExecuteAsync())     │
│         │ .ReturnsAsync(Result.Success())   │
│         │                                   │
│         ▼                                   │
│  UsersController                            │
│         │                                   │
│         │ await _useCase.ExecuteAsync()     │
│         │                                   │
│         ▼                                   │
│  return Ok(result)                          │
│         │                                   │
│         ▼                                   │
│  Assert: OkObjectResult                     │
│                                             │
│  ❌ NO HAY:                                 │
│     - HTTP Request                          │
│     - Autenticación                         │
│     - Routing                               │
│     - Middleware                            │
│     - Use Case real                         │
│     - Repository real                       │
│                                             │
└─────────────────────────────────────────────┘
```

### Tests de Integración
```
┌─────────────────────────────────────────────┐
│  TEST DE INTEGRACIÓN                        │
├─────────────────────────────────────────────┤
│                                             │
│  HttpClient                                 │
│         │                                   │
│         │ GET /api/v1/users/123             │
│         │ Header: X-Test-UserId             │
│         │                                   │
│         ▼                                   │
│  TestAuthHandler (Middleware)               │
│         │                                   │
│         ▼                                   │
│  Routing                                    │
│         │                                   │
│         ▼                                   │
│  Authorization (Middleware)                 │
│         │                                   │
│         ▼                                   │
│  UsersController (REAL)                     │
│         │                                   │
│         ▼                                   │
│  GetUserByIdUseCase (REAL)                  │
│         │                                   │
│         ▼                                   │
│  InMemoryUserRepository (REAL)              │
│         │                                   │
│         ▼                                   │
│  Dictionary<Guid, User> (en memoria)        │
│         │                                   │
│         ▼                                   │
│  HTTP Response                              │
│         │                                   │
│         ▼                                   │
│  Assert: StatusCode = 200 OK                │
│                                             │
│  ✅ SE PRUEBA TODO EL FLUJO                 │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 🎯 Cuándo Usar Cada Tipo

### Usa Tests Unitarios cuando:
- ✅ Quieres probar la **lógica del Controller** de forma aislada
- ✅ Quieres verificar la **conversión de Result a ActionResult**
- ✅ Quieres tests **extremadamente rápidos**
- ✅ Quieres **aislar completamente** el Controller de sus dependencias
- ✅ Quieres verificar el **manejo de diferentes tipos de errores**

### Usa Tests de Integración cuando:
- ✅ Quieres probar el **flujo completo** de un endpoint
- ✅ Quieres verificar **autenticación y autorización**
- ✅ Quieres verificar **routing**
- ✅ Quieres verificar la **integración entre capas**
- ✅ Quieres tests más **realistas**

### Estrategia Recomendada
```
📊 Pirámide de Tests para Presentación:

         /\
        /  \       Tests de Integración (pocos)
       /____\      - Flujos críticos
      /      \     - Autenticación/Autorización
     /        \    - Casos de uso principales
    /__________\   
   /            \  Tests Unitarios (muchos)
  /              \ - Todos los controllers
 /________________\- Todos los casos de error
                   - Todas las conversiones Result → ActionResult
```

---

## 📊 Estadísticas del Proyecto

### Tests Unitarios de Presentación
- **Archivos**: 1 (`UsersControllerTests.cs`)
- **Tests**: 8
- **Cobertura**: UsersController completo

### Tests de Integración de Presentación
- **Archivos**: 9
- **Tests**: ~30+
- **Cobertura**: Múltiples controllers y flujos

---

## 🚀 Ejecutar Tests

```bash
# Solo tests unitarios de presentación
dotnet test --filter "Category=Unit&Layer=Presentation"

# Solo tests de integración de presentación
dotnet test --filter "Category=Integration&Layer=Presentation"

# Todos los tests de presentación
dotnet test --filter "Layer=Presentation"

# Test específico
dotnet test --filter "FullyQualifiedName~UsersControllerTests"
```

---

## 📚 Resumen Final

| Característica | Tests Unitarios | Tests de Integración |
|---------------|----------------|---------------------|
| **Mocks** | ✅ Sí (Moq) | ❌ No |
| **HTTP** | ❌ No | ✅ Sí |
| **Autenticación** | ❌ No | ✅ Sí |
| **Routing** | ❌ No | ✅ Sí |
| **Middleware** | ❌ No | ✅ Sí |
| **Use Cases** | 🎭 Mock | ✅ Real |
| **Repositories** | 🎭 Mock | 💾 En memoria |
| **Velocidad** | ⚡⚡⚡ | ⚡⚡ |
| **Aislamiento** | 🔒🔒🔒 | 🔒🔒 |
| **Realismo** | 📊 | 🎯🎯🎯 |

**Ambos tipos son importantes y complementarios!** 🎯
