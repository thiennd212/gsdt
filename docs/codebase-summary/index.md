# Codebase Summary Reference

Detailed documentation for GSDT architecture, modules, and infrastructure.

## Overview

GSDT is a production-ready .NET 10 modular monolith + React 19 frontend for Vietnamese government projects.

- **Status:** v2.46 COMPLETE (2026-04-07) — Production Ready
- **Backend:** 8 core modules (Identity, Cases, Files, Notifications, Audit, Integration, MasterData, Organization)
- **Frontend:** React 19 with 90+ routes, 46 feature modules, admin UI redesign, dark mode, i18n (vi/en)
- **Tests:** 2,029+ tests (1,808 BE unit + 491 FE vitest + 140+ Playwright E2E)
- **Security:** 30/30 audit findings fixed (100%), OWASP Top 10 remediated, QĐ742 compliant
- **Compliance:** Law 91/Decree 356 (100%), NĐ53, NĐ85/TT12, QĐ742 (40/40)
- **RLS:** 40 SQL policies + TenantSessionContextInterceptor + Dapper isolation
- **Data Classification:** 22 entity files tagged (Public/Internal/Confidential/Restricted)

---

## Documentation Index

### [Modules & APIs](./modules.md)
Complete reference for all 8 core + 9 phase modules:
- Layer structure (Domain, Application, Infrastructure, Presentation)
- Key features and entity models
- API endpoints and CQRS patterns
- Repository patterns and cross-module communication

### [Infrastructure & Patterns](./infrastructure.md)
Core infrastructure and architectural patterns:
- SharedKernel base classes and contracts
- Infrastructure services (cache, messaging, jobs, secrets)
- Data layer patterns (write side, read side, RLS)
- Design system and UI components

### [Testing Framework](./testing.md)
Comprehensive testing infrastructure:
- Unit test pattern (NSubstitute, FluentAssertions)
- Integration tests (Testcontainers, WebAppFixture)
- Architecture tests (NetArchTest.Rules)
- Test coverage metrics and organization

---

## Quick Reference

| Metric | Value |
|--------|-------|
| Backend Modules | 8 core (Identity, Cases, Files, Notifications, Audit, Integration, MasterData, Organization) |
| API Endpoints | 393+ REST + SignalR hubs |
| Database | SQL Server 2022, 162 entities, EF Core 10, Always Encrypted (PII) |
| Test Count | 2,029+ (1,808 BE unit + 491 FE vitest + 140+ Playwright E2E) |
| Security Audit | 30/30 findings fixed (100%) |
| Compliance | QĐ742 (40/40 ✓), PDPL (100% ✓), NĐ53, NĐ85/TT12 |
| RLS | 40 SQL policies + TenantSessionContextInterceptor + Dapper isolation |
| Data Classification | 22 entity files (Public/Internal/Confidential/Restricted) |

---

## Architecture at a Glance

```
┌─ Presentation Layer ─┐
│ REST Controllers,    │
│ SignalR Hubs        │
├──────────────────────┤
│ Application Layer    │
│ CQRS (MediatR),     │
│ Validators           │
├──────────────────────┤
│ Domain Layer         │
│ Entities, Aggregates,│
│ Domain Events        │
├──────────────────────┤
│ Infrastructure       │
│ EF Core, Dapper,     │
│ MassTransit          │
└──────────────────────┘
     ↑ All depend on ↑
┌─ SharedKernel ──────┐
│ Base classes,        │
│ Contracts, Primitives│
└──────────────────────┘
```

---

## Production Readiness

- [x] CI/CD (GitHub Actions, SonarQube, CycloneDX)
- [x] Docker hardening (multi-stage, non-root user)
- [x] Kubernetes Helm charts (StatefulSet, HPA, probes)
- [x] Vault integration (secrets rotation)
- [x] Distributed cache (Redis)
- [x] Monitoring (Prometheus, Grafana, Serilog+Seq)
- [x] Load testing (k6 smoke/baseline/full)
- [x] Database RLS (SESSION_CONTEXT + SQL policies)
- [x] Data classification (governance framework)
- [x] Security audit (30/30 remediated)
- [x] WCAG 2.2 AA accessibility
- [x] Full i18n (Vietnamese + English)

---

## Tech Stack

**Backend:** .NET 10, EF Core 10, Dapper, MassTransit, FluentValidation, MediatR, OpenIddict

**Frontend:** React 19, TypeScript 5.7, Vite, Ant Design 5.x, TanStack Router/Query

**Infrastructure:** SQL Server 2022, Redis, RabbitMQ, MinIO, Kubernetes, Vault

**Testing:** xUnit, NSubstitute, Testcontainers, Playwright, k6, NetArchTest.Rules
