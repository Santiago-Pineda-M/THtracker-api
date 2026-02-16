# Arquitectura de Tests de Presentación - Diagrama Visual

## 🏗️ Flujo de un Test de Integración

```
┌─────────────────────────────────────────────────────────────────────┐
│                         TEST DE INTEGRACIÓN                          │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  1. ARRANGE - Preparar el Escenario                                 │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  var factory = new ApiWebApplicationFactory();                │  │
│  │  var client = factory.CreateClient();                         │  │
│  │                                                                │  │
│  │  // Configurar autenticación                                  │  │
│  │  client.DefaultRequestHeaders.Add(                            │  │
│  │      TestAuthHandler.HeaderUserId,                            │  │
│  │      userId.ToString()                                        │  │
│  │  );                                                            │  │
│  │                                                                │  │
│  │  // Preparar datos en repositorio en memoria                  │  │
│  │  var repo = factory.Services.GetService<ICategoryRepository>();│  │
│  │  await repo.AddAsync(new Category(userId, "Test"));           │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  2. ACT - Ejecutar la Petición HTTP                                 │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  var response = await client.GetAsync("/api/v1/categories");  │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  SERVIDOR DE PRUEBA (ApiWebApplicationFactory)                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │  MIDDLEWARE PIPELINE                                     │  │  │
│  │  │  ┌────────────────────────────────────────────────────┐  │  │  │
│  │  │  │  1. TestAuthHandler (Autenticación Simulada)       │  │  │  │
│  │  │  │     - Lee X-Test-UserId header                     │  │  │  │
│  │  │  │     - Crea ClaimsPrincipal                         │  │  │  │
│  │  │  └────────────────────────────────────────────────────┘  │  │  │
│  │  │                        ▼                                 │  │  │
│  │  │  ┌────────────────────────────────────────────────────┐  │  │  │
│  │  │  │  2. Routing                                        │  │  │  │
│  │  │  │     - Mapea URL a Controller/Action                │  │  │  │
│  │  │  └────────────────────────────────────────────────────┘  │  │  │
│  │  │                        ▼                                 │  │  │
│  │  │  ┌────────────────────────────────────────────────────┐  │  │  │
│  │  │  │  3. Authorization                                  │  │  │  │
│  │  │  │     - Verifica [Authorize] attributes              │  │  │  │
│  │  │  └────────────────────────────────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  CAPA DE PRESENTACIÓN (Controllers)                                 │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  CategoriesController                                         │  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │  [HttpGet]                                              │  │  │
│  │  │  public async Task<IActionResult> GetAll()              │  │  │
│  │  │  {                                                       │  │  │
│  │  │      var userId = User.GetUserId();                     │  │  │
│  │  │      var result = await _getAllCategoriesUseCase        │  │  │
│  │  │          .ExecuteAsync(userId);                         │  │  │
│  │  │      return result.ToActionResult();                    │  │  │
│  │  │  }                                                       │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  CAPA DE APLICACIÓN (Use Cases)                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  GetAllCategoriesUseCase                                      │  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │  public async Task<Result<List<CategoryDto>>>           │  │  │
│  │  │  ExecuteAsync(Guid userId)                              │  │  │
│  │  │  {                                                       │  │  │
│  │  │      var categories = await _categoryRepository         │  │  │
│  │  │          .GetAllByUserAsync(userId);                    │  │  │
│  │  │      return Result.Success(categories.ToDto());         │  │  │
│  │  │  }                                                       │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  CAPA DE INFRAESTRUCTURA (Repositories)                             │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  InMemoryCategoryRepository (EN TESTS)                        │  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │  private Dictionary<Guid, Category> _categories;        │  │  │
│  │  │                                                          │  │  │
│  │  │  public Task<IEnumerable<Category>>                     │  │  │
│  │  │  GetAllByUserAsync(Guid userId)                         │  │  │
│  │  │  {                                                       │  │  │
│  │  │      return Task.FromResult(                            │  │  │
│  │  │          _categories.Values                             │  │  │
│  │  │              .Where(c => c.UserId == userId)            │  │  │
│  │  │      );                                                  │  │  │
│  │  │  }                                                       │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  │                                                                │  │
│  │  ⚠️  NO SE USA: AppDbContext / SQL Server                     │  │
│  │  ✅  SE USA: Dictionary en memoria                             │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  RESPUESTA HTTP                                                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  HTTP/1.1 200 OK                                              │  │
│  │  Content-Type: application/json                               │  │
│  │                                                                │  │
│  │  [                                                             │  │
│  │    {                                                           │  │
│  │      "id": "guid",                                             │  │
│  │      "name": "Test",                                           │  │
│  │      "userId": "guid"                                          │  │
│  │    }                                                           │  │
│  │  ]                                                             │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│  3. ASSERT - Verificar el Resultado                                 │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  response.StatusCode.Should().Be(HttpStatusCode.OK);          │  │
│  │                                                                │  │
│  │  var content = await response.Content.ReadAsStringAsync();    │  │
│  │  content.Should().Contain("Test");                            │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## 🔄 Comparación: Producción vs Tests

```
┌──────────────────────────────────────────────────────────────────────┐
│                          EN PRODUCCIÓN                                │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  Cliente (Browser/App)                                                │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  JWT Token      │  ← Autenticación real con tokens                │
│  │  Bearer xxx...  │                                                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  Controllers    │                                                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  Use Cases      │                                                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  EF Core        │  ← Acceso real a base de datos                  │
│  │  Repositories   │                                                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  SQL Server     │  ← Base de datos real                           │
│  └─────────────────┘                                                 │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                          EN TESTS                                     │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  HttpClient (Test)                                                    │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  TestAuthHandler│  ← Autenticación simulada con headers           │
│  │  X-Test-UserId  │                                                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  Controllers    │  ← MISMO código que producción                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  Use Cases      │  ← MISMO código que producción                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  InMemory       │  ← Repositorios simulados                       │
│  │  Repositories   │                                                  │
│  └─────────────────┘                                                 │
│         │                                                             │
│         ▼                                                             │
│  ┌─────────────────┐                                                 │
│  │  Dictionary     │  ← Datos en memoria (no DB)                     │
│  │  <Guid, Entity> │                                                  │
│  └─────────────────┘                                                 │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘
```

## 🎯 Ventajas del Enfoque

```
┌─────────────────────────────────────────────────────────────────┐
│  ✅ VENTAJAS                                                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  🚀 VELOCIDAD                                                    │
│     • No hay I/O de base de datos                               │
│     • Todo en memoria RAM                                       │
│     • Tests ejecutan en milisegundos                            │
│                                                                  │
│  🔒 AISLAMIENTO                                                  │
│     • Cada test tiene su propio estado                          │
│     • No hay efectos secundarios entre tests                    │
│     • Resultados predecibles                                    │
│                                                                  │
│  🎯 REALISMO                                                     │
│     • Prueba el flujo completo de la API                        │
│     • Usa el mismo código que producción                        │
│     • Detecta problemas de integración                          │
│                                                                  │
│  🛠️ MANTENIBILIDAD                                              │
│     • Fácil de entender                                         │
│     • No requiere configuración compleja                        │
│     • Sin dependencias externas                                 │
│                                                                  │
│  📦 PORTABILIDAD                                                 │
│     • Funciona en cualquier máquina                             │
│     • No requiere SQL Server instalado                          │
│     • Ideal para CI/CD                                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## 🔍 Escenarios de Test Comunes

