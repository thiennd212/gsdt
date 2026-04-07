# Developer Onboarding Guide

Welcome to GSDT — a production-ready .NET 10 backend template for Vietnamese Government projects. This guide gets you from zero to a running local environment.

---

## Prerequisites

| Tool | Version | Install |
|---|---|---|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Docker Desktop | 4.x+ | https://www.docker.com/products/docker-desktop |
| Git | 2.x+ | https://git-scm.com |
| IDE | VS Code or JetBrains Rider | — |
| psql (optional) | any | For DB debugging |

**VS Code extensions:** C# Dev Kit, REST Client, Docker.
**Rider plugins:** .NET Core User Secrets, Ideolog.

---

## 1. Clone & Restore

```bash
git clone https://github.com/aequitas/GSDT.git
cd GSDT/src
dotnet restore GSDT.slnx
```

---

## 2. Start Infrastructure (Docker Compose)

All dev infrastructure is declared in `infra/docker-compose.yml`:

```bash
cd infra
docker compose up -d
```

Services started:

| Service | Port | Purpose |
|---|---|---|
| SQL Server 2022 | 1433 | Primary database |
| Redis | 6379 | Cache + SignalR backplane |
| MinIO | 9000 / 9001 | Object storage (files, exports) |
| RabbitMQ | 5672 / 15672 | Async messaging (MassTransit) |
| HashiCorp Vault | 8200 | Secrets management |
| VNeID mock | 8300 | NĐ59 VNeID integration stub |
| Seq | 5341 / 80 | Structured log viewer |

Wait ~30 seconds for SQL Server to be ready before first run.

---

## 3. Configure Secrets (Vault)

Vault is **required** in dev — no plaintext secrets in `appsettings.json`.

```bash
# Initialise Vault with dev secrets (idempotent)
cd infra
./scripts/vault-init-dev.sh
```

This seeds:
- SQL Server connection string
- Redis connection string
- MinIO access/secret key
- RabbitMQ credentials
- JWT signing key

Verify Vault is running: http://localhost:8200 (token: `dev-root-token`)

---

## 4. Apply Database Migrations

```bash
cd src

# Apply all module DbContexts
dotnet ef database update \
  --project modules/identity/GSDT.Identity.Infrastructure \
  --startup-project host/GSDT.Api \
  --context IdentityDbContext

dotnet ef database update \
  --project modules/cases/GSDT.Cases.Infrastructure \
  --startup-project host/GSDT.Api \
  --context CasesDbContext

# Repeat for: Audit, Files, Notifications, Organization, Workflow, Forms, MasterData
```

MasterData seeding (provinces/districts/wards) runs automatically on first startup.

---

## 5. First Run

```bash
# Terminal 1 — Auth Server (OIDC/OAuth2)
cd src/host/GSDT.AuthServer
dotnet run

# Terminal 2 — API
cd src/host/GSDT.Api
dotnet run
```

API: https://localhost:7001
AuthServer: https://localhost:7000
Swagger UI: https://localhost:7001/swagger
Scalar UI: https://localhost:7001/scalar
Hangfire dashboard: https://localhost:7001/hangfire (requires SystemAdmin role)

---

## 6. Environment Configuration Reference

Key `appsettings.Development.json` entries (non-secret):

```json
{
  "Vault": {
    "Enabled": true,
    "Address": "http://localhost:8200",
    "Token": "dev-root-token"
  },
  "MessageBus": {
    "Transport": "RabbitMQ",
    "Host": "localhost"
  },
  "Telemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  },
  "Serilog": {
    "WriteTo": [{ "Name": "Seq", "Args": { "serverUrl": "http://localhost:80" } }]
  }
}
```

Secrets (loaded from Vault at startup, never in files):
- `ConnectionStrings:Default`
- `ConnectionStrings:Redis`
- `Jwt:SigningKey`
- `Minio:AccessKey` / `Minio:SecretKey`

---

## 7. Common Development Tasks

### Add a new module
Follow `docs/module-creation-guide.md` step-by-step.

### Add a migration
```bash
dotnet ef migrations add {MigrationName} \
  --project modules/{module}/GSDT.{Module}.Infrastructure \
  --startup-project host/GSDT.Api \
  --context {Module}DbContext
```

### Run tests
```bash
# Unit tests only (fast — no containers)
dotnet test tests/unit/ --logger trx

# Integration tests (requires Docker — pulls SQL Server image)
dotnet test tests/integration/ --logger trx

# Architecture tests
dotnet test tests/architecture/ --logger trx

# All tests with coverage
dotnet test GSDT.slnx \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage
```

### View logs
Seq UI: http://localhost:80
Filter by correlation ID: `CorrelationId = "abc-123"`

### View background jobs
Hangfire: https://localhost:7001/hangfire
RabbitMQ management: http://localhost:15672 (guest/guest)

---

## 8. Architecture Overview

| Doc | Purpose |
|---|---|
| `docs/system-architecture.md` | Layer model, module map, request flow |
| `docs/code-standards.md` | Naming, CQRS, error handling patterns |
| `docs/module-creation-guide.md` | Step-by-step new module guide |
| `docs/adr/` | Architectural Decision Records |
| `src/docs/api-design-standards.md` | URL conventions, pagination, versioning |
| `src/docs/tech-stack.md` | Full dependency list with versions |

---

## 9. Debugging Tips

**CORS errors:** Add your frontend origin to `GovApiPolicy` in `AddInfrastructure`.

**401 Unauthorized:** Check that the AuthServer is running and the JWT `issuer` in `appsettings` matches.

**EF Core migrations fail:** Ensure SQL Server container is healthy (`docker ps`) and Vault has seeded the connection string.

**MassTransit consumer not firing:** Check RabbitMQ management UI — confirm the exchange and queue exist. Restart the API if they are missing (registered on startup).

**Vault secrets not loading:** Confirm `vault-init-dev.sh` ran successfully. Check `dotnet run` output for Vault connection errors.

**Hangfire jobs not running:** Hangfire requires the `hangfire` SQL Server schema. Re-run the Hangfire migration if it is missing.
