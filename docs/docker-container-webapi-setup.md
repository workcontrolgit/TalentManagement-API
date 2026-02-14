# Docker Setup and Run Guide for TalentManagementAPI.WebApi

## Why This Matters (Especially for Angular Developers)
When working on the Angular frontend, the fastest workflow is to keep the Web API running in Docker.
You can run Angular locally and point it to a stable API endpoint (`https://localhost:44378`) without repeatedly starting/stopping the API from Visual Studio.

Benefits:
- **Consistent backend environment** across developers
- **Fewer machine-specific runtime issues** (no "works on my machine")
- **Faster frontend iteration** (Angular hot-reload + always-on API)
- **Lower context switching** during local development
- **Simplified onboarding** for new team members

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

Docker Compose **automatically merges** these files when both are present in the same directory.

### Build Context Explained

**Important:** The Dockerfile is located in `TalentManagementAPI.WebApi/` but the build context in `docker-compose.yml` is set to `context: ../..` (two directories up - the AngularNetTutorial root).

**Why?** The Dockerfile needs to COPY project files from multiple API subprojects:
- `TalentManagementAPI.WebApi`
- `TalentManagementAPI.Application`
- `TalentManagementAPI.Domain`
- `TalentManagementAPI.Infrastructure.Persistence`
- `TalentManagementAPI.Infrastructure.Shared`

All `COPY` paths in the Dockerfile are relative to the **AngularNetTutorial root**, not the API directory. This is standard for multi-project .NET solutions in Docker.

## Docker YAML Explained (Current Setup)
File: `docker-compose.override.yml`

### Port Mapping Explained
- **Dockerfile base stage** exposes: `8080` (HTTP), `8081` (HTTPS default)
- **Override runtime** changes ports to: `8080` (HTTP), `8443` (HTTPS custom)
- **Host machine** accesses via: `44378` → container `8443` (HTTPS only for external access)
- **Result:** Angular calls `https://localhost:44378` which forwards to container port `8443`

### `talentmanagementapi.webapi` Service

**Environment Variables:**
- `ASPNETCORE_URLS=https://+:8443;http://+:8080`
  - API listens on HTTPS `8443` and HTTP `8080` inside container
- `ASPNETCORE_Kestrel__Certificates__Default__Path=/https/talentmanagement-dev.pfx`
- `ASPNETCORE_Kestrel__Certificates__Default__Password=TalentManagement!Pfx2026#`
  - Kestrel HTTPS certificate configuration for container TLS
- `UseInMemoryDatabase=true`
- `FeatureManagement__UseInMemoryDatabase=true`
  - Enables in-memory database for local development (faster startup, no persistence needed)
- `ConnectionStrings__DefaultConnection=Server=sqlserver,1433;...`
  - API connects to SQL container using Docker network service name (`sqlserver`)

**Networking:**
- `ports: "44378:8443"`
  - Maps host port `44378` to container HTTPS port `8443`
  - Angular frontend calls `https://localhost:44378`
- `depends_on: sqlserver`
  - Ensures SQL Server container starts before API container

**Volume Mounts:**
- `${USERPROFILE}/.aspnet/https:/https:ro` - Development HTTPS certificates (read-only)
- `${APPDATA}/Microsoft/UserSecrets:/home/app/.microsoft/usersecrets:ro` - User secrets for Linux containers
- `${APPDATA}/Microsoft/UserSecrets:/root/.microsoft/usersecrets:ro` - User secrets for root user

### `sqlserver` Service

**Image:**
- `mcr.microsoft.com/mssql/server:2022-latest` - Official Microsoft SQL Server 2022 Linux container

**Environment Variables:**
- `ACCEPT_EULA=Y` - Accepts SQL Server license agreement (required)
- `SA_PASSWORD=Your_password123` - System Administrator password (development only!)

**Networking:**
- `ports: "1433"` - Exposes SQL port with **dynamic host mapping** (avoids local port conflicts)
  - Docker assigns a random available port on the host (e.g., `49153`)
  - Use `docker compose port sqlserver 1433` to find the assigned port

**Storage:**
- Named volume `sqlserver-data` → `/var/opt/mssql`
  - Persists database files across container restarts
  - Data survives `docker compose down` (destroyed only with `docker compose down -v`)