```
┌─────────────────────────────────────────────────────────────────┐
│  ESCENARIO 1: Sin Autenticación                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Test Code:                                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  var client = _factory.CreateClient();                     │ │
│  │  var response = await client.GetAsync("/api/v1/endpoint"); │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Flujo:                                                          │
│  Request → TestAuthHandler (no header) → 401 Unauthorized       │
│                                                                  │
│  Resultado Esperado:                                             │
│  ✅ StatusCode = 401 Unauthorized                                │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ESCENARIO 2: Usuario Autenticado pero No Autorizado            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Test Code:                                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  var ownerId = Guid.NewGuid();                             │ │
│  │  var otherUserId = Guid.NewGuid();                         │ │
│  │                                                             │ │
│  │  // Crear recurso del owner                                │ │
│  │  await repo.AddAsync(new Category(ownerId, "Cat"));        │ │
│  │                                                             │ │
│  │  // Intentar acceder como otro usuario                     │ │
│  │  client.DefaultRequestHeaders.Add(                         │ │
│  │      TestAuthHandler.HeaderUserId,                         │ │
│  │      otherUserId.ToString()                                │ │
│  │  );                                                         │ │
│  │  var response = await client.GetAsync("/api/v1/...");      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Flujo:                                                          │
│  Request → TestAuthHandler (otherUserId) → Controller           │
│         → Use Case → Check Ownership → 403 Forbidden            │
│                                                                  │
│  Resultado Esperado:                                             │
│  ✅ StatusCode = 403 Forbidden                                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ESCENARIO 3: Usuario Autorizado - Operación Exitosa            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Test Code:                                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  var ownerId = Guid.NewGuid();                             │ │
│  │                                                             │ │
│  │  // Crear recurso                                          │ │
│  │  await repo.AddAsync(new Category(ownerId, "Cat"));        │ │
│  │                                                             │ │
│  │  // Acceder como propietario                               │ │
│  │  client.DefaultRequestHeaders.Add(                         │ │
│  │      TestAuthHandler.HeaderUserId,                         │ │
│  │      ownerId.ToString()                                    │ │
│  │  );                                                         │ │
│  │  var response = await client.GetAsync("/api/v1/...");      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Flujo:                                                          │
│  Request → TestAuthHandler (ownerId) → Controller               │
│         → Use Case → Repository → 200 OK + Data                 │
│                                                                  │
│  Resultado Esperado:                                             │
│  ✅ StatusCode = 200 OK                                          │
│  ✅ Response contiene los datos esperados                        │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```
