# Docker Setup and Run Guide for TalentManagementAPI.WebApi

## Why This Matters (Especially for Angular Developers)
When working on the Angular frontend, the fastest workflow is to keep the Web API running in Docker.
You can run Angular locally and point it to a stable API endpoint (`https://localhost:44378`) without repeatedly starting/stopping the API from Visual Studio.

Benefits:
- consistent backend environment across developers
- fewer machine-specific runtime issues
- faster frontend iteration (Angular hot-reload + always-on API)
- lower context switching during local development

## Docker Basics for New Developers
Docker packages an app and its dependencies into containers.
A container is an isolated runtime for a specific service.

In this project, Docker Compose orchestrates multiple containers:
- Web API container (`talentmanagementapi.webapi`)
- SQL Server container (`sqlserver`)

Key concepts:
- image: blueprint used to create a container
- container: running instance of an image
- port mapping: host port -> container port
- volume: host folder mounted inside container
- environment variables: runtime configuration

## How This Project Is Wired
This repository uses two compose files:
- `docker-compose.yml`
  - base definition (service name, image build context, Dockerfile path)
- `docker-compose.override.yml`
  - local-development runtime settings (ports, env vars, volumes)

Compose merges these files when both are supplied.

## Docker YAML Explained (Current Setup)
File: `docker-compose.override.yml`

### `talentmanagementapi.webapi`
- `ASPNETCORE_URLS=https://+:8443;http://+:8080`
  - API listens on HTTPS 8443 and HTTP 8080 inside container.
- `ASPNETCORE_Kestrel__Certificates__Default__Path=/https/talentmanagement-dev.pfx`
- `ASPNETCORE_Kestrel__Certificates__Default__Password=...`
  - Kestrel HTTPS cert config for container TLS.
- `UseInMemoryDatabase=true`
- `FeatureManagement__UseInMemoryDatabase=true`
  - local Docker runs with in-memory DB behavior enabled by config.
- `ConnectionStrings__DefaultConnection=Server=sqlserver,1433;...`
  - API points to SQL container by service name (`sqlserver`).
- `ports: "44378:8443"`
  - Host `44378` -> container HTTPS `8443`.
  - Angular should call `https://localhost:44378`.
- `depends_on: sqlserver`
  - starts SQL service before API container.
- volume mounts:
  - `${USERPROFILE}/.aspnet/https:/https:ro` for dev certificates
  - user secrets mounts for local development

### `sqlserver`
- image: `mcr.microsoft.com/mssql/server:2022-latest`
- `ACCEPT_EULA=Y`
- `SA_PASSWORD=...`
- `ports: "1433"`
  - exposes SQL port with dynamic host mapping (avoids local port conflicts)
- named volume `sqlserver-data`
  - persists database files across container restarts

## Prerequisites
- Docker Desktop running
- .NET SDK installed (for build checks)
- Path:
  - `C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API`

Optional but recommended:
- trusted local HTTPS dev certificate

## Start the API Stack
From project root:

```powershell
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml up --build -d
```

## Verify It Is Running
```powershell
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml ps
```

Check Swagger:
- `https://localhost:44378/swagger/index.html`

Quick endpoint checks:
- `https://localhost:44378/swagger/v1/swagger.json`
- `https://localhost:44378/health`

## Angular Local Configuration
Set Angular API base URL to:
- `https://localhost:44378`

Example (environment file):

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:44378'
};
```

If browser blocks the cert:
- open `https://localhost:44378/swagger/index.html`
- accept/trust certificate for localhost

## Stop the Stack
```powershell
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml down
```

## Useful Commands
View API logs:

```powershell
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml logs -f talentmanagementapi.webapi
```

View SQL logs:

```powershell
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml logs -f sqlserver
```

Rebuild API image after backend code changes:

```powershell
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml up --build -d
```

## Common Issues
- `LocalDB is not supported on this platform`
  - Cause: container is Linux, LocalDB is Windows-only.
  - Fix: ensure Docker connection string points to `sqlserver` service.

- `port is already allocated` (44378)
  - Cause: another container/process already uses that port.
  - Fix: stop/remove conflicting container, then restart compose.

- HTTPS reset / certificate errors
  - Cause: missing or wrong PFX/password in container config.
  - Fix: verify cert file exists in `${USERPROFILE}\.aspnet\https` and password matches compose value.

## Security Note
Current SQL password in local compose is for development only.
Do not reuse in shared/staging/production environments.
Use secrets management for non-local deployments.