## Prerequisites

**Required:**
- **Docker Desktop** - Running and ready (check system tray icon)
- **.NET SDK 10.0+** - For building the API image
- **Git** - For cloning/updating the repository

**Optional but recommended:**
- Trusted local HTTPS development certificate
- SQL Server Management Studio or Azure Data Studio (for database inspection)

## Quick Start (Recommended)

**Navigate to the API project directory first:**

```powershell
cd C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API
```

**Then use short commands** (Docker Compose auto-discovers `docker-compose.yml` and `docker-compose.override.yml`):

```powershell
# Start all services (API + SQL Server)
docker compose up --build -d

# Check status
docker compose ps

# View API logs (follow mode)
docker compose logs -f talentmanagementapi.webapi

# Stop all services
docker compose down
```

**Flags explained:**
- `--build` - Rebuilds the API image before starting (ensures latest code changes)
- `-d` - Detached mode (runs in background)
- `-f` - Follow logs in real-time

## Alternative: Full Path Commands

If you prefer to run commands from anywhere without navigating to the directory:

```powershell
# Start
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml up --build -d

# Stop
docker compose -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.yml -f C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API\docker-compose.override.yml down
```

**Recommendation:** Use the short commands - they're easier to type and less error-prone.

## Verify It Is Running

**Check container status:**

```powershell
docker compose ps
```

Expected output:
```
NAME                                    STATUS          PORTS
talentmanagementapi-sqlserver-1         Up             0.0.0.0:49153->1433/tcp
talentmanagementapi-webapi-1            Up             0.0.0.0:44378->8443/tcp
```

**Check API endpoints:**

1. **Swagger UI** (interactive API documentation):
   - https://localhost:44378/swagger/index.html

2. **Health check endpoint:**
   ```powershell
   curl https://localhost:44378/health
   # Or visit in browser
   ```

3. **Swagger JSON** (for API clients):
   - https://localhost:44378/swagger/v1/swagger.json

**If browser shows certificate warning:**
- Click "Advanced" → "Proceed to localhost (unsafe)"
- Or trust the development certificate (one-time setup)

**Check container logs for errors:**

```powershell
# API logs
docker compose logs talentmanagementapi.webapi

# SQL Server logs
docker compose logs sqlserver

# All logs combined
docker compose logs
```

## Full CAT Stack Integration

To run the complete authenticated application with the **CAT pattern** (Client, API Resource, Token Service):

### Step 1: Start IdentityServer (Required First)

**Why first?** Both the API and Angular client depend on IdentityServer for authentication.

```powershell
cd C:\apps\AngularNetTutotial\TokenService\Duende-IdentityServer\src\Duende.STS.Identity
dotnet run
```

**Wait for:**
```
Now listening on: https://localhost:44310
```

### Step 2: Start API in Docker (This Guide)

```powershell
cd C:\apps\AngularNetTutotial\ApiResources\TalentManagement-API
docker compose up -d
```

**Verify:** Open https://localhost:44378/swagger/index.html

### Step 3: Start Angular Frontend

```powershell
cd C:\apps\AngularNetTutotial\Clients\TalentManagement-Angular-Material\talent-management
npm start
```

**Verify:** Open http://localhost:4200

### Authentication Flow

1. User visits Angular app → `http://localhost:4200`
2. Login redirects to IdentityServer → `https://localhost:44310`
3. User authenticates (username: `ashtyn1`, password: `Pa$$word123`)
4. IdentityServer issues tokens, redirects back to Angular
5. Angular stores tokens in local storage
6. Angular attaches Bearer token to all API requests → `https://localhost:44378`
7. API validates token against IdentityServer, returns protected data

### Quick Health Check

**All services running?**

```powershell
# IdentityServer
curl https://localhost:44310/.well-known/openid-configuration

# API
curl https://localhost:44378/health

# Angular (browser)
# http://localhost:4200
```

## Angular Local Configuration

**Set Angular API base URL in environment files:**

File: `Clients/TalentManagement-Angular-Material/talent-management/src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:44378/api/v1',
  identityServerUrl: 'https://localhost:44310',
  clientId: 'TalentManagement',
  scope: 'openid profile email roles app.api.talentmanagement.read app.api.talentmanagement.write'
};
```

