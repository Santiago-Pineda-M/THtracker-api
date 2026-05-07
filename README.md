<div align="center">

# ⚙️ THtracker API

### Backend REST API — Clean Architecture · .NET · PostgreSQL

[![.NET](https://img.shields.io/badge/.NET_10-5C2D91?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-2CA5E0?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com)
[![Railway](https://img.shields.io/badge/Railway-131415?style=for-the-badge&logo=railway&logoColor=white)](https://railway.app)

[![CI/CD](https://github.com/Santiago-Pineda-M/THtracker-api/actions/workflows/main.yml/badge.svg)](https://github.com/Santiago-Pineda-M/THtracker-api/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

**[🖥️ Frontend App](https://github.com/Santiago-Pineda-M/THtrackerApp)** · **[🌐 Demo Live](https://thtracker-develop.netlify.app/activities)** · **[🐛 Reportar Bug](https://github.com/Santiago-Pineda-M/THtracker-api/issues)**

</div>

---

## 📖 Sobre el Proyecto

**THtracker API** es el backend del ecosistema THtracker — una REST API construida con **.NET (ASP.NET Core)** siguiendo los principios de **Clean Architecture**. Provee los servicios de autenticación, gestión de tareas, actividades y finanzas que consume el [frontend PWA](https://github.com/Santiago-Pineda-M/THtrackerApp).

> 🏗️ La arquitectura está diseñada para ser **independiente de frameworks** — el dominio y los casos de uso no dependen de ASP.NET Core, PostgreSQL, ni ninguna tecnología de infraestructura.

---

## ✨ Características Principales

| Feature | Descripción |
|---------|-------------|
| 🔐 **Autenticación JWT** | Access tokens + Refresh tokens para sesiones seguras |
| ♻️ **Rotación de Refresh Token** | En refresh se rota token y se actualiza la sesión existente |
| 📄 **Paginación estándar** | Listados con `pageNumber` / `pageSize` y `PaginatedResponse<T>` |
| 🏗️ **Clean Architecture** | 5 proyectos: Domain, Application, Infrastructure, API, Tests |
| 📦 **Repository Pattern** | Abstracción completa de la capa de datos |
| ✅ **Validaciones** | FluentValidation + pipeline MediatR (`ValidationBehavior`) |
| 🗄️ **ORM** | Entity Framework Core con Fluent API y migraciones |
| 🧪 **Tests Unit + Integration** | Cobertura en todas las capas incluida la presentación |
| 🔢 **API Versionada** | Endpoints bajo `/api/v1/` para compatibilidad futura |
| 🛡️ **Hardening HTTP** | HSTS + security headers + rate limit en autenticación |
| 🔄 **CI/CD** | Pipeline automatizado con GitHub Actions |
| 🐳 **Docker** | Containerizado para deploy reproducible |
| 📋 **Swagger** | Documentación de endpoints interactiva |

---

## 🛠️ Stack Tecnológico

```
Backend
├── 🟣 .NET 10 / ASP.NET Core — Framework web
├── 🔷 C#                     — Lenguaje principal
├── 🗄️  Entity Framework Core  — ORM + Migraciones
├── 🐘 PostgreSQL              — Base de datos principal
├── 🔑 JWT Bearer              — Autenticación
├── 📝 FluentValidation        — Validaciones de requests
└── 🔄 MediatR                 — CQRS + pipeline behaviors

DevOps & Tooling
├── 🐳 Docker + Docker Compose — Containerización
├── ⚙️  GitHub Actions          — CI/CD pipeline
├── 🚂 Railway                 — Deploy en producción
└── 📖 Swagger / OpenAPI       — Documentación API
```

---

## 🏗️ Arquitectura — Clean Architecture

La solución está dividida en **5 proyectos** con dependencias estrictamente unidireccionales.

```
THtracker.slnx
│
├── THtracker.Domain/              # 🔵 Núcleo — sin dependencias externas
│   ├── Entities/                  # Entidades del negocio
│   ├── Interfaces/                # Contratos de repositorios y servicios
│   └── Common/                    # Tipos base compartidos del dominio
│
├── THtracker.Application/         # 🟡 Casos de uso y orquestación
│   ├── Features/                  # CQRS por feature (Commands/Queries + Handlers + Validators)
│   │   └── [Activities|Tasks|Auth|Reports|UserSessions...]
│   ├── Common/                    # Behaviors, paginación y utilidades transversales
│   ├── Interfaces/                # Contratos de servicios de aplicación
│   └── Constants/                 # Constantes de la capa de aplicación
│
├── THtracker.Infrastructure/      # 🟠 Implementaciones de contratos
│   ├── Persistence/
│   │   └── Configurations/        # EF Core Fluent API por entidad
│   ├── Repositories/              # Implementación de IRepository<T>
│   ├── Services/                  # Servicios externos (JWT, email, etc.)
│   ├── Seeding/                   # Datos semilla de la BD
│   └── Migrations/                # Migraciones EF Core
│
├── THtracker.API/                 # 🔴 Capa de presentación HTTP
│   ├── Controllers/v1/            # Endpoints REST versionados
│   ├── Routing/                   # Definición centralizada de rutas
│   ├── Middlewares/               # Error handling, logging, auth
│   ├── Extensions/                # Métodos de extensión para DI/pipeline
│   └── Properties/                # launchSettings.json
│
└── THtracker.Tests/               # 🟢 Cobertura de pruebas
    ├── Unit/
    │   ├── Application/           # Tests de UseCases por feature
    │   ├── Domain/Entities/       # Tests de reglas del dominio
    │   ├── Infrastructure/        # Tests de Repositories y Services
    │   └── Presentation/Controllers/
    ├── Integration/
    │   └── Presentation/Support/  # Tests de endpoints HTTP
    └── Helpers/                   # Utilidades compartidas de test
```

### Flujo de dependencias

```
THtracker.API
    └──▶ THtracker.Application
              └──▶ THtracker.Domain
                        ▲
              THtracker.Infrastructure (implementa contratos del Domain)
```

> ✅ `Domain` no depende de nadie. `Infrastructure` implementa las interfaces definidas en `Domain`. `API` solo conoce `Application`. Las dependencias siempre apuntan **hacia adentro**.

---

## 🚀 Inicio Rápido

### Prerrequisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (para PostgreSQL local)
- [EF Core CLI](https://docs.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### Con Docker (recomendado)

```bash
# 1. Clona el repositorio
git clone https://github.com/Santiago-Pineda-M/THtracker-api.git
cd THtracker-api

# 2. Levanta los servicios
docker compose up -d

# API disponible en http://localhost:5000
# Swagger en http://localhost:5000/swagger
```

### Sin Docker

```bash
# 1. Clona el repositorio
git clone https://github.com/Santiago-Pineda-M/THtracker-api.git
cd THtracker-api

# 2. Configura la base de datos (PostgreSQL local)
# Edita appsettings.Development.json con tu connection string

# 3. Aplica migraciones
dotnet ef database update --project THtracker.Infrastructure --startup-project THtracker.API

# 4. Ejecuta la API
cd THtracker.API
dotnet run
```

### Variables de Entorno

```json
// appsettings.json (fragmento)
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=thtracker;Username=postgres;Password=tu_password"
  },
  "Jwt": {
    "SecretKey": "tu_clave_secreta_min_32_chars",
    "Issuer": "https://tu-dominio.com",
    "Audience": "https://tu-dominio.com",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## 📋 Endpoints Principales

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/v1/auth/register` | Registro de usuario (retorna 201) | ❌ |
| `POST` | `/api/v1/auth/login` | Inicio de sesión | ❌ |
| `POST` | `/api/v1/auth/refresh` | Renovar access token (rota refresh token y actualiza sesión) | ❌ |
| `GET` | `/api/v1/sessions` | Listar sesiones activas del usuario autenticado (paginado) | ✅ |
| `POST` | `/api/v1/sessions/{sessionId}/revoke` | Revocar una sesión propia | ✅ |
| `GET` | `/api/v1/activity-logs` | Listar logs de actividad con filtros + paginación | ✅ |
| `GET` | `/api/v1/activity-logs/active` | Listar solo logs activos | ✅ |
| `GET` | `/api/v1/health` | Estado general sin detalles | ❌ |
| `GET` | `/api/v1/health/details` | Estado detallado (solo Admin) | ✅ |

> 📖 Documentación completa disponible en `/swagger` una vez levantada la API.

### Contrato de Refresh Token

`POST /api/v1/auth/refresh` espera body JSON:

```json
{
  "refreshToken": "token_actual"
}
```

No se envía un string plano en el body.

---

## 🔄 CI/CD Pipeline

El pipeline de GitHub Actions ejecuta automáticamente en cada push a `main`:

```yaml
1. ✅ Restore dependencies   (dotnet restore)
2. ✅ Build                  (dotnet build)
3. ✅ Run tests              (dotnet test)
4. ✅ Build Docker image
5. ✅ Push to registry
6. ✅ Deploy to Railway
```

---

## 🐳 Docker

```bash
# Build de la imagen
docker build -t thtracker-api .

# Ejecutar el contenedor
docker run -p 5000:5000 \
  -e ConnectionStrings__Default="..." \
  -e Jwt__SecretKey="..." \
  thtracker-api

# Con Docker Compose (incluye PostgreSQL)
docker compose up -d
```

---

## 📡 Ecosistema THtracker

| Repositorio | Descripción | Estado |
|-------------|-------------|--------|
| **THtracker-api** (este repo) | Backend API — .NET + Clean Architecture | ✅ Activo |
| **[THtrackerApp](https://github.com/Santiago-Pineda-M/THtrackerApp)** | Frontend PWA — React + TypeScript | ✅ Activo |

---

## 👨‍💻 Autor

**Santiago Pineda** — Desarrollador Full Stack

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=flat-square&logo=linkedin)](https://linkedin.com/in/jhonyer-santiago-pineda-marin-dev)
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=flat-square&logo=github)](https://github.com/Santiago-Pineda-M)
[![Email](https://img.shields.io/badge/Email-D14836?style=flat-square&logo=gmail)](mailto:santiago01morfe@gmail.com)

---

<div align="center">

*Si este proyecto te fue útil, considera darle una ⭐*

</div>
