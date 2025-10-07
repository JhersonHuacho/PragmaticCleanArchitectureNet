# Docker Compose - Guía Completa y Mejores Prácticas

## ?? Tabla de Contenidos
1. [Configuración Final del Docker Compose](#configuración-final-del-docker-compose)
2. [Mejores Prácticas Implementadas](#mejores-prácticas-implementadas)
3. [Volúmenes: Named vs Bind Mount](#volúmenes-named-vs-bind-mount)
4. [Variables de Entorno y Registry](#variables-de-entorno-y-registry)
5. [Desarrollo Local vs Docker](#desarrollo-local-vs-docker)
6. [Docker Ignore - Optimización de Build](#docker-ignore---optimización-de-build)
7. [Ejecución: Visual Studio vs Terminal](#ejecución-visual-studio-vs-terminal)
8. [Gestión de Cambios en Código](#gestión-de-cambios-en-código)
9. [Debugging y Troubleshooting](#debugging-y-troubleshooting)
10. [Comandos Útiles](#comandos-útiles)
11. [Problemas Comunes y Soluciones](#problemas-comunes-y-soluciones)
12. [Estructura del Proyecto](#estructura-del-proyecto)

---

## ?? Configuración Final del Docker Compose

```yaml
version: '3.4'

name: bookify

services:
  bookify.api:
    image: ${DOCKER_REGISTRY-}bookifyapi
    container_name: Bookify.Api
    build:
      context: .
      dockerfile: Bookify.Api/Dockerfile
    environment:
      - SEED_DATA=true  # Cambiar a false después del primer run
    depends_on:
      bookify-db:
        condition: service_healthy
    ports:
      - "5000:80"
      
  bookify-db:
    image: postgres:13.22-alpine
    container_name: Bookify.Db
    restart: unless-stopped
    environment:
      - POSTGRES_DB=bookify
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d bookify"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 30s
  
  bookify-idp:
    image: quay.io/keycloak/keycloak:23.0.0
    container_name: Bookify.Identity
    command: start-dev --import-realm
    restart: unless-stopped
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
    volumes:
      - keycloak_data:/opt/keycloak/data
      - ./.files/bookify-realm-export.json:/opt/keycloak/data/import/realm.json
    ports:
      - "18080:8080"

volumes:
  postgres_data:
  keycloak_data:
```

---

## ? Mejores Prácticas Implementadas

### 1. **Versión Específica de PostgreSQL**
```yaml
# ? Buena práctica
image: postgres:13.22-alpine

# ? Evitar
image: postgres:latest
```
**Por qué:** Evita problemas de compatibilidad entre versiones y garantiza consistencia.

### 2. **Nombre del Proyecto Explícito**
```yaml
name: bookify
```
**Por qué:** Evita prefijos aleatorios generados por Visual Studio.

### 3. **Contenedores con Nombres Específicos**
```yaml
container_name: Bookify.Api
container_name: Bookify.Db
container_name: Bookify.Identity
```
**Por qué:** Facilita la identificación y gestión de contenedores.

### 4. **Health Checks**
```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres -d bookify"]
  interval: 30s
  timeout: 10s
  retries: 3
```
**Por qué:** Asegura que los servicios estén completamente listos antes de iniciar servicios dependientes.

### 5. **Restart Policy**
```yaml
restart: unless-stopped
```
**Por qué:** Los contenedores se reinician automáticamente en caso de fallo.

### 6. **Dependencias con Condiciones**
```yaml
depends_on:
  bookify-db:
    condition: service_healthy
```
**Por qué:** Espera a que la base de datos esté completamente lista antes de iniciar la API.

---

## ?? Volúmenes: Named vs Bind Mount

### **Named Volumes (Recomendado para Datos)**
```yaml
volumes:
  - postgres_data:/var/lib/postgresql/data
```

**Ventajas:**
- ? Gestión automática por Docker
- ? Mejor rendimiento
- ? Portabilidad entre sistemas
- ? Backup/restore más fácil
- ? Sin problemas de permisos
- ? Aislamiento y seguridad

**Ubicación física:**
- Windows: `C:\Users\[usuario]\AppData\Local\Docker\wsl\data\`
- Linux: `/var/lib/docker/volumes/`
- macOS: `~/Library/Containers/com.docker.docker/Data/vms/0/`

### **Bind Mounts (Para Archivos de Configuración)**
```yaml
volumes:
  - ./.files/bookify-realm-export.json:/opt/keycloak/data/import/realm.json
```

**Cuándo usar:**
- ? Archivos de configuración
- ? Desarrollo (acceso directo desde host)
- ? Logs que necesitas ver fácilmente

### **Comandos de Gestión de Volúmenes:**
```bash
# Ver volúmenes
docker volume ls

# Inspeccionar volumen
docker volume inspect bookify_postgres_data

# Backup de volumen
docker run --rm -v bookify_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/db-backup.tar.gz /data

# Restore de volumen
docker run --rm -v bookify_postgres_data:/data -v $(pwd):/backup alpine tar xzf /backup/db-backup.tar.gz -C /

# Eliminar volumen (¡CUIDADO!)
docker volume rm bookify_postgres_data
```

---

## ?? Variables de Entorno y Registry

### **¿Qué hace `${DOCKER_REGISTRY-}bookifyapi`?**

```yaml
image: ${DOCKER_REGISTRY-}bookifyapi
```

**Comportamiento:**
- Si `DOCKER_REGISTRY` está definida: `miregistry.com/bookifyapi`
- Si `DOCKER_REGISTRY` NO está definida: `bookifyapi`

**Casos de uso:**
```bash
# Desarrollo local
DOCKER_REGISTRY=  # Resultado: bookifyapi

# Staging
DOCKER_REGISTRY=staging.mycompany.com/  # Resultado: staging.mycompany.com/bookifyapi

# Producción
DOCKER_REGISTRY=prod.mycompany.com/  # Resultado: prod.mycompany.com/bookifyapi

# Docker Hub
DOCKER_REGISTRY=jhersonhuacho/  # Resultado: jhersonhuacho/bookifyapi
```

**Archivo .env recomendado:**
```bash
# .env
DOCKER_REGISTRY=
COMPOSE_PROJECT_NAME=bookify
ASPNETCORE_ENVIRONMENT=Development
```

---

## ?? Desarrollo Local vs Docker

### **Problema Identificado**
Cuando ejecutas la API localmente (F5 en Visual Studio) con Docker Compose ejecutándose, surgen conflictos de conexión porque:

- **Docker Compose**: Usa hostnames internos (`bookify-db`, `bookify-idp`)
- **Local Development**: Necesita `localhost` para conectarse a los servicios de Docker

### **Solución: Configuración Dual**

#### **1. Archivo appsettings.Local.json**
```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=bookify;Username=postgres;Password=postgres;"
  },
  "Authentication": {
    "Audience": "account",
    "ValidIssuer": "http://localhost:18080/realms/bookify",
    "MetadataUrl": "http://localhost:18080/realms/bookify/.well-known/openid-configuration",
    "RequireHttpsMetadata": false
  },
  "Keycloak": {
    "BaseUrl": "http://localhost:18080",
    "AdminUrl": "http://localhost:18080/admin/realms/bookify/",
    "TokenUrl": "http://localhost:18080/realms/bookify/protocol/openid-connect/token",
    "AdminClientId": "bookify-admin-client",
    "AdminClientSecret": "UZDmbNxWmV4TlpaCRcju6pMRsyuV3er1",
    "AuthClientId": "bookify-auth-client",
    "AuthClientSecret": "3E3yvXaYppoYBF3Ir6DgtEzADKKzSurZ"
  }
}
```

#### **2. Modificación en Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Cargar configuración local si está definida
var configEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_CONFIGURATION");
if (!string.IsNullOrEmpty(configEnvironment))
{
    builder.Configuration.AddJsonFile($"appsettings.{configEnvironment}.json", optional: true, reloadOnChange: true);
}

// Resto de la configuración...

// SeedData condicional
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigration();
    
    // Solo ejecutar seed si la variable de entorno está presente
    var shouldSeed = Environment.GetEnvironmentVariable("SEED_DATA");
    if (!string.IsNullOrEmpty(shouldSeed) && shouldSeed.ToLower() == "true")
    {
        app.SeedData();
    }
}
```

#### **3. launchSettings.json para Desarrollo Local**
```json
{
  "profiles": {
    "Local Bookify.Api": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_CONFIGURATION": "Local"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
```

### **Flujos de Trabajo**

#### **Desarrollo Local (Debugging)**
```bash
# 1. Levantar solo infraestructura
docker-compose up -d bookify-db bookify-idp

# 2. Ejecutar API desde Visual Studio con perfil "Local Bookify.Api"
# Esto usará localhost para conectarse a los servicios de Docker
```

#### **Desarrollo Full Docker**
```bash
# 1. Levantar todo el stack
docker-compose up -d

# 2. Para cambios de código:
docker-compose up -d --build bookify.api
```

### **¿Cuándo usar cada uno?**

| Escenario | Configuración | Ventajas |
|-----------|--------------|----------|
| **Debugging activo** | Local + Docker (DB/Keycloak) | ? Breakpoints<br>? Hot reload<br>? Performance |
| **Testing de integración** | Full Docker | ? Ambiente real<br>? Networking interno<br>? Consistency |
| **CI/CD** | Full Docker | ? Reproducibilidad<br>? Isolation<br>? Deployment ready |

---

## ?? Docker Ignore - Optimización de Build

### **¿Por qué es importante .dockerignore?**

El archivo `.dockerignore` excluye archivos y directorios del contexto de build, lo que:
- ? **Reduce el tamaño** del contexto de build
- ? **Acelera el proceso** de build
- ? **Mejora la seguridad** excluyendo archivos sensibles
- ? **Optimiza el cache** de Docker layers

### **Archivo .dockerignore recomendado:**

```dockerfile
# .dockerignore

# Binaries
**/bin/
**/obj/
**/out/
**/publish/

# Visual Studio
.vs/
.vscode/
*.user
*.suo
*.cache

# Build artifacts
**/wwwroot/dist/
**/ClientApp/node_modules/
**/ClientApp/build/

# Tests
**/*Tests/
**/TestResults/

# Package files
*.nupkg
*.snupkg

# Git
.git/
.gitignore
.gitattributes

# Documentation
*.md
docs/

# Docker files
Dockerfile*
docker-compose*
.dockerignore

# Environment files
.env
.env.*
!.env.example

# Logs
logs/
*.log

# Temporary files
**/tmp/
**/temp/
**/.tmp/

# OS generated files
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db

# IDE
**/.idea/
**/nbproject/

# NPM (si usas frontend)
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*

# Coverage reports
**/coverage/
**/*.lcov

# Runtime data
pids/
*.pid
*.seed
*.pid.lock

# Database files (si los tienes localmente)
**/data/
*.db
*.sqlite
*.sqlite3

# Specific to this project
.files/
postgres_data/
keycloak_data/
Docker-Guide.md
```

### **Impacto en el Build Process:**

#### **Sin .dockerignore:**
```bash
# Build context incluye TODOS los archivos
Sending build context to Docker daemon  890.5MB
Step 1/15 : FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
```

#### **Con .dockerignore:**
```bash
# Build context optimizado
Sending build context to Docker daemon  45.2MB
Step 1/15 : FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
```

### **Verificar qué se incluye en el contexto:**
```bash
# Ver archivos que se envían al daemon
docker build --no-cache --progress=plain -t test-context .

# O usar esta utilidad
docker run --rm -v $(pwd):/workspace alpine find /workspace -type f | head -20
```

---

## ?? Ejecución: Visual Studio vs Terminal

### **Desde Visual Studio (F5)**

#### **Configuración en launchSettings.json:**
```json
{
  "profiles": {
    "Container (Dockerfile)": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
      "environmentVariables": {
        "ASPNETCORE_URLS": "https://+:443;http://+:80"
      },
      "publishAllPorts": true,
      "useSSL": true
    }
  }
}
```

**Comportamiento:**
- ? Debugging integrado
- ? Hot reload automático
- ? Puede usar prefijos aleatorios (`dockercompose871726160203352320_`)
- ? Más difícil control desde terminal
- ? Configuraciones específicas de desarrollo

**Visual Studio hace internamente:**
```bash
# Visual Studio ejecuta algo similar a:
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
# Con nombres de proyecto aleatorios
```

### **Desde Terminal**

#### **Comandos principales:**
```bash
# Comando principal
docker-compose up -d

# Comando con rebuild
docker-compose up -d --build

# Solo servicios específicos
docker-compose up -d bookify-db bookify-idp
```

**Comportamiento:**
- ? Control total sobre el ciclo de vida
- ? Respeta el nombre del proyecto definido
- ? Flexibilidad en configuración
- ? Mejor para producción y staging
- ? No tiene debugging integrado

### **Comparación de Flujos:**

| Aspecto | Visual Studio | Terminal |
|---------|--------------|----------|
| **Debugging** | ? Breakpoints, Watch | ? Solo logs |
| **Hot Reload** | ? Automático | ? Manual rebuild |
| **Control** | ? Limitado | ? Total |
| **Naming** | ? Prefijos aleatorios | ? Consistente |
| **CI/CD Ready** | ? No | ? Sí |
| **Learning Curve** | ? Fácil | ?? Media |

---

## ?? Gestión de Cambios en Código

### **Problema Principal**
Cuando desarrollas con Docker, cada cambio en el código requiere rebuilding de la imagen porque Docker copia el código en build time, no en runtime.

### **Estrategias de Rebuild**

#### **1. Rebuild Selectivo (Recomendado)**
```bash
# Solo rebuilder el servicio que cambió
docker-compose up -d --build bookify.api

# O paso a paso:
docker-compose stop bookify.api
docker-compose build bookify.api
docker-compose up -d bookify.api
```

#### **2. Rebuild Completo**
```bash
# Rebuilder todo desde cero
docker-compose down
docker-compose up --build
```

#### **3. Rebuild con Cache Bust**
```bash
# Forzar rebuild sin cache (más lento pero más confiable)
docker-compose build --no-cache bookify.api
docker-compose up -d bookify.api
```

### **Optimización del Dockerfile para Desarrollo**

#### **Multi-stage con cache optimization:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar solo archivos de proyecto primero (para cache de restore)
COPY ["Bookify.Api/Bookify.Api.csproj", "Bookify.Api/"]
COPY ["Bookify.Application/Bookify.Application.csproj", "Bookify.Application/"]
COPY ["Bookify.Domain/Bookify.Domain.csproj", "Bookify.Domain/"]
COPY ["Bookify.Infrastructure/Bookify.Infrastructure.csproj", "Bookify.Infrastructure/"]

# Restore (se cachea si no cambian las dependencias)
RUN dotnet restore "./Bookify.Api/Bookify.Api.csproj"

# Copiar el código fuente (esto invalidará cache cuando cambie código)
COPY . .
WORKDIR "/src/Bookify.Api"
RUN dotnet build "./Bookify.Api.csproj" -c Release -o /app/build
```

### **Scripts de Automatización**

#### **Windows (rebuild-api.bat):**
```batch
@echo off
echo Rebuilding Bookify API...
docker-compose stop bookify.api
docker-compose build bookify.api
docker-compose up -d bookify.api
echo API rebuilt and started!
docker-compose logs -f bookify.api
```

#### **Linux/Mac (rebuild-api.sh):**
```bash
#!/bin/bash
echo "Rebuilding Bookify API..."
docker-compose stop bookify.api
docker-compose build bookify.api
docker-compose up -d bookify.api
echo "API rebuilt and started!"
docker-compose logs -f bookify.api
```

### **Desarrollo Híbrido (Recomendado)**

```bash
# 1. Levantar infraestructura en Docker
docker-compose up -d bookify-db bookify-idp

# 2. Desarrollar API localmente con Visual Studio
# Usar perfil "Local Bookify.Api" que apunta a localhost

# 3. Para testing de integración
docker-compose up -d --build bookify.api
```

---

## ?? Debugging y Troubleshooting

### **Estrategias de Debugging**

#### **1. Debugging Local (Desarrollo activo)**
```bash
# Infraestructura en Docker, API local
docker-compose up -d bookify-db bookify-idp
# Ejecutar API desde Visual Studio (F5)
```

**Ventajas:**
- ? Breakpoints completos
- ? Watch variables
- ? Hot reload inmediato
- ? Performance nativo

#### **2. Debugging en Contenedor**
```bash
# Entrar al contenedor en ejecución
docker exec -it Bookify.Api /bin/bash

# Ver logs en tiempo real
docker-compose logs -f bookify.api

# Ejecutar comandos dentro del contenedor
docker exec -it Bookify.Api dotnet --version
```

#### **3. Remote Debugging (Avanzado)**
Para debugging remoto en contenedor, modificar Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
# Instalar debugging tools
RUN apk add --no-cache unzip curl
# Descargar vsdbg para remote debugging
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
```

### **Comandos de Diagnóstico**

#### **Estado de Servicios:**
```bash
# Ver estado de todos los servicios
docker-compose ps

# Ver servicios en Docker (no solo compose)
docker ps -a

# Ver logs de todos los servicios
docker-compose logs

# Logs específicos con timestamps
docker-compose logs -f -t bookify.api
```

#### **Conectividad entre Contenedores:**
```bash
# Probar conectividad desde API a DB
docker exec -it Bookify.Api ping bookify-db

# Probar puerto específico
docker exec -it Bookify.Api nc -zv bookify-db 5432

# Probar resolución DNS
docker exec -it Bookify.Api nslookup bookify-db
```

#### **Inspección de Contenedores:**
```bash
# Ver configuración completa del contenedor
docker inspect Bookify.Api

# Ver solo la configuración de red
docker inspect Bookify.Api --format='{{.NetworkSettings}}'

# Ver variables de entorno
docker inspect Bookify.Api --format='{{.Config.Env}}'
```

#### **Performance Monitoring:**
```bash
# Ver uso de recursos en tiempo real
docker stats

# Solo servicios específicos
docker stats Bookify.Api Bookify.Db

# Información del sistema Docker
docker system df
docker system info
```

### **Logs Avanzados**

#### **Configuración de Logging en ASP.NET Core:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

#### **Configuración de Logging en Docker Compose:**
```yaml
services:
  bookify.api:
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

---

## ??? Comandos Útiles

### **Gestión General**
```bash
# Iniciar todos los servicios
docker-compose up -d

# Iniciar servicios específicos
docker-compose up -d bookify-db bookify-idp

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (¡CUIDADO!)
docker-compose down -v

# Reiniciar servicios
docker-compose restart

# Reconstruir imágenes
docker-compose up -d --build

# Ver estado de servicios
docker-compose ps

# Ver logs de todos los servicios
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f bookify-db
```

### **Gestión por Contenedor Individual**
```bash
# Iniciar contenedor específico
docker start Bookify.Db

# Detener contenedor específico
docker stop Bookify.Db

# Reiniciar contenedor específico
docker restart Bookify.Db

# Ver logs de contenedor específico
docker logs Bookify.Db -f

# Ejecutar comando en contenedor
docker exec -it Bookify.Db psql -U postgres -d bookify

# Inspeccionar contenedor
docker inspect Bookify.Db

# Ver estadísticas de recursos
docker stats Bookify.Db
```

### **Comandos de Construcción de Imágenes**

#### **Docker Build:**
```bash
# Build básico
docker build -t bookifyapi .

# Build con tag específico
docker build -t bookifyapi:1.0 .

# Build sin cache (fuerza rebuild completo)
docker build --no-cache -t bookifyapi .

# Build con argumentos
docker build --build-arg BUILD_CONFIGURATION=Release -t bookifyapi .

# Ver progreso detallado
docker build --progress=plain -t bookifyapi .

# Etiquetar imagen para registro
docker tag bookifyapi:latest myregistry.com/bookifyapi:latest
```

#### **Docker Compose Build:**
```bash
# Build todos los servicios
docker-compose build

# Build servicio específico
docker-compose build bookify.api

# Build sin cache
docker-compose build --no-cache

# Build en paralelo
docker-compose build --parallel

# Build y ejecutar
docker-compose up --build
```

### **Orden de Comandos Recomendado**

#### **Primera Ejecución:**
```bash
# 1. Crear volumes y network
docker-compose up --no-start

# 2. Build imágenes
docker-compose build

# 3. Iniciar servicios
docker-compose up -d

# 4. Verificar estado
docker-compose ps
```

#### **Desarrollo Diario:**
```bash
# 1. Verificar estado
docker-compose ps

# 2. Iniciar si está detenido
docker-compose up -d

# 3. Ver logs si hay problemas
docker-compose logs -f

# 4. Para cambios de código
docker-compose up -d --build bookify.api
```

#### **Limpieza Completa:**
```bash
# 1. Detener servicios
docker-compose down

# 2. Eliminar volúmenes (opcional)
docker-compose down -v

# 3. Limpiar imágenes no utilizadas
docker image prune

# 4. Limpiar sistema completo (¡CUIDADO!)
docker system prune -a --volumes
```

### **Scripts de Automatización**

#### **docker-control.bat (Windows):**
```batch
@echo off
setlocal

if "%1"=="up" (
    echo Starting Bookify services...
    docker-compose up -d
    docker-compose ps
) else if "%1"=="down" (
    echo Stopping Bookify services...
    docker-compose down
) else if "%1"=="restart" (
    echo Restarting Bookify services...
    docker-compose restart
    docker-compose ps
) else if "%1"=="rebuild" (
    echo Rebuilding API...
    docker-compose up -d --build bookify.api
) else if "%1"=="logs" (
    if "%2"=="" (
        docker-compose logs -f
    ) else (
        docker-compose logs -f %2
    )
) else if "%1"=="clean" (
    echo Cleaning up...
    docker-compose down -v
    docker system prune -f
) else if "%1"=="status" (
    docker-compose ps
    echo.
    docker stats --no-stream
) else (
    echo Uso: docker-control [up|down|restart|rebuild|logs|clean|status]
    echo.
    echo   up      - Iniciar servicios
    echo   down    - Detener servicios
    echo   restart - Reiniciar servicios
    echo   rebuild - Reconstruir API
    echo   logs    - Ver logs (opcional: logs [servicio])
    echo   clean   - Limpiar todo
    echo   status  - Ver estado
)
```

#### **docker-control.sh (Linux/Mac):**
```bash
#!/bin/bash

case "$1" in
    up)
        echo "Starting Bookify services..."
        docker-compose up -d
        docker-compose ps
        ;;
    down)
        echo "Stopping Bookify services..."
        docker-compose down
        ;;
    restart)
        echo "Restarting Bookify services..."
        docker-compose restart
        docker-compose ps
        ;;
    rebuild)
        echo "Rebuilding API..."
        docker-compose up -d --build bookify.api
        ;;
    logs)
        if [ -z "$2" ]; then
            docker-compose logs -f
        else
            docker-compose logs -f "$2"
        fi
        ;;
    clean)
        echo "Cleaning up..."
        docker-compose down -v
        docker system prune -f
        ;;
    status)
        docker-compose ps
        echo
        docker stats --no-stream
        ;;
    *)
        echo "Usage: $0 {up|down|restart|rebuild|logs|clean|status}"
        echo
        echo "  up      - Start services"
        echo "  down    - Stop services" 
        echo "  restart - Restart services"
        echo "  rebuild - Rebuild API"
        echo "  logs    - View logs (optional: logs [service])"
        echo "  clean   - Clean everything"
        echo "  status  - View status"
        ;;
esac
```

### **¿Qué hace Docker internamente?**

#### **docker-compose up -d:**
```bash
# Docker internamente ejecuta:
# 1. Crear network (si no existe)
docker network create bookify_default

# 2. Crear volúmenes (si no existen)
docker volume create bookify_postgres_data
docker volume create bookify_keycloak_data

# 3. Build imágenes (si no existen)
docker build -t bookify-bookifyapi .

# 4. Crear y ejecutar contenedores
docker run -d --name Bookify.Db --network bookify_default postgres:13.22-alpine
docker run -d --name Bookify.Identity --network bookify_default keycloak:23.0.0
docker run -d --name Bookify.Api --network bookify_default bookify-bookifyapi
```

#### **Jerarquía de Comandos Docker:**
```
Docker Engine
??? Images (docker build, docker pull)
??? Containers (docker run, docker exec)
??? Volumes (docker volume)
??? Networks (docker network)
??? Compose (docker-compose up/down)
    ??? Orchestrates all above
    ??? Manages dependencies
    ??? Handles service discovery
```

---

## ?? Problemas Comunes y Soluciones

### **1. PostgreSQL: "directory exists but is not empty"**

**Problema:**
```
initdb: error: directory "/var/lib/postgresql/data" exists but is not empty
```

**Causas:**
- Datos corruptos de ejecución anterior
- Cambio de versión de PostgreSQL
- Parada incorrecta del contenedor

**Soluciones:**
```bash
# Opción 1: Limpiar volumen
docker-compose down
docker volume rm bookify_postgres_data
docker-compose up -d

# Opción 2: Limpiar directorio (si usas bind mount)
docker-compose down
rmdir /s postgres_data
docker-compose up -d

# Opción 3: Forzar recreación
docker-compose down -v
docker-compose up -d
```

### **2. Error de Conexión: "No such host is known"**

**Problema:**
```
System.Net.Sockets.SocketException: No such host is known.
```

**Causa:**
La API local intenta conectarse a `bookify-db` pero debe usar `localhost`.

**Solución:**
```bash
# 1. Usar configuración local (appsettings.Local.json)
# 2. Configurar variable de entorno
set ASPNETCORE_CONFIGURATION=Local

# 3. O ejecutar todo en Docker
docker-compose up -d
```

### **3. Keycloak: "You need local access to create the initial admin user"**

**Problema:**
Variables de entorno incorrectas para Keycloak 23.0.0

**Solución:**
```bash
# 1. Usar variables correctas en docker-compose.yml
environment:
  - KEYCLOAK_ADMIN=admin
  - KEYCLOAK_ADMIN_PASSWORD=admin

# 2. Limpiar volumen de Keycloak
docker-compose down
docker volume rm bookify_keycloak_data
docker-compose up -d
```

**Variables deprecadas (NO usar):**
```yaml
# ? Deprecadas en v23.0.0
- KEYCLOAK_USER=admin
- KEYCLOAK_PASSWORD=admin
```

### **4. Docker Compose no responde desde terminal**

**Problema:**
`docker-compose down` no funciona después de ejecutar desde Visual Studio

**Causa:**
Visual Studio usa prefijos aleatorios para los volúmenes

**Identificación:**
```bash
# Ver volúmenes reales
docker volume ls
# Resultado: dockercompose871726160203352320_postgres_data

# Ver contenedores activos
docker ps
```

**Soluciones:**
```bash
# Opción 1: Usar nombres reales
docker volume rm dockercompose871726160203352320_keycloak_data

# Opción 2: Detener por nombres de contenedor
docker stop Bookify.Api Bookify.Db Bookify.Identity
docker rm Bookify.Api Bookify.Db Bookify.Identity

# Opción 3: Forzar desde terminal
docker-compose down --remove-orphans
docker-compose up -d
```

### **5. Problemas de puertos ocupados**

**Problema:**
```
Error: Port 5432 is already in use
```

**Soluciones:**
```bash
# Ver qué está usando el puerto
netstat -ano | findstr :5432

# Detener servicios específicos
docker stop $(docker ps --filter "publish=5432" -q)

# Cambiar puerto en docker-compose.yml
ports:
  - "5433:5432"  # Puerto host diferente
```

### **6. SeedData() ejecutándose repetidamente**

**Problema:**
Los datos se insertan múltiples veces en cada restart.

**Solución 1: Variable de entorno**
```csharp
// En Program.cs
var shouldSeed = Environment.GetEnvironmentVariable("SEED_DATA");
if (!string.IsNullOrEmpty(shouldSeed) && shouldSeed.ToLower() == "true")
{
    app.SeedData();
}
```

```yaml
# En docker-compose.yml
environment:
  - SEED_DATA=false  # Cambiar a true solo cuando necesite
```

**Solución 2: Verificación en base de datos**
```csharp
// En el método SeedData
public static void SeedData(this IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Solo hacer seed si no hay datos
    if (!dbContext.Apartments.Any())
    {
        // Ejecutar seed...
    }
}
```

### **7. Problemas de permisos (Windows)**

**Problema:**
Archivos no accesibles desde contenedor

**Soluciones:**
```bash
# Verificar que Docker Desktop tenga acceso a la unidad
# Settings > Resources > File Sharing

# Usar rutas absolutas
volumes:
  - C:/mi/proyecto/data:/var/lib/postgresql/data

# O usar volúmenes nombrados (recomendado)
volumes:
  - postgres_data:/var/lib/postgresql/data
```

### **8. Contenedor se detiene inmediatamente**

**Problema:**
El contenedor se inicia y se detiene inmediatamente.

**Diagnóstico:**
```bash
# Ver logs del contenedor
docker logs Bookify.Api

# Ver último exit code
docker ps -a

# Inspeccionar configuración
docker inspect Bookify.Api
```

**Soluciones comunes:**
```bash
# 1. Verificar que la aplicación no termine inmediatamente
# 2. Revisar configuración de puertos
# 3. Verificar variables de entorno
# 4. Ejecutar en modo interactivo para debug
docker run -it --rm bookifyapi /bin/bash
```

---

## ?? Estructura del Proyecto

```
E:\RepoNet\milanjovanovic\Bookify\
??? Bookify.Api/
?   ??? Dockerfile                 # ?? Usado por docker-compose
?   ??? Bookify.Api.csproj
?   ??? Program.cs                 # ?? Configuración dual Local/Docker
?   ??? Properties/
?   ?   ??? launchSettings.json    # ?? Perfiles de ejecución
?   ??? appsettings.json
?   ??? appsettings.Development.json  # ?? Para Docker
?   ??? appsettings.Local.json        # ?? Para desarrollo local
??? Bookify.Application/
?   ??? Bookify.Application.csproj
??? Bookify.Domain/
?   ??? Bookify.Domain.csproj
??? Bookify.Infrastructure/
?   ??? Bookify.Infrastructure.csproj
??? .files/
?   ??? bookify-realm-export.json  # ?? Configuración de Keycloak
??? docker-compose.yml             # ?? Configuración principal
??? .dockerignore                  # ?? Optimización de build
??? .env                           # ?? Variables de entorno (opcional)
??? .gitignore                     # ?? Excluir datos sensibles
??? Docker-Guide.md                # ?? Esta documentación
??? docker-control.bat             # ??? Script de automatización Windows
??? docker-control.sh              # ??? Script de automatización Linux/Mac
??? README.md                      # ?? Documentación principal

# Volúmenes gestionados por Docker (NO crear manualmente)
# bookify_postgres_data -> Manejado automáticamente
# bookify_keycloak_data -> Manejado automáticamente
```

### **Configuración de archivos clave:**

#### **.gitignore recomendado:**
```gitignore
# Docker volumes (si usas bind mounts)
postgres_data/
keycloak_data/

# Docker environment
.env.local
.env.production

# Visual Studio
.vs/
bin/
obj/

# Build artifacts
**/wwwroot/dist/
**/publish/

# Logs
logs/
*.log

# Temporary files
**/tmp/
**/temp/
```

#### **.env (opcional):**
```bash
# .env
DOCKER_REGISTRY=
COMPOSE_PROJECT_NAME=bookify
ASPNETCORE_ENVIRONMENT=Development
POSTGRES_PASSWORD=postgres
KEYCLOAK_ADMIN_PASSWORD=admin
SEED_DATA=false
```

---

## ?? Flujo de Trabajo Recomendado

### **Desarrollo Diario:**

#### **Opción 1: Desarrollo Local (Recomendado para coding activo)**
```bash
# 1. Iniciar servicios de infraestructura
docker-compose up -d bookify-db bookify-idp

# 2. Desarrollar con Visual Studio usando perfil "Local Bookify.Api"
# Esto conecta a localhost:5432 y localhost:18080

# 3. Al finalizar
docker-compose down
```

#### **Opción 2: Full Docker (Para testing)**
```bash
# 1. Construir y ejecutar todo
docker-compose up -d --build

# 2. Para cambios de código
docker-compose up -d --build bookify.api

# 3. Ver logs
docker-compose logs -f bookify.api

# 4. Al finalizar
docker-compose down
```

### **Testing/Staging:**
```bash
# 1. Limpiar entorno anterior
docker-compose down -v

# 2. Construir imagen fresca
docker-compose build --no-cache

# 3. Ejecutar todos los servicios
docker-compose up -d

# 4. Verificar estado
docker-compose ps
docker-compose logs

# 5. Ejecutar pruebas
# ...

# 6. Limpiar
docker-compose down -v
```

### **Troubleshooting:**
```bash
# 1. Ver logs de todos los servicios
docker-compose logs -f

# 2. Ver logs de servicio específico
docker-compose logs -f bookify-db

# 3. Reiniciar servicio problemático
docker-compose restart bookify-idp

# 4. Entrar al contenedor para debug
docker exec -it Bookify.Db /bin/bash

# 5. Verificar conectividad
docker exec -it Bookify.Api ping bookify-db
```

### **Producción (Consideraciones):**
```bash
# 1. Usar registry externo
export DOCKER_REGISTRY=myregistry.com/

# 2. Usar archivo de compose para producción
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# 3. Configurar health checks
# 4. Implementar logging centralizado
# 5. Configurar monitoring
```

---

## ?? Recursos Adicionales

### **URLs de Servicios:**
- **API:** http://localhost:5000
- **PostgreSQL:** localhost:5432
- **Keycloak:** http://localhost:18080
- **Keycloak Admin:** http://localhost:18080/admin

### **Credenciales por Defecto:**
- **PostgreSQL:** postgres/postgres
- **Keycloak Admin:** admin/admin

### **Comandos de Conexión:**
```bash
# PostgreSQL desde host
psql -h localhost -p 5432 -U postgres -d bookify

# PostgreSQL desde contenedor
docker exec -it Bookify.Db psql -U postgres -d bookify

# Keycloak Admin CLI
docker exec -it Bookify.Identity /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin
```

### **Referencias útiles:**
- [Docker Compose documentation](https://docs.docker.com/compose/)
- [Dockerfile best practices](https://docs.docker.com/develop/dev-best-practices/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Keycloak Docker documentation](https://www.keycloak.org/server/containers)

---

*Última actualización: [Fecha actual]*
*Versión: 2.0*