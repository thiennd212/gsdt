# Codebase Summary

GSDT is a production-ready .NET 10 modular monolith for Vietnamese government projects implementing Clean/Onion architecture + CQRS + DDD.

**Status:** v2.46 (2026-04-08) — 8 core modules + React 19 frontend (90+ routes + admin catalogs CRUD) + Form Builder + AI Copilot + Dynamic Workflow + OpenIddict OIDC + Admin UI redesign + Security audited (30/30 fixed) + 2,029+ tests passing + Production ready

## Quick Stats

| Metric | Value | Notes |
|--------|-------|-------|
| **Backend Modules** | 9 core | Identity, Cases, Files, Notifications, Audit, Integration, MasterData, Organization, InvestmentProjects |
| **Additional Modules** | 9 phase modules | Forms, Workflow, Reporting, AI, Search, Dashboard, Views, Extensions, Governance |
| **Source Code LOC** | ~168K | 2,226 source files |
| **Frontend Pages** | 90+ routes | React 19 + Vite + TypeScript 5.7 + Ant Design 5.x + TanStack Router |
| **Frontend Components** | 47 feature modules | Admin pages (including catalogs CRUD), user profiles, dashboards, workflows |
| **Solution Projects** | 80+ | 47 backend + 33 tests + shared |
| **Test Projects** | 35 total | 18 unit + 13 integration + 1 architecture + 1 contract + 1 regression + 1 compliance |
| **Test Count** | 2,029+ passing | 1,808 BE unit + 491 FE vitest + 140+ Playwright E2E |
| **Security Audit** | 30/30 fixed (100%) | All OWASP Top 10 + NĐ85/TT12 findings remediated |
| **Compliance** | QĐ742: 40/40 ✓ | PDPL: 100% ✓ (consent, RTBF, PII, RLS, audit) |
| **API Endpoints** | 393+ REST | SignalR hubs, WebSocket support, async patterns |
| **Database Entities** | 186 | SQL Server 2022 + EF Core 10 + Always Encrypted (PII); 24 InvestmentProjects entities in "investment" schema |
| **RLS Architecture** | 40 SQL policies | TenantSessionContextInterceptor + Dapper isolation |
| **Data Classification** | 22 entity files | Public/Internal/Confidential/Restricted tags |
| **Form Field Types** | 32 types | Text, Email, Phone, Date, Select, MultiSelect, RichText, File, Signature, etc. |
| **Workflow State Machine** | 7 phases | Design, Create, Execute, Monitor, SLA, Recall, Archive |

---

## Architecture Overview

**Full-stack:** .NET 10 backend + React 19 frontend + separate OpenIddict AuthServer

```
Browser → React SPA (Vite, Ant Design) → GSDT.Api (8 core + 5 phase modules)
         ↓ OIDC Auth Code Flow
       AuthServer (OpenIddict 7.4)
```

**Layers per module:**
- Domain (entities, value objects, domain events) → No infrastructure
- Application (CQRS commands/queries, handlers, validators) → Uses FluentValidation + FluentResults
- Infrastructure (EF DbContexts, Dapper, repos, MassTransit) → Persistence + messaging
- Presentation (REST controllers, SignalR hubs) → HTTP layer

---

## Detailed Documentation

See **[Codebase Summary Index](./codebase-summary/index.md)** for complete module reference, testing patterns, and infrastructure architecture:

- **[Modules & APIs](./codebase-summary/modules.md)** — All 8 core modules with entities, APIs, test coverage
- **[Infrastructure & Patterns](./codebase-summary/infrastructure.md)** — SharedKernel, 13-layer middleware, CQRS patterns, RLS, caching
- **[Testing Framework](./codebase-summary/testing.md)** — 35 test projects, unit/integration/E2E patterns

---

## Key Delivery Items (v2.20 — Form Builder + AI Copilot Complete)

**Form Builder UI (100% Ready)**
- @dnd-kit drag-drop canvas interface with field type palette
- 32 field types: Text, Email, Phone, Number, Date, Time, Select, MultiSelect, Checkbox, Radio, Textarea, RichText, File, Signature, Rating, Slider, Toggle, Location, URL, Autocomplete, Matrix, PageBreak, Hidden, Calculation, Likert, Ranking, Password, Color, Barcode, QR, Section, Description
- Conditional fields with visibility rule engine
- Real-time preview, undo/redo support
- Dual storage: relational (PostgreSQL) + document (JSONB)

**Forms Enhancements (Phase 2.20)**
- Conditional Fields: Dynamic show/hide logic, expression evaluation, nested conditions
- File Upload: S3 integration, client-side chunking, progress tracking, file validation
- CSV Export: Data transformation, template mapping, bulk operations
- Multi-step Forms: Wizard UI, step navigation, progress tracking, conditional visibility
- Submission Approval Workflow: 3-state machine (pending→approved/rejected), reviewer assignment
- Template Duplication: Form clone with entity preservation, bulk field copying
- RichText Field: TipTap editor, HTML sanitization, media embeds

