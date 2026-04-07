# GSDT — GOV Framework v2.46

> Production-ready .NET 10 backend + React 19 frontend for Vietnamese Government projects.
> Built by **AEQUITAS** — clone, extend, ship compliant systems. **2,029+ tests passing | 30/30 security findings fixed | Production ready.**

## What Is This?

A **template repository** that provides a complete, production-ready foundation for Vietnamese Government backend projects. Teams clone this repo and extend it — no need to set up architecture, compliance controls, or infrastructure from scratch.

**Compliant with:** Law 91/2025/QH15 + Decree 356/2025 (PDPL, replaces NĐ13), NĐ53 (Cybersecurity), NĐ59 (VNeID), NĐ68 (Digital Signatures), NĐ85/TT12 (ATTT), QĐ742.

## Architecture at a Glance

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        GSDT — Modular Monolith                     │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Presentation Layer (API)                      │   │
│  │  Controllers (v1) │ YARP Gateway │ OpenAPI 3.1.1 │ Health Checks │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌───────────┐ ┌──────────┐ ┌────────────┐ ┌─────────┐ ┌──────────┐   │
│  │ Identity  │ │  Cases   │ │   Files    │ │  Audit  │ │  Notif.  │   │
│  │ Module    │ │  Module  │ │  Module    │ │  Module │ │  Module  │   │
│  │ OpenIddict│ │ DDD+CQRS │ │ MinIO+ClamAV│ │ HMAC log│ │ Email/SMS│   │
│  └───────────┘ └──────────┘ └────────────┘ └─────────┘ └──────────┘   │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    SharedKernel                                  │   │
│  │  IReadDbConnection │ ICacheService │ ISearchService │ IMessageBus│   │
│  │  IBackgroundJobService │ IWebhookService │ WorkflowEngine<S,A>  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Infrastructure                                │   │
│  │  SQL Server 2022 │ Redis │ MinIO │ Hangfire │ MassTransit/MQ     │   │
│  │  HashiCorp Vault │ OpenTelemetry │ Serilog │ YARP                │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

**Phase 2 path:** Strangler pattern → extract modules as microservices. SharedKernel becomes NuGet packages.

## Core Modules (8)

| Module | Files | Key Features | Compliance |
|--------|-------|---|---|
| **Identity** | 282 | OpenIddict OIDC/OAuth2, RBAC+ABAC, MFA, JIT provisioning, VNeID stub, consent | NĐ59, Law 91 Art.11 |
| **Cases** | 150 | DDD golden-path, WorkflowEngine, CQRS, state machine, batch operations | Core use case |
| **Files** | 96 | MinIO, ClamAV, digital signatures, document lifecycle (M08) | NĐ68 |
| **Notifications** | 83 | Email (MailKit), SMS, SignalR hubs, Scriban templates, 403 toast | - |
| **Audit** | 129 | HMAC-chained logs, RTBF workflow, AI governance (M15), compliance policies | Law 91, NĐ53 |
| **Integration** | 76 | Partners, contracts, webhooks, YARP gateway, API keys | - |
| **MasterData** | 105 | Province/District/Ward hierarchies, dictionaries, external mapping | - |
| **Organization** | 55 | OrgUnit tree, staff positions, tenure tracking | - |
| **SystemParams** | 53 | System-wide feature flags, config parameters | - |

**Phase Modules (9 optional):** Forms, Workflow, Reporting, AI, Search, Dashboard, Views, Extensions, Governance

## Tech Stack

- **Runtime:** .NET 10, C# 14, ASP.NET Core 10
- **ORM:** EF Core 10 (writes) + Dapper via `IReadDbConnection` (reads)
- **Database:** SQL Server 2022 primary; `IDbProvider` abstraction for PG/Oracle/MySQL
- **Auth:** OpenIddict 7.x (on-prem OIDC), RBAC+ABAC, API Key scheme
- **Cache:** `ICacheService` → IMemoryCache (dev) / Redis (prod)
- **Async:** MassTransit + RabbitMQ (internal events) + Hangfire (jobs) + Webhook Engine (external)
- **Search:** SQL Server FTS (default) + Elasticsearch/OpenSearch (optional)
- **Observability:** OpenTelemetry 1.15 + Serilog 4.3 + Prometheus + Grafana
- **Security:** HashiCorp Vault (secrets), Always Encrypted (PII), OWASP Headers, SAST/DAST

