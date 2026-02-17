# HOJA DE DEFINICIONES

Clean Architecture – Directivas Operativas para Agentes de IA

## Propósito del documento

Este documento define de forma normativa, precisa y no ambigua cómo debe estructurarse, extenderse y modificarse un proyecto de software que sigue Clean Architecture, con el objetivo de servir como contexto operativo para agentes de IA que interactúan con el código base.

Este documento no describe intenciones, describe reglas ejecutables.

## 1. Definición General de Clean Architecture

Clean Architecture es un modelo arquitectónico basado en los principios SOLID y el patrón de arquitectura hexagonal (Ports & Adapters), cuyos pilares son:

* **Independencia de Frameworks:** La arquitectura no depende de la existencia de alguna librería de software.
* **Testabilidad:** Las reglas de negocio se pueden probar sin la interfaz de usuario, base de datos, servidor web o cualquier otro elemento externo.
* **Independencia de la UI:** La UI puede cambiar fácilmente sin cambiar el resto del sistema.
* **Independencia de la Base de Datos:** Se puede intercambiar el motor de persistencia sin afectar las reglas de negocio.
* **Regla de Dependencia:** Las dependencias de código solo pueden apuntar hacia adentro, hacia los círculos de mayor nivel (Políticas de Negocio).

La arquitectura se organiza en capas concéntricas donde el flujo de control se invierte mediante interfaces para mantener el desacoplamiento.

## 2. Capas del Sistema (Definición Formal)

El sistema está compuesto por los siguientes módulos independientes, mapeados a las carpetas del proyecto:

* **Domain** (`THtracker.Domain`)
* **Application** (`THtracker.Application`)
* **Infrastructure** (`THtracker.Infrastructure`)
* **Presentation** (`THtracker.API`)
* **Test** (`THtracker.Tests`)

Cada módulo es una unidad lógica aislada con reglas explícitas de dependencia.

## 3. Regla Global de Dependencias (OBLIGATORIA)

### 3.1 Regla Principal

Toda dependencia debe apuntar hacia capas más internas.

### 3.2 Grafo de Dependencias Permitidas

* `THtracker.API` (Presentation)   → `THtracker.Application`
* `THtracker.Application`    → `THtracker.Domain`
* `THtracker.Infrastructure` → `THtracker.Application`
* `THtracker.Infrastructure` → `THtracker.Domain`
* `THtracker.Tests`          → Cualquiera (según tipo de prueba)

### 3.3 Dependencias Prohibidas

* `THtracker.Domain` → cualquier otra capa
* `THtracker.Application` → `THtracker.Infrastructure`
* `THtracker.API` (Presentation) → `THtracker.Infrastructure`
* `THtracker.Domain` → frameworks, librerías externas, DTOs
* `THtracker.Domain` → detalles técnicos

**Nota sobre la raíz de composición (Composition Root):** El proyecto `Presentation` (API) puede tener una referencia física a `Infrastructure` exclusivamente para la configuración de la Inyección de Dependencias en `Program.cs`. El uso de clases concretas de infraestructura fuera de esta configuración está estrictamente prohibido.

Violación de esta regla invalida la arquitectura.

## 4. Capa Domain (NÚCLEO DEL SISTEMA)

### 4.1 Definición Técnica

El Domain (`THtracker.Domain`) representa el modelo de negocio puro.
Contiene las reglas que definen qué es válido o inválido en el sistema, independientemente de tecnología, persistencia o interfaz.

### 4.2 Responsabilidades Permitidas

* **Entidades (Aggregates):** Objetos con identidad única que encapsulan estado y comportamiento. Son el punto de entrada para cambios de estado (Garantizan invariantes).
* **Value Objects:** Objetos sin identidad definidos por sus atributos (ej. Email, Dinero). Deben ser inmutables.
* **Reglas de Negocio Invariantes:** Lógica que debe ser siempre verdadera para que el sistema sea consistente.
* **Domain Services:** Lógica que involucra múltiples entidades y no pertenece naturalmente a una sola.
* **Interfaces (Interfaces/Ports):** Definición de contratos para persistencia (IRepository) o servicios externos.
* **Domain Events:** Notificaciones de cambios significativos en el dominio para consistencia eventual.
* **Excepciones de Dominio:** Errores específicos de negocio (ej. `InsufficientFundsException`).