**Frontend AI Copilot (100% Ready)**
- Copilot Sidebar: Collapsible Ant Design Drawer, shared CopilotProvider context, useCopilotChat hook
- 375/375 Vitest tests passing + 220+ Form Builder tests
- scrollIntoView polyfill for CI compatibility
- 35+ FE features with ARIA accessibility + dark mode + i18n vi/en

**Admin Frontend Redesign (2026-03-31)**
- Design System: Semantic GOV_COLORS tokens (navy action blue, 4 status colors, distinct dark mode cards)
- New Components: AdminStatCard (metric + trend), AdminPageHeader (title, actions, stats), AdminTableToolbar (search, filters), AdminContentCard (card wrapper)
- Layout: Header 56px, sidebar 260px (desktop) / 80px (tablet) / 280px drawer (mobile), content max-width 1440px
- Navigation: 25 admin pages → 4 categorized groups (Identity & Access, Content & Workflow, System, Integration), 4-level breadcrumb
- Responsive: useLayoutMode hook with matchMedia (zero re-renders), 3-tier breakpoints (desktop/tablet/mobile)
- Dashboard: Health stats, user/session/job counts, quick actions, activity feed
- Border radius: 8px default, 12px large; elevation shadows 3-level system

**Backend AI Services**
- OCR Document Extractor: OllamaDocumentExtractor service, multimodal vision processing
- ReAct Agent: 4 built-in tools, 5-iteration max
- AI Auto-flagging: 5 risk categories, fail-open design
- TenantSessionContextInterceptor in 16 module DbContexts, SESSION_CONTEXT RLS enforcement
- DataClassification attributes on 22 entity files, SQL RLS policies

**JIT SSO Provisioning (v2.45)**
- External OIDC auth schemes configured in AuthServer via appsettings (OpenID Connect, Google, Microsoft, custom)
- JitProvisioningService: auto-create users on first SSO login (email, fullName, TenantId mapping)
- JitProviderConfig entity: per-provider configuration (enable JIT, domain whitelist, require tenant, auto-create email domain)
- Security: email squatting prevention (RT-01), domain whitelist validation (RT-02), tenant requirement enforcement (RT-03), hourly rate limiting (RT-04)
- Admin APIs: 5 endpoints (CRUD JitProviderConfigs) in Identity module + React admin page (/admin/jit-provider-configs)
- ExternalIdentity + ApplicationUser linked on first login, no email-based auto-linking (security by design)

**Testing Expansion**
- 1,808 BE unit tests (8 core + 5 phase modules + shared kernel, includes JIT SSO tests)
- 32 BE integration tests (Testcontainers pattern)
- 220+ Forms module tests (field validators, conditional logic, approval workflow)
- 11 architecture tests covering 6 modules
- 140+ Playwright E2E tests (45 render checks + 30+ CRUD tests across admin/user routes, Page Object Models with Vietnamese label matching, self-cleanup via afterAll)
- 20 k6 performance test scripts

**Compliance Framework**
- RLS + data classification for PII governance
- Audit logging on sensitive data access
- RTBF erasure respecting classification levels
- DataRetentionRule enforcement for compliance

---

## Production Readiness Checklist

- [x] CI/CD pipeline (GitHub Actions, SonarQube SAST, CycloneDX SBOM)
- [x] Docker multi-stage builds with security hardening
- [x] Kubernetes Helm charts (StatefulSet, HPA, probes)
- [x] Vault integration for secrets rotation (24h DB, 7d API keys, 90d JWT)
- [x] Distributed cache (Redis in prod, memory in dev)
- [x] Monitoring (Prometheus metrics, Grafana dashboards, Serilog + Seq)
- [x] Load testing (k6 smoke/baseline/full modes, p95<500ms for read ops)
- [x] Database RLS enforced at SQL layer
- [x] Data classification governance in place
- [x] Security audit complete (30/30 findings fixed)
- [x] WCAG 2.2 AA accessibility
- [x] Mobile-responsive design
- [x] Full i18n (vi, en)

---

## Tech Stack

**Backend:** .NET 10, EF Core 10, Dapper, MassTransit, FluentValidation, FluentResults, MediatR, OpenIddict 7.4, Hangfire

**Frontend:** React 19, TypeScript 5.7, Vite, Ant Design 5.x, TanStack Router/Query, Zustand, oidc-client-ts

**Infrastructure:** SQL Server 2022, Redis, RabbitMQ, MinIO, Docker Compose, Kubernetes + Helm, Vault, Prometheus/Grafana

**Testing:** xUnit, NSubstitute, FluentAssertions, Testcontainers, WebApplicationFactory, Playwright, k6, NetArchTest.Rules

---

## Phase Roadmap