**If browser blocks HTTPS certificates:**

1. Open `https://localhost:44310` → Accept certificate for IdentityServer
2. Open `https://localhost:44378/swagger/index.html` → Accept certificate for API
3. Refresh Angular app at `http://localhost:4200`

**Common issue:** "invalid_scope" error
- **Cause:** Angular `scope` doesn't match IdentityServer `AllowedScopes`
- **Fix:** Verify `environment.ts` scope matches `TokenService/.../identityserverdata.json`

## Stop the Stack

**Stop containers (preserves database data):**

```powershell
docker compose down
```

**Stop and remove volumes (⚠️ DELETES all database data):**

```powershell
docker compose down -v
```

## Advanced Operations

### Rebuild API Image After Code Changes

**Rebuild and restart only the API** (fastest for API code changes):

```powershell
docker compose build talentmanagementapi.webapi
docker compose up -d talentmanagementapi.webapi
```

**Rebuild everything:**

```powershell
docker compose up --build -d
```

### View Real-Time Logs

**Follow API logs:**

```powershell
docker compose logs -f talentmanagementapi.webapi
```

**Follow SQL Server logs:**

```powershell
docker compose logs -f sqlserver
```

**Follow all logs:**

```powershell
docker compose logs -f
```

**View last 50 lines:**

```powershell
docker compose logs --tail=50 talentmanagementapi.webapi
```

### Access SQL Server Directly

**Get the dynamically assigned SQL port:**

```powershell
docker compose port sqlserver 1433
# Example output: 0.0.0.0:49153
```

**Connect using SQL Server Management Studio or Azure Data Studio:**

```
Server: localhost,49153
Authentication: SQL Server Authentication
Login: sa
Password: Your_password123
```

**Execute SQL commands from command line:**

```powershell
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -Q "SELECT name FROM sys.databases"
```

### Debugging Inside Containers

**Open bash shell in API container:**

```powershell
docker compose exec talentmanagementapi.webapi /bin/bash
```

**Check environment variables:**

```powershell
docker compose exec talentmanagementapi.webapi env | grep ASPNETCORE
```

**Inspect container details:**

```powershell
docker inspect talentmanagementapi-webapi-1
```

### Resource Management

**View resource usage:**

```powershell
docker stats
```

**Remove unused images/containers:**

```powershell
docker system prune -a
```

**View all volumes:**

```powershell
docker volume ls
```

**Remove specific volume (⚠️ deletes data):**

```powershell
docker volume rm talentmanagement-api_sqlserver-data
```

## Common Issues & Troubleshooting

### `LocalDB is not supported on this platform`

**Problem:** API container fails to start with LocalDB error

**Cause:** Container runs Linux, LocalDB is Windows-only

**Solution:** Verify `docker-compose.override.yml` connection string uses `Server=sqlserver,1433` (not LocalDB)

```powershell
# Check connection string
docker compose config | grep ConnectionStrings
```

### `port is already allocated` (44378 or 1433)

**Problem:** Container won't start, port conflict error

**Cause:** Another container or process already uses the port

**Solution:**

```powershell
# Find what's using port 44378 (Windows)
netstat -ano | findstr :44378

# Stop conflicting container
docker ps
docker stop <container_id>

# Or change port in docker-compose.override.yml
ports:
  - "44379:8443"  # Use different host port
```

### HTTPS certificate errors

**Problem:** Browser shows "Your connection is not private" or API won't start

**Cause:** Missing or incorrect development certificate

**Solution:**

```powershell
# Generate development certificate (one-time setup)
dotnet dev-certs https --clean
dotnet dev-certs https -ep ${env:USERPROFILE}\.aspnet\https\talentmanagement-dev.pfx -p TalentManagement!Pfx2026#
dotnet dev-certs https --trust

# Verify certificate file exists
dir $env:USERPROFILE\.aspnet\https\talentmanagement-dev.pfx

# Restart containers
docker compose down
docker compose up -d
```

### Container starts but API doesn't respond

**Problem:** Container shows "Up" status but API endpoints return nothing

**Cause:** Application crashed inside container

**Solution:**

