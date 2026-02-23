# Despliegue a producción

Este documento explica cómo configurar despliegue automático desde GitHub Actions hacia un servidor de producción usando Docker y Docker Compose.

Requisitos en el servidor de destino:
- Docker instalado (Engine) y Docker Compose (v2 preferible).
- Un usuario con permisos para ejecutar Docker y acceso SSH con clave privada.

Secrets requeridos en el repositorio GitHub (Settings → Secrets):
- `SSH_HOST`: IP o hostname del servidor.
- `SSH_USER`: usuario SSH.
- `SSH_PORT`: puerto SSH (22 por defecto).
- `SSH_PRIVATE_KEY`: clave privada para autenticación SSH (sin passphrase, o con passphrase si se maneja adecuadamente).
- `GHCR_USER`: usuario para login en GHCR (ej: nombre de usuario de GitHub del propietario del paquete).
- `GHCR_PAT`: Personal Access Token con permisos `read:packages` (y `write:packages` si usas el push desde Actions).

Flujo de trabajo que se genera:
1. Al hacer push a `main` se compila la solución, ejecutan tests si existen.
2. Se construye la imagen Docker usando el `Dockerfile` en `THtracker.API/` y se publica en `ghcr.io`.
3. El workflow copia `docker-compose.prod.yml` al servidor y ejecuta:
   - `docker login ghcr.io`
   - `docker compose pull`
   - `docker compose up -d --remove-orphans`

Pasos mínimos para preparar el servidor:
1. Crear el usuario `deploy` (o usar uno existente) y añadir la clave pública correspondiente a `~/.ssh/authorized_keys`.
2. Instalar Docker y Docker Compose.
3. (Opcional) Crear el directorio `~/deploy`.

Notas y ajustes recomendados:
- Reemplazar `REPLACE_OWNER` en `docker-compose.prod.yml` por el owner correcto o editar el workflow para que sustituya dinámicamente.
- Añadir variables de entorno sensibles en el server (o usar un `.env` en `~/deploy` y referenciarlo desde `docker-compose.yml`).
- Si prefieres desplegar a un proveedor (Azure App Service, AWS ECS, etc.), puedo preparar un workflow específico.