→ Full tech stack: [`docs/tech-stack.md`](docs/tech-stack.md)

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker + Docker Compose
- Git

### 1. Clone & rename

```bash
git clone https://gitlab.aequitas.vn/templates/gsdt.git MyGovProject
cd MyGovProject
# Replace "GSDT" with your project name (find & replace in solution)
```

### 2. Start dev infrastructure

```bash
docker compose up -d
# Starts: SQL Server 2022, Redis, MinIO, RabbitMQ, Vault, VNeID mock, Seq
```

### 3. Configure secrets

```bash
# Vault is required (production parity) — see docs/deployment-guide.md
vault kv put secret/gsdt \
  ConnectionStrings__Default="Server=localhost;Database=GSDT;..." \
  Redis__ConnectionString="localhost:6379"
```

### 4. Run migrations

```bash
# Apply all module migrations in correct order
./scripts/migrate-all.sh
```

### 5. Run the API

```bash
dotnet run --project src/host/GSDT.Api
# API: https://localhost:5001
# Swagger: https://localhost:5001/swagger
# Health: https://localhost:5001/health/ready
# Hangfire: https://localhost:5001/hangfire
```

## Async Processing — Decision Guide

```
Domain event crosses module boundary?  → MassTransit (IMessageBus)
Notify external HTTP endpoint?         → Webhook Engine (IWebhookService)
Scheduled / long-running / retry job?  → Hangfire (IBackgroundJobService)
Sync response to caller?               → MediatR command handler
```

## Project Structure

```
src/
├── shared/
│   ├── GSDT.SharedKernel/      # Interfaces, base classes, domain primitives
│   └── GSDT.Infrastructure/    # Cross-cutting implementations
├── modules/
│   ├── identity/
│   ├── cases/
│   ├── files/
│   ├── notifications/
│   ├── audit/
│   ├── integration/
│   └── masterdata/
└── host/
    └── GSDT.Api/               # Entry point, DI composition root

tests/
├── unit/
├── integration/
├── architecture/
├── performance/
└── pentest/                         # Pentest evidence template (NĐ85/TT12)

scripts/
├── migrate-all.sh                   # Run all module migrations in order
├── db-backup.sh                     # NĐ53 backup script
└── db-restore-drill.sh              # NĐ53 restore drill

infra/
├── docker-compose.yml               # Dev environment
├── docker-compose.override.yml      # Local overrides
└── helm/                            # Production Kubernetes charts
```

## Adding a New Module

```bash
dotnet new aqt-module --name MyModule --output src/modules/mymodule
```

→ See [`docs/module-creation-guide.md`](docs/module-creation-guide.md) for step-by-step guide.

## Documentation

| Doc | Description |
|-----|-------------|
| [`docs/tech-stack.md`](docs/tech-stack.md) | Full tech stack with versions and rationale |
| [`docs/system-architecture.md`](docs/system-architecture.md) | Architecture deep-dive |
| [`docs/code-standards.md`](docs/code-standards.md) | Coding standards and conventions |
| [`docs/deployment-guide.md`](docs/deployment-guide.md) | Docker Compose + Kubernetes deployment |
| [`docs/project-roadmap.md`](docs/project-roadmap.md) | Development phases and roadmap |

## Compliance Checklist

Before deploying to GOV environment, verify:

- [ ] Law 91/2025/QH15 + Decree 356/2025: PII fields encrypted (Always Encrypted), consent records present, RTBF implemented, breach notification 72h workflow
- [ ] NĐ53: Audit log retention configured (12 months online, 24 months archive), backup drill done
- [ ] NĐ59: VNeID connector implemented (replace stub), eKYC level configured
- [ ] NĐ68: Digital signature service implemented (replace mock), CRL/OCSP enabled
- [ ] NĐ85/TT12: SAST/DAST passed, pentest evidence documented (`tests/pentest/`)
- [ ] QĐ742: Security headers enabled, account management configured, audit trail complete

---

> **AEQUITAS Internal** — Not for public distribution.