```powershell
# Check logs for errors
docker compose logs talentmanagementapi.webapi

# Common errors:
# - Missing appsettings.json
# - Database migration failures
# - Missing environment variables

# Exec into container to investigate
docker compose exec talentmanagementapi.webapi /bin/bash
cd /app
ls -la
```

### SQL Server container fails to start

**Problem:** SQL container exits immediately

**Cause:** Insufficient memory allocation to Docker Desktop

**Solution:**

1. Open Docker Desktop → Settings → Resources
2. Increase Memory to at least **4 GB**
3. Click "Apply & Restart"
4. Restart containers: `docker compose up -d`

### API returns 401 Unauthorized

**Problem:** Angular receives 401 errors for all API calls

**Cause:** IdentityServer not running or token validation failure

**Solution:**

```powershell
# 1. Verify IdentityServer is running
curl https://localhost:44310/.well-known/openid-configuration

# 2. Check API logs for token validation errors
docker compose logs talentmanagementapi.webapi | findstr "token"

# 3. Verify IdentityServer Authority in API config
docker compose exec talentmanagementapi.webapi cat /app/appsettings.json | grep Authority
```

### Database changes not persisting

**Problem:** Data disappears after restarting containers

**Cause:** Using in-memory database or volume not mounted correctly

**Solution:**

```powershell
# Check if in-memory database is enabled
docker compose config | grep UseInMemoryDatabase

# If you want persistent SQL Server database, disable in-memory mode:
# Edit docker-compose.override.yml:
# - UseInMemoryDatabase=false
# - FeatureManagement__UseInMemoryDatabase=false

# Verify volume is mounted
docker volume ls | findstr sqlserver
docker compose down
docker compose up -d
```

## Production Considerations

### Security Best Practices

**⚠️ WARNING:** Current configuration is for **local development only!**

**DO NOT use in production:**
- ❌ Hardcoded passwords in `docker-compose.override.yml`
- ❌ Development HTTPS certificates
- ❌ `TrustServerCertificate=True` in connection strings
- ❌ `ASPNETCORE_ENVIRONMENT=Development`

### Using Environment Variables for Secrets

**For non-local environments**, use `.env` file (never commit this file!):

**Step 1:** Create `.env` file in the same directory as `docker-compose.yml`:

```env
# .env (add to .gitignore!)
SA_PASSWORD=YourSecureProductionPassword123!
CERT_PASSWORD=YourSecureCertPassword456!
JWT_SECRET=YourSecureJwtSecret789!
```

**Step 2:** Update `docker-compose.override.yml` to reference variables:

```yaml
services:
  talentmanagementapi.webapi:
    environment:
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=TalentManagementAPIDb;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;Encrypt=True

  sqlserver:
    environment:
      - SA_PASSWORD=${SA_PASSWORD}
```

**Step 3:** Add `.env` to `.gitignore`:

```gitignore
.env
*.env.local
*.env.production
```

### Production Deployment Checklist

For staging/production deployments:

- [ ] Use **Azure Key Vault**, **AWS Secrets Manager**, or **Docker Secrets**
- [ ] Replace development certificates with **valid SSL/TLS certificates** (Let's Encrypt, Azure App Service, etc.)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Use **managed databases** (Azure SQL, AWS RDS) instead of containerized SQL Server
- [ ] Enable **connection encryption** (`Encrypt=True`, `TrustServerCertificate=False`)
- [ ] Implement **health checks** and **monitoring** (Application Insights, Prometheus)
- [ ] Use **image scanning** (Docker Scout, Trivy) to check for vulnerabilities
- [ ] Configure **resource limits** (CPU, memory) in compose file
- [ ] Use **specific image tags** (not `:latest`) for reproducible builds
- [ ] Implement **log aggregation** (ELK Stack, Azure Monitor)

### Example: Production-Ready Compose Configuration

```yaml
services:
  talentmanagementapi.webapi:
    image: myregistry.azurecr.io/talentmanagementapi:1.2.3  # Specific tag
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:8443
      - ConnectionStrings__DefaultConnection=${DATABASE_CONNECTION_STRING}  # Azure SQL
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "https://localhost:8443/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### References

- [Docker Compose Production Guide](https://docs.docker.com/compose/production/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [SQL Server Container Best Practices](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)