### 4.3 Responsabilidades Prohibidas

* DTOs
* Casos de uso
* Lógica de orquestación
* Persistencia
* Frameworks (ORM, HTTP, DI, MediatR, etc.)
* Controladores o UI

### 4.4 Regla Crítica

Si una regla puede romperse sin usar infraestructura, pertenece al Domain.

## 5. Capa Application (CASOS DE USO)

### 5.1 Definición Técnica

La capa Application (`THtracker.Application`) contiene la lógica de aplicación, es decir, la coordinación de reglas de dominio para cumplir un objetivo específico del sistema.

### 5.2 Responsabilidades Permitidas

* **Use Cases (Interactors):** Clases que implementan una operación específica del sistema orquestando el dominio.
* **Input/Output Ports:** Definición de los datos que entran y salen del caso de uso (DTOs).
* **Result Pattern:** Uso de un objeto `Result<T>` para comunicar éxito o fracaso, evitando el flujo basado en excepciones.
* **DTO Mappings:** Transformación de Entidades de Dominio a DTOs de Aplicación.
* **Validación de Aplicación:** Validación de la Request inicial para asegurar integridad antes de tocar el Dominio.
* **Coordinación de Transacciones:** Gestión de la unidad de trabajo (Unit of Work).

### 5.3 Responsabilidades Prohibidas

* Reglas de negocio invariantes (pertenecen al Domain)
* Acceso directo a detalles de persistencia
* Uso de frameworks de infraestructura (EF Core, ASP.NET Core en lógica)
* Implementaciones técnicas concretas
* Controladores o manejo de protocolos de red

### 5.4 Regla Crítica

Application coordina. Domain decide.

## 6. Capa Infrastructure (DETALLES TÉCNICOS)

### 6.1 Definición Técnica

Infrastructure (`THtracker.Infrastructure`) contiene implementaciones concretas de los contratos definidos en capas internas.
Es intercambiable sin modificar el núcleo.

### 6.2 Responsabilidades Permitidas

* Implementación de repositorios
* Persistencia (SQL, NoSQL, ORM)
* APIs externas
* Servicios técnicos (correo, autenticación, archivos)
* Adaptadores de frameworks

### 6.3 Responsabilidades Prohibidas

* Reglas de negocio
* Lógica de casos de uso
* Decisiones de dominio
* Dependencias hacia Presentation

### 6.4 Regla Crítica

Infrastructure ejecuta decisiones técnicas. Nunca toma decisiones de negocio.

## 7. Capa Presentation (INTERFAZ)

### 7.1 Definición Técnica

Presentation (`THtracker.API`) es la capa de entrada y salida del sistema.
Traduce protocolos externos a invocaciones de casos de uso.

### 7.2 Responsabilidades Permitidas

* Endpoints (HTTP, gRPC, CLI, UI)
* Validación de formato
* Mapeo Request → Input del UseCase
* Mapeo Output → Response

### 7.3 Responsabilidades Prohibidas

* Lógica de negocio o validaciones de invariantes
* Reglas de dominio
* Acceso directo a repositorios o bases de datos
* Orquestación de lógica de aplicación (debe ir en UseCases)

### 7.4 Regla Crítica

Presentation coordina protocolos, no procesos.

### 7.5 Convenciones de Controladores

* Heredar de `AuthorizedControllerBase` cuando requieran usuario autenticado para acceder al claim NameIdentifier de forma consistente.
* No contener lógica de negocio; delegar en UseCases de Application.
* Usar atributos `[Authorize]` y `[Authorize(Roles = ...)]` según corresponda.
* Validar formato y usar `[FromBody]`/`[FromRoute]`/`[FromQuery]` explícitos.
* Mapear Request → Input del UseCase y Output → `IActionResult` con códigos adecuados (200, 201, 204, 404, 403, 401).
* Documentar respuestas con `ProducesResponseType`.

