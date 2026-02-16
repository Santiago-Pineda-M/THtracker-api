# Guía de Testing para la Capa de Presentación

> **📌 Nota Importante**: Esta guía se enfoca en **Tests de Integración** de la capa de presentación. 
> Para información sobre **Tests Unitarios** de Controllers, consulta: 
> [`presentation-unit-vs-integration-tests.md`](./presentation-unit-vs-integration-tests.md)

## 📋 Índice
1. [Introducción](#introducción)
2. [Arquitectura de Tests](#arquitectura-de-tests)
3. [Componentes Clave](#componentes-clave)
4. [Tipos de Tests](#tipos-de-tests)
5. [Ejemplos Prácticos](#ejemplos-prácticos)
6. [Mejores Prácticas](#mejores-prácticas)

---

## 🎯 Introducción

Los tests de la capa de presentación en THtracker incluyen **dos tipos**:

1. **Tests Unitarios** (`Unit/Presentation/`) - Prueban Controllers aislados con Mocks
2. **Tests de Integración** (`Integration/Presentation/`) - Prueban el flujo completo de la API ← **Esta guía**

Esta guía se enfoca en los **tests de integración**, que verifican el comportamiento completo de los endpoints de la API, desde la recepción de la petición HTTP hasta la respuesta, pasando por toda la cadena de procesamiento (Controllers → Use Cases → Repositories).

### Características Principales

- **Tipo**: Tests de Integración (no unitarios)
- **Alcance**: End-to-End de la API
- **Framework**: xUnit + WebApplicationFactory
- **Ubicación**: `THtracker.Tests/Integration/Presentation/`

---

## 🏗️ Arquitectura de Tests

### Estructura de Directorios

```
THtracker.Tests/
└── Integration/
    └── Presentation/
        ├── Support/
        │   ├── ApiWebApplicationFactory.cs    # Factory para crear servidor de pruebas
        │   └── TestAuthHandler.cs             # Autenticación simulada
        ├── ActivitiesOwnerIntegrationTests.cs
        ├── ActivityLogsControllerIntegrationTests.cs
        ├── ActivityValueDefinitionsIntegrationTests.cs
        ├── AuthControllerIntegrationTests.cs
        ├── CategoriesControllerIntegrationTests.cs
        ├── HealthControllerIntegrationTests.cs
        ├── RolesControllerIntegrationTests.cs
        ├── UsersControllerIntegrationTests.cs
        └── UsersMeIntegrationTests.cs
```

---

## 🔧 Componentes Clave

### 1. ApiWebApplicationFactory

**Propósito**: Crear una instancia de prueba de la aplicación ASP.NET Core con configuración personalizada.

```csharp
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Reemplazar autenticación real con autenticación de prueba
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    options => { }
                );

            // 2. Reemplazar repositorios reales con repositorios en memoria
            ReplaceService<IUserRepository>(services, new InMemoryUserRepository());
            ReplaceService<IActivityRepository>(services, new InMemoryActivityRepository());
            ReplaceService<ICategoryRepository>(services, new InMemoryCategoryRepository());
            // ... más repositorios

            // 3. Deshabilitar seeding de datos
            ReplaceService<IDataSeeder>(services, new NoOpSeeder());
        });
    }
}
```

**Características**:
- ✅ Crea un servidor HTTP real en memoria
- ✅ Reemplaza dependencias con implementaciones en memoria
- ✅ No requiere base de datos real
- ✅ Aislamiento completo entre tests
- ✅ Rápido y predecible

### 2. TestAuthHandler

**Propósito**: Simular autenticación JWT sin necesidad de tokens reales.

```csharp
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string HeaderUserId = "X-Test-UserId";
    public const string HeaderRoles = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Lee el userId de un header personalizado
        if (!Request.Headers.TryGetValue(HeaderUserId, out var userIdVals))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = Guid.Parse(userIdVals.ToString());
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        // Lee roles opcionales
        if (Request.Headers.TryGetValue(HeaderRoles, out var rolesVals))
        {
            var roles = rolesVals.ToString().Split(',');
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

**Uso en Tests**:
```csharp
// Sin autenticación
var response = await client.GetAsync("/api/v1/categories");
// Resultado: 401 Unauthorized

// Con autenticación
var userId = Guid.NewGuid();
client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
var response = await client.GetAsync("/api/v1/categories");
// Resultado: 200 OK (si el usuario tiene permisos)

// Con roles
client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderRoles, "Admin,User");
```

### 3. Repositorios en Memoria

**Propósito**: Simular la capa de datos sin base de datos real.

```csharp
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default) 
        => Task.FromResult(_users.AsEnumerable());

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) 
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    // ... más métodos
}
```

**Ventajas**:
- ⚡ Extremadamente rápido
- 🔒 Aislamiento total entre tests
- 🧪 Control completo del estado
- 📦 Sin dependencias externas

---

## 🧪 Tipos de Tests

### 1. Tests de Autenticación

Verifican que los endpoints requieren autenticación correcta.

```csharp
[Fact]
public async Task GetAll_ShouldReturnUnauthorized_WhenNoAuth()
{
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/api/v1/categories");
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
```

### 2. Tests de Autorización

Verifican que solo los usuarios autorizados pueden acceder a recursos.

```csharp
[Fact]
public async Task GetById_ShouldReturnForbidden_WhenNotOwner()
{
    var client = _factory.CreateClient();
    
    var ownerId = Guid.NewGuid();
    var otherUserId = Guid.NewGuid();
    
    // Crear recurso con un propietario
    var repo = _factory.Services.GetService(typeof(ICategoryRepository)) 
        as ICategoryRepository;
    var category = new Category(ownerId, "Cat 1");
    await repo!.AddAsync(category);
    
    // Intentar acceder con otro usuario
    client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, otherUserId.ToString());
    var response = await client.GetAsync($"/api/v1/categories/{category.Id}");
    
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

### 3. Tests de Operaciones CRUD

Verifican que las operaciones básicas funcionan correctamente.

```csharp
[Fact]
public async Task GetById_ShouldReturnOk_WhenOwner()
{
    var client = _factory.CreateClient();
    
    var ownerId = Guid.NewGuid();
    var repo = _factory.Services.GetService(typeof(ICategoryRepository)) 
        as ICategoryRepository;
    var category = new Category(ownerId, "Cat 1");
    await repo!.AddAsync(category);
    
    // Acceder como propietario
    client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
    var response = await client.GetAsync($"/api/v1/categories/{category.Id}");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### 4. Tests de Validación

Verifican que la API valida correctamente los datos de entrada.

```csharp
[Fact]
public async Task Create_ShouldReturnBadRequest_WhenInvalidData()
{
    var client = _factory.CreateClient();
    var userId = Guid.NewGuid();
    
    client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
    
    // Enviar datos inválidos (nombre vacío)
    var content = new StringContent(
        "{\"name\":\"\",\"email\":\"invalid\"}",
        Encoding.UTF8,
        "application/json"
    );
    
    var response = await client.PostAsync("/api/v1/users", content);
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

### 5. Tests de Reglas de Negocio

Verifican que se aplican correctamente las reglas de negocio.

```csharp
[Fact]
public async Task Stop_ShouldReturnOk_WhenOwner()
{
    var client = _factory.CreateClient();
    var ownerId = Guid.NewGuid();
    
    // Preparar datos
    var activityRepo = _factory.Services.GetService(typeof(IActivityRepository)) 
        as IActivityRepository;
    var logRepo = _factory.Services.GetService(typeof(IActivityLogRepository)) 
        as IActivityLogRepository;
    
    var activity = new Activity(ownerId, Guid.NewGuid(), "Act 1", false);
    await activityRepo!.AddAsync(activity);
    
    var log = new ActivityLog(activity.Id, DateTime.UtcNow.AddMinutes(-5));
    await logRepo!.AddAsync(log);
    
    // Ejecutar acción
    client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, ownerId.ToString());
    var response = await client.PostAsync($"/api/v1/activity-logs/{log.Id}/stop", null);
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

## 📚 Ejemplos Prácticos

### Ejemplo Completo: Test de Categorías

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
        
        var repo = _factory.Services.GetService(typeof(ICategoryRepository)) 
            as ICategoryRepository;
        var category = new Category(ownerId, "Cat 1");
        await repo!.AddAsync(category);
        
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

### Ejemplo: Test con Datos JSON

```csharp
[Fact]
public async Task UpdateMe_ShouldReturnOk_WhenUserExists()
{
    // Arrange
    var client = _factory.CreateClient();
    var userId = Guid.NewGuid();
    
    var repo = (IUserRepository)_factory.Services.GetService(typeof(IUserRepository))!;
    var user = new User("Bob", "bob@example.com");
    await repo.AddAsync(user);
    
    client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
    
    var content = new StringContent(
        "{\"name\":\"Bob2\",\"email\":\"bob2@example.com\"}", 
        Encoding.UTF8, 
        "application/json"
    );
    
    // Act
    var response = await client.PutAsync("/api/v1/users/me", content);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

## ✅ Mejores Prácticas

### 1. Estructura AAA (Arrange-Act-Assert)

```csharp
[Fact]
public async Task Example_Test()
{
    // Arrange: Preparar el escenario
    var client = _factory.CreateClient();
    var userId = Guid.NewGuid();
    client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderUserId, userId.ToString());
    
    // Act: Ejecutar la acción
    var response = await client.GetAsync("/api/v1/endpoint");
    
    // Assert: Verificar el resultado
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### 2. Usar Factory Pattern

```csharp
public class CategoriesControllerIntegrationTests
{
    private readonly ApiWebApplicationFactory _factory = new();
    
    // Los tests usan _factory para crear clientes
}
```

### 3. Nombres Descriptivos

```csharp
// ✅ BIEN
[Fact]
public async Task GetById_ShouldReturnForbidden_WhenNotOwner()

// ❌ MAL
[Fact]
public async Task Test1()
```

### 4. Aislar Tests

Cada test debe ser independiente y no depender del estado de otros tests.

```csharp
// ✅ BIEN: Cada test crea sus propios datos
[Fact]
public async Task Test1()
{
    var category = new Category(userId, "Cat 1");
    await repo.AddAsync(category);
    // ...
}

[Fact]
public async Task Test2()
{
    var category = new Category(userId, "Cat 2");
    await repo.AddAsync(category);
    // ...
}
```

### 5. Verificar Códigos de Estado HTTP

```csharp
response.StatusCode.Should().Be(HttpStatusCode.OK);           // 200
response.StatusCode.Should().Be(HttpStatusCode.Created);      // 201
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);   // 400
response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // 401
response.StatusCode.Should().Be(HttpStatusCode.Forbidden);    // 403
response.StatusCode.Should().Be(HttpStatusCode.NotFound);     // 404
```

### 6. Usar Traits para Organización

```csharp
[Trait("Category", "Integration")]
[Trait("Layer", "Presentation")]
public class MyControllerTests
{
    // ...
}
```

Esto permite ejecutar tests específicos:
```bash
dotnet test --filter "Layer=Presentation"
dotnet test --filter "Category=Integration"
```

### 7. Limpiar Recursos

```csharp
public class MyTests : IDisposable
{
    private readonly ApiWebApplicationFactory _factory = new();
    
    public void Dispose()
    {
        _factory.Dispose();
    }
}
```

---

## 🎯 Resumen

### Ventajas de este Enfoque

✅ **Tests Realistas**: Prueban el flujo completo de la API  
✅ **Rápidos**: Sin base de datos real, todo en memoria  
✅ **Aislados**: Cada test es independiente  
✅ **Mantenibles**: Fácil de entender y modificar  
✅ **Confiables**: Resultados predecibles y consistentes  

### Diferencias con Tests Unitarios

| Aspecto | Tests Unitarios | Tests de Integración (Presentación) |
|---------|----------------|-------------------------------------|
| **Alcance** | Una clase/método | Endpoint completo (Controller → Use Case → Repository) |
| **Dependencias** | Mocks/Stubs | Implementaciones reales en memoria |
| **Velocidad** | Muy rápido | Rápido |
| **Realismo** | Bajo | Alto |
| **Propósito** | Verificar lógica aislada | Verificar integración completa |

### Cuándo Usar Cada Tipo

- **Tests Unitarios**: Para lógica de negocio compleja en Use Cases, validadores, entidades
- **Tests de Integración**: Para verificar que los endpoints funcionan correctamente end-to-end

---

## 📖 Referencias

- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
