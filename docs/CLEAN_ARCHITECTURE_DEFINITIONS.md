# HOJA DE DEFINICIONES

Clean Architecture – Directivas Operativas para Agentes de IA

## Propósito del documento

Este documento define de forma normativa, precisa y no ambigua cómo debe estructurarse, extenderse y modificarse un proyecto de software que sigue Clean Architecture, con el objetivo de servir como contexto operativo para agentes de IA que interactúan con el código base.

Este documento no describe intenciones, describe reglas ejecutables.

## 1. Definición General de Clean Architecture

Clean Architecture es un modelo arquitectónico basado en:

* aración estricta de responsabilidades
* Independencia del dominio respecto a frameworks
* Regla de dependencia hacia el núcleo
* Uso de abstracciones como puntos de acoplamiento

La arquitectura se organiza en capas concéntricas, no jerárquicas.

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

Violación de esta regla invalida la arquitectura.

## 4. Capa Domain (NÚCLEO DEL SISTEMA)

### 4.1 Definición Técnica

El Domain (`THtracker.Domain`) representa el modelo de negocio puro.
Contiene las reglas que definen qué es válido o inválido en el sistema, independientemente de tecnología, persistencia o interfaz.

### 4.2 Responsabilidades Permitidas

* Entidades de dominio con comportamiento
* Value Objects
* Reglas de negocio invariantes
* Excepciones de dominio
* Interfaces solo si representan conceptos del dominio

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

* Casos de uso ({Acción}{Entidad}UseCase)
* Orquestación de entidades del Domain
* Coordinación de transacciones
* Validación de flujo (no invariantes)
* Definición de interfaces (puertos) hacia Infrastructure
* Modelos de entrada y salida (DTOs de aplicación)

### 5.3 Responsabilidades Prohibidas

* Reglas de negocio invariantes
* Acceso directo a persistencia
* Uso de frameworks
* Implementaciones técnicas
* Controladores

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

Infrastructure ejecuta decisiones. Nunca las toma.

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

* Lógica de negocio
* Reglas de dominio
* Acceso a repositorios
* Orquestación compleja

### 7.4 Regla Crítica

Presentation coordina protocolos, no procesos.

### 7.5 Convenciones de Controladores

* Heredar de `AuthorizedControllerBase` cuando requieran usuario autenticado para acceder al claim NameIdentifier de forma consistente.
* No contener lógica de negocio; delegar en UseCases de Application.
* Usar atributos `[Authorize]` y `[Authorize(Roles = ...)]` según corresponda.
* Validar formato y usar `[FromBody]`/`[FromRoute]`/`[FromQuery]` explícitos.
* Mapear Request → Input del UseCase y Output → `IActionResult` con códigos adecuados (200, 201, 204, 404, 403, 401).
* Documentar respuestas con `ProducesResponseType`.

### 7.6 Manejo de Errores en Presentation

* Errores de negocio desde Application deben traducirse a 400 (BadRequest) con payload consistente (`ApiErrorResponse`).
* Autenticación/Autorización: 401 cuando no autenticado, 403 cuando no autorizado/dueño.
* Nunca lanzar excepciones desde Presentation por lógica de negocio; capturar y mapear.

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
    * Ajustar entidades y reglas
    * Proteger invariantes
2. **Application (`THtracker.Application`)**
    * Crear UseCase
    * Orquestar dominio
    * Definir interfaces necesarias
3. **Infrastructure (`THtracker.Infrastructure`)**
    * Implementar interfaces
    * Resolver detalles técnicos
4. **Presentation (`THtracker.API`)**
    * Exponer endpoint
    * Validar formato
    * Invocar UseCase
5. **Composition Root**
    * Registrar dependencias
    * Conectar Infrastructure

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