* **Result Mapping:** Mapear el objeto `Result` del Use Case a un `ProblemDetails` o respuesta estándar HTTP.
* **Global Exception Handling:** Un middleware debe capturar excepciones inesperadas (500), pero la lógica de negocio debe fluir vía `Result`.
* **ViewModel / Response DTOs:** Objetos optimizados para el consumo del cliente (frontend/móvil).
* **Validación de Formato:** Uso de Data Annotations o FluentValidation para asegurar que los tipos de datos son correctos.
* **Thin Controllers:** Los controladores no deben tener más de 5-10 líneas de código por método. Solo extraen datos de la Request, llaman al Use Case y devuelven la Response.

## 8. DTOs (Definición y Ubicación)

### 8.1 Definición

DTO (Data Transfer Object) es un objeto diseñado exclusivamente para cruzar límites entre capas.

### 8.2 Ubicación Permitida

* Presentation (`THtracker.API`)
* Application (`THtracker.Application`)

### 8.3 Ubicación Prohibida

* Domain (`THtracker.Domain`)

## 9. CQRS (Regla de Uso)

### 9.1 Definición

CQRS separa operaciones de lectura y escritura.

### 9.2 Regla

* Commands → Application (UseCases)
* Queries → Application o capa de lectura dedicada

CQRS es opcional.
No debe introducirse si no resuelve un problema real.

## 10. Convenciones de Nomenclatura (OBLIGATORIAS)

| Concepto | Convención |
| :--- | :--- |
| Entidad | `{Entidad}` |
| Value Object | `{Nombre}` |
| Use Case | `{Acción}{Entidad}UseCase` |
| Interfaz | `I{Nombre}` |
| Implementación | `{Nombre}Service` / `{Nombre}Repository` |
| Controller | `{Entidad}Controller` |
| DTO | `{Entidad}Request` / `Response` / `DTO` |

## 11. Flujo Normativo para Agregar Funcionalidad

1. **Domain (`THtracker.Domain`)**
    * Ajustar entidades y proteger invariantes.
    * Crear Value Objects o Domain Services si es necesario.
2. **Application (`THtracker.Application`)**
    * Definir el contrato de entrada/salida (DTOs).
    * Implementar el Caso de Uso (UseCase/Interactor).
    * Definir interfaces de infraestructura necesarias (Puerto).
3. **Infrastructure (`THtracker.Infrastructure`)**
    * Implementar las interfaces definidas en la capa Application o Domain (Adaptador).
    * Configurar mappers de persistencia si es necesario.
4. **Presentation (`THtracker.API`)**
    * Exponer el endpoint (Controller/Minimal API).
    * Mapear la Request al Input del Use Case.
    * Invocar el Use Case y transformar el `Result` en una Respuesta HTTP.
5. **Composition Root (API/Program.cs)**
    * Registrar las nuevas dependencias en el contenedor de DI.
6. **Testing (`THtracker.Tests`)**
    * Validar lógica en todos los niveles para asegurar que los cambios no rompen invariantes existentes.

## 12. Testing (Clasificación)

* **Domain (`THtracker.Domain`)** → Unit Tests
* **Application (`THtracker.Application`)** → Use Case Tests
* **Infrastructure (`THtracker.Infrastructure`)** → Integration Tests
* **Presentation (`THtracker.API`)** → API/UI Tests

### 12.1 Pruebas de Presentation

* Unit: tests de controladores que verifican el mapeo de UseCases a respuestas HTTP usando Moq y FluentAssertions.
* Integration (recomendado): pruebas con Microsoft.AspNetCore.Mvc.Testing y `WebApplicationFactory` para validar pipeline, autenticación JWT y políticas `[Authorize]`, incluyendo 401/403.
* Filtrado por Traits: `Category=Unit` y `Layer=Presentation` para ejecución selectiva.

## 13. Reglas de Decisión para Agentes de IA

Un agente de IA debe aplicar las siguientes reglas:

* Si una clase usa frameworks → NO es Domain
* Si coordina pasos → Application
* Si persiste o integra → Infrastructure
* Si recibe o responde → Presentation
* Si una dependencia apunta hacia afuera → ERROR
* Si una regla de negocio está fuera del Domain → ERROR

## 14. Condición de Validez Arquitectónica

Un proyecto solo cumple Clean Architecture si:

* El Domain es independiente
* La lógica de negocio vive en el núcleo
* Infrastructure es reemplazable
* Presentation es delgada
* Las dependencias fluyen hacia adentro

Cualquier excepción a estas reglas debe considerarse una violación, no una interpretación.