| Phase | Version | Focus | Status |
|-------|---------|-------|--------|
| P1-P6 | v1.0 | Monolith foundation (13 modules, DDD/CQRS) | ✓ Complete |
| P7-P8 | v1.5 | Feature flags, contract tests | ✓ Complete |
| P9-P10 | v2.0 | React 19 frontend (20 pages) | ✓ Complete |
| P11 | v2.1 | Profile page, Playwright E2E, WCAG AA | ✓ Complete |
| P12-P13 | v2.2-v2.4 | Case comments, webhooks, admin pages | ✓ Complete |
| P14 | v2.5-v2.7 | Security fixes (35 findings), test expansion | ✓ Complete |
| P15 | v2.8-v2.9 | FE code review, compliance tests (48) | ✓ Complete |
| P16 | v2.10 | Authorization upgrade (6 phases A-F) | ✓ Complete |
| P17 | v2.11 | Test expansion (482 new unit/integration) | ✓ Complete |
| P18 | v2.12 | Core entities & API expansion (22 entities, 10 APIs) | ✓ Complete |
| P19-P20 | v2.13-v2.15 | Phases 3-6 (Views, Search, Dashboard, AI, Governance) + Tier 2 Stabilization | ✓ Complete |
| P21 | v2.18-v2.20 | M12 Integration, AI Copilot (Sidebar, OCR, ReAct, auto-flagging), Form Builder UI, Forms enhancements | ✓ Complete |
| P22 | v2.21-v2.23 | Dynamic Workflow Phases 4-6: Frontend Definition Management (React Flow visual designer), Parallel Branching (5 entities, AND/OR/N-of-M logic), Frontend Inbox & Monitoring (generic instance inbox, dashboard) | ✓ Complete |
| **P23** | **v2.24** | **Dynamic Workflow Phase 7 Complete: E2E tests (28 Workflow), EF concurrency fixes (SaveGraph raw SQL, history tracking, RowVersion retry), SLA/recall/attachments/auto-transition** | **✓ Complete** |
| **P24** | **v2.40** | **Codebase Audit v1 + v2 Remediation: Dapper RLS, Hangfire queue migration, Auth session UX, SignalR auth, 403 notification, FormSettingsPanel closure, ViewManager scope, AddressField IsPii, debounce filter, EF dual provider fix (1,808 tests), vitest/orval upgrade (28 npm vulns), Audit v2 Phases 1-5 (Dapper RLS via sp_set_session_context, OutboxInterceptor, BackgroundJobTenantContext, Copilot SSE auth, open redirect validation, tenant GUID validation, 26 pages client-side search, AI controller tenant spoof, SSRF validation, CaseRepository filter, bulk import limit, report auth, 401 redirect dedup, SignalR ref-counting, 20 pages allSettled, i18n)** | **✓ Complete** |
| P25 | v3.0+ | Microservices extraction (Phase 17), VNeID/Digital Sig (Phase 18), Advanced observability | → Planned |

---

## Next Steps

1. **Microservices Extraction (Phase 17):** Strangler pattern for Notifications, Audit, Files, Reporting, Organization modules
2. **Real Integrations (Phase 18):** VNeID OAuth2 federation, digital signature service replacement
3. **Distributed Tracing (Phase 19):** OpenTelemetry + Jaeger for production observability

---

## v2.40 Audit Remediation Summary (2026-04-02)

### Audit v1 Fixes
- Dapper tenant isolation (3 queries + 2 controllers)
- Hangfire queue migration + dashboard middleware ordering
- Auth session error UX (onSilentRenewError notification)
- Chat SignalR auth + pagination reset on search/filter
- 403 notification, error boundaries, validateSearch, HangfireAuth fix

### Test Infrastructure & Dependencies
- EF Core dual provider fix (ModuleDbContext.OnConfiguring guard) — 1,736→1,808 tests pass
- vitest 2→4, orval 7→8 — 28 npm vulnerabilities fixed
- FE test coverage expanded to 491 vitest tests

### Medium Edge Cases (M2, M5-M7)
- FormSettingsPanel stale closure, ViewManager scope, AddressField IsPii, debounce filter

### Audit v2 Fixes (Phases 1-5 COMPLETE)
- **Phase 1:** Dapper RLS via sp_set_session_context, OutboxInterceptor event preservation, BackgroundJobTenantContext
- **Phase 2:** Copilot SSE auth token, open redirect validation, tenant GUID validation
- **Phase 3:** 26 pages fetch-all for client-side search
- **Phase 4:** AI controller tenant spoof, SSRF validation, CaseRepository filter, bulk import limit
- **Phase 5:** Report download auth, 401 redirect dedup, SignalR ref-counting, 20 pages Promise.allSettled, i18n

### Post-fix
- Copilot GUID validation, consent allSettled, DapperReadDbConnection connection leak fix

---

## Notes

- Production deployment ready with Vault-managed secrets, K8s StatefulSet, health checks, auto-scaling
- All tests passing (1,808+ BE unit + 491 FE vitest), 0 warnings, 100% security audit remediation
- RLS enforced at SQL layer via SESSION_CONTEXT + Dapper RLS implementation + Vault for RBAC decisions
- Full compliance with Law 91/2025 (PII encryption, RTBF, consent, data classification)
