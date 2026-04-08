# Project Overview & Product Development Requirements (PDR)

## Executive Summary

**GSDT** is a production-ready .NET 10 modular monolith + React 19 frontend designed for rapid deployment of Vietnamese government projects. Built with Clean Architecture, CQRS, DDD patterns. Fully compliant with NĐ85/TT12 (security), Law 91/Decree 356 (privacy), NĐ53 (audit), NĐ68 (signatures), NĐ59 (VNeID stub).

**Status:** v2.46 (2026-04-08) — GSDT P1 + P2-01 to P2-03 Complete + 2,029+ Tests Passing

**Current Completion (2026-04-08):**
- **GSDT Phase 1:** 10 phases complete (DTC setup, MasterData, BE domain, CQRS, FE pages, Auth, Testing, Polish) + E2E Playwright infrastructure (8 phases, 120 tests)
- **GSDT Phase 2:** 3 phases complete (P2-01 Catalogs & Migration, P2-02 PPP BE Domain, P2-03 PPP FE) — adds GovernmentAgency/Investor catalogs, InvestmentProjects module with Domestic/ODA/PPP types
- **9 backend modules** — Identity, Cases, Files, Notifications, Audit, Integration, MasterData, Organization, InvestmentProjects (+ optional phase modules)
- **React 19 frontend** — 90+ routes, 68+ feature modules (46 core + 22 PPP), dark mode, i18n (vi/en), admin UI redesign, DesignEstimate shared popup
- **Backend Tests:** 1,808 unit + 35 test projects (integration, architecture, E2E)
- **Frontend Tests:** 491 vitest + 120 Playwright E2E (28 workflow, 30+ CRUD, 28 infrastructure)
- **Security audit:** 30/30 findings fixed (100%) — All OWASP Top 10 remediated
- **Compliance:** QĐ742 (40/40) ✓ PDPL (100%) ✓ — PII encryption, RTBF, consent, RLS enforced
- **Production ready:** Vault secrets, Docker hardening, Kubernetes Helm charts, health checks, auto-scaling

---

## Functional Requirements

### FR-001: User Identity & Authentication
- Requirement: Support OIDC/OAuth2 authentication via OpenIddict 7.x
- Acceptance Criteria:
  - Users can authenticate via authorization code flow
  - JWT access tokens issued with claims: `sub`, `tenant_id`, `roles`, `dept_code`
  - Token introspection endpoint validates tokens for API calls
  - Refresh token flow supported
- Implementation Status: **COMPLETE** (Identity module, Phase 04)
- Evidence: OpenIddict configured in `GSDT.AuthServer`, integration tests passing

### FR-002: Multi-Factor Authentication
- Requirement: Support TOTP-based MFA per QĐ742 guidelines
- Acceptance Criteria:
  - Users can enable/disable MFA via AuthServer
  - QR code generation for authenticator apps (Google Authenticator, Microsoft Authenticator)
  - Time-based OTP validation on login
  - Backup codes provided on MFA setup
- Implementation Status: **COMPLETE** (IMfaService in Application layer)
- Evidence: `IMfaService` interface in GSDT.Identity.Application, handlers for setup/validation

### FR-003: Role-Based Access Control (RBAC)
- Requirement: Assign roles to users, validate role claims on protected endpoints
- Acceptance Criteria:
  - Roles stored in OpenIddict claims
  - Controllers can authorize by role: `[Authorize(Roles = "Admin,Operator")]`
  - Role hierarchy: Admin ⊇ Supervisor ⊇ Operator ⊇ Viewer
  - API returns 403 Forbidden if role missing
- Implementation Status: **COMPLETE** (OpenIddict claims mapping)
- Evidence: JWT claims include `roles` array, tested in integration tests

### FR-004: Attribute-Based Access Control (ABAC)
- Requirement: Fine-grained authorization based on user attributes (department, classification level)
- Acceptance Criteria:
  - Attribute rules stored in database (IAttributeRuleRepository)
  - Authorization handler evaluates user attributes against resource attributes
  - Deny access if department mismatch or classification level insufficient
  - Audit log records authorization decisions
- Implementation Status: **COMPLETE** (IAttributeRuleRepository, AbacAuthorizationHandler)
- Evidence: Repository interface in Domain, implementation in Infrastructure, handler in Application

### FR-005: Consent Management (GDPR/PDPL)
- Requirement: Track user consent for data processing per Law 91/Decree 356 Article 11
- Acceptance Criteria:
  - Users can grant/withdraw consent for specific data processing purposes
  - Consent records timestamped and immutable (append-only)
  - GrantConsentCommand and WithdrawConsentCommand available
  - Audit trail shows who granted/withdrew when
- Implementation Status: **COMPLETE** (IConsentRepository, grant/withdraw handlers)
- Evidence: Consent aggregate with DomainEvent, handlers depend on repository interface

### FR-006: Role Delegation
- Requirement: Authorized users can delegate roles to other users temporarily
- Acceptance Criteria:
  - Delegation stored with effective date range (from, until)
  - DelegateRoleCommand and RevokeDelegationCommand available
  - Expired delegations automatically excluded from authorization
  - Audit log shows delegation chain
- Implementation Status: **COMPLETE** (IDelegationRepository, delegate/revoke handlers)
- Evidence: Delegation aggregate, handlers in Application layer

### FR-007: VNeID Federation
- Requirement: Integrate with Vietnam's national e-ID (VNeID) per NĐ59
- Acceptance Criteria:
  - VNeID connector stub present (replace with real integration)
  - eKYC level configuration (1 = ID card, 2 = FA, 3 = video)
  - User attributes imported from VNeID: name, DOB, address, ID number
  - Audit log tracks VNeID authentication events
- Implementation Status: **STUB** (GSDT.Identity.Presentation/VNeIdController.cs)
- Evidence: Placeholder implementation present, requires replacement with real VNeID OAuth endpoint

---

## Non-Functional Requirements

### NFR-001: Security
- Requirement: Implement OWASP Top 10 and Vietnamese government standards
- Acceptance Criteria:
  - TLS 1.2+ enforced on all endpoints
  - Security headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options)
  - SQL injection prevention (parameterized queries via EF Core + Dapper)
  - CSRF protection via anti-forgery tokens
  - API rate limiting (10 req/sec per IP by default)
  - Secrets stored in HashiCorp Vault (no plaintext in config)
- Standards Alignment: OWASP, NĐ85/TT12, NĐ53
- Implementation Status: **COMPLETE** (SecurityHeadersMiddleware, JWT validation, rate limiting)
- Evidence: Middleware stack in Program.cs, tested in architecture tests

### NFR-002: Data Privacy
- Requirement: Protect personally identifiable information (PII) per Law 91/Decree 356
- Acceptance Criteria:
  - PII columns encrypted at rest using Always Encrypted (SQL Server)
  - Column-level encryption keys managed via Vault
  - Right-to-be-forgotten (RTBF) implemented (anonymization, not deletion)
  - Consent records immutable and audit-logged
  - Data breach notification workflow (72-hour timeline)
- Implementation Status: **COMPLETE (F-04)** (Always Encrypted columns defined, RTBF anonymization live with full test coverage)
- Evidence: F-04 feature branch merged. PiiAnonymizer implementations (Identity, Cases, etc.). ProcessRtbfRequestCommandHandler in Audit. Unit tests passing. Audit trail immutable via HMAC chaining. Commit 537be3.

### NFR-003: Audit Logging
- Requirement: Comprehensive audit trail per NĐ53
- Acceptance Criteria:
  - All data mutations logged with: actor, timestamp, operation (CRUD), before/after values
  - Audit logs immutable and append-only (HMAC-chained)
  - Retention: 12 months online, 24 months in archive
  - Searchable by actor, entity, timestamp, operation
  - Real-time alerts on sensitive operations (admin login, role grant, etc.)
- Implementation Status: **COMPLETE** (Audit module with HMAC-chained logs)
- Evidence: AuditLog aggregate, MassTransit consumers subscribe to domain events, queryable via IReadDbConnection

### NFR-004: Performance
- Requirement: API response time <500ms (p95) for typical operations
- Acceptance Criteria:
  - Read-side queries optimized (Dapper + SQL indexing)
  - Caching strategy: Redis for hot data (roles, masterdata, search results)
  - No N+1 queries (Dapper, not ORM for reads)
  - Connection pooling configured
  - Query timeouts enforced (30 sec default)
  - k6 load testing for all modules (load, spike, soak profiles)
- Implementation Status: **COMPLETE** (Dapper for reads, Redis ICacheService, EF Core query filters, k6 test suite with module-specific tests)
- Evidence: IReadDbConnection abstraction, ICacheService decorator on query handlers, shared k6 libs with API Key auth, 4 module load tests

### NFR-005: High Availability
- Requirement: API designed for 99.9% uptime (Phase 2 with distributed tracing)
- Acceptance Criteria:
  - Stateless API servers (no session affinity required)
  - Database connection pooling and retry logic
  - Distributed cache (Redis) for session/token data
  - Health checks endpoint (`/health/ready`)
  - Graceful shutdown (drain in-flight requests)
- Implementation Status: **PHASE 2** (Stateless design in place, distributed cache via ICacheService)
- Evidence: Redis configuration, health checks in Program.cs

### NFR-006: Scalability
- Requirement: Support 10x user growth without code changes
- Acceptance Criteria:
  - Horizontal scaling: multiple API instances behind load balancer
  - Async messaging via MassTransit (decouples modules)
  - Background jobs via Hangfire (queued, not blocking)
  - Read/write separation (Dapper reads scale independently)
  - Caching reduces database load
- Implementation Status: **PHASE 2** (Architecture supports it; extraction to microservices in Phase 2)
- Evidence: MassTransit configuration for async events, Hangfire background jobs, read-side Dapper queries

### NFR-007: Testability
- Requirement: >80% code coverage with automated tests
- Acceptance Criteria:
  - Unit tests for business logic (NSubstitute mocks, no database)
  - Integration tests for API contracts (real database via Testcontainers)
  - Architecture tests enforce layer separation (NetArchTest.Rules)
  - Test fixtures reusable (WebAppFixture, SqlServerFixture)
  - CI/CD pipeline runs all tests before merge
- Implementation Status: **COMPLETE** (224 tests passing, 80%+ coverage across 18 projects)
- Evidence: 104 unit tests (Identity 33, Audit 34, Notifications 21, Reporting 16), 18 contract tests (API envelopes, event schemas, error codes), 102 integration tests (module + legacy). TestAuthHandler + SqlServerFixture standardized. CI/CD validated. Latest: reporting module tests (16 tests).

---

## Technical Constraints

### TC-001: .NET 10 Requirement
- Constraint: All code must target .NET 10 (LTS until Nov 2027)
- Rationale: Stability, security updates, compatibility with government IT infrastructure
- Implementation: `net10.0` target framework in all .csproj files

### TC-002: SQL Server 2022 Primary Database
- Constraint: Primary store is SQL Server 2022 Enterprise; PG/Oracle via `IDbProvider` abstraction
- Rationale: Always Encrypted support, government DBMS compliance, licensing
- Implementation: EF Core SqlServer provider, `IDbProvider` adapter pattern for alternatives

### TC-003: OpenIddict (On-Premise OIDC)
- Constraint: Authorization server must be self-hosted (no cloud auth providers)
- Rationale: Compliance with NĐ53 (data residency), NĐ59 (VNeID integration)
- Implementation: `GSDT.AuthServer` project, OpenIddict 7.x, ASP.NET Identity

### TC-004: Vietnamese Language First
- Constraint: All user-facing messages and validation errors in Vietnamese (with English fallback)
- Rationale: Government users, compliance requirement
- Implementation: `WithMessage()` in FluentValidation includes Vietnamese text, API response `detail_vi` field

### TC-005: Module Isolation
- Constraint: Modules communicate **only** via SharedKernel interfaces or domain events (no direct project references)
- Rationale: Enables Phase 2 microservices extraction without code refactoring
- Implementation: Solution structure with separate project per layer per module, MassTransit event bus

### TC-006: Clean Architecture Enforcement
- Constraint: Dependency arrows point inward only (Presentation → Application → Domain ← Infrastructure)
- Rationale: Domain remains pure business logic, testable, independent of frameworks
- Implementation: NetArchTest.Rules in `GSDT.Architecture.Tests`, CI/CD gate on architecture violations

---

## Phase Breakdown & Status (2026-04-07)

| Phase | Name | Status | Key Delivery | Tests |
|-------|------|--------|---|---|
| **Core** | Identity + RBAC/ABAC, SharedKernel, Infrastructure, OpenIddict OIDC | **COMPLETE** | JIT SSO provisioning, 40 RLS policies, Vault secrets | 33+ unit |
| **Audit** | HMAC-chained logs, RTBF erasure, AI governance, compliance policies | **COMPLETE** | PII encryption, Dapper RLS, OutboxInterceptor, BackgroundJob context | 129 files |
| **Cases** | DDD golden-path, WorkflowEngine, CQRS, search | **COMPLETE** | State transitions, batch operations, 30+ CRUD E2E tests | 150+ files |
| **Files** | MinIO storage, ClamAV scanning, digital signatures, lifecycle M08 | **COMPLETE** | Document versioning, signature validation, multi-tenancy | 96 files |
| **Notifications** | Email (MailKit), SMS, SignalR, Scriban templates | **COMPLETE** | Real-time push, 403 toast, session timeout warning, allSettled | 83 files |
| **Integration** | Partners, Contracts, Message Logs, YARP gateway | **COMPLETE** | Webhooks, API keys, external partners, async messaging | 76 files |
| **MasterData** | Province/District/Ward, Dictionaries, ExternalMapping | **COMPLETE** | Hierarchical data, bulk loading, denormalization cache | 105 files |
| **Organization** | OrgUnit hierarchy, Staff positions, tenure | **COMPLETE** | Tree structure, position management, delegation context | 55 files |
| **SystemParams** | Config parameters, feature flags, announcements | **COMPLETE** | Dynamic configuration, admin panel, banner display | 53 files |
| **Frontend** | React 19 SPA, 90+ routes, 46 feature modules, Ant Design GOV | **COMPLETE** | Form Builder UI (32 field types), Workflow designer, Admin redesign | 491 vitest |
| **E2E Testing** | Playwright, WebApplicationFactory, k6 performance | **COMPLETE** | 28 workflow, 30+ CRUD, 28 infrastructure tests, self-cleanup | 140+ E2E |
| **Security Audit** | OWASP Top 10, NĐ85/TT12, pentest evidence | **COMPLETE** | 30/30 findings fixed, SAST/DAST integration, security headers | 100% |
| **Compliance** | QĐ742, PDPL, NĐ53, data classification | **COMPLETE** | Consent management, RTBF workflow, audit retention, RLS | 100% |

---

## Success Metrics (v2.46 - 2026-04-07)

| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| Backend Modules | 8 core | **8/8 COMPLETE** (Identity, Audit, Cases, Files, Notifications, Integration, MasterData, Organization, SystemParams) | ✓ Complete |
| Test Suite | >80% coverage | **2,029 tests:** 1,808 unit + 35 projects + 140+ Playwright E2E | ✓ Complete |
| Backend Unit Tests | Per module | 1,808 unit tests across all 8 modules + optional phases | ✓ Complete |
| Frontend Tests | FE coverage | 491 vitest + 140+ Playwright E2E (28 workflow, 30+ CRUD, 28 infra) | ✓ Complete |
| API Endpoints | REST coverage | 393+ endpoints (identity, audit, cases, files, notifications, etc.) | ✓ Complete |
| Security Audit | OWASP Top 10 | **30/30 findings fixed (100%)** — All groups A-F remediated | ✓ Complete |
| Compliance | QĐ742 + PDPL | QĐ742: 40/40 ✓ | PDPL: 100% (PII, RTBF, consent, RLS) | ✓ Complete |
| RLS Architecture | Tenant isolation | 40 SQL policies + TenantSessionContextInterceptor + Dapper | ✓ Complete |
| Database Entities | ~162 | SQL Server 2022 with Always Encrypted (PII) + 22 classified | ✓ Complete |
| CI/CD Pipeline | GitHub Actions | Build → Test → Docker → Registry → SBOM + SonarQube SAST | ✓ Complete |
| Production Readiness | Vault + K8s | Docker hardening, Helm charts (API + AuthServer), auto-scaling | ✓ Complete |
| Performance | p95 <500ms | k6 load tests: 27 scripts, spike/soak profiles, <500ms baseline | ✓ Complete |

---

## Dependencies & External Integrations

| Dependency | Version | Purpose | Status |
|---|---|---|---|
| OpenIddict | 7.x | OIDC/OAuth2 server | Integrated |
| EF Core | 10.0 | ORM (writes) | Integrated |
| Dapper | 2.x | SQL query mapper (reads) | Integrated |
| MassTransit | 8.x | Event bus, async messaging | Integrated |
| Hangfire | 1.8.x | Background job scheduler | Integrated |
| Redis | 7.x | Distributed cache | Integrated (Docker) |
| SQL Server | 2022 | Primary database | Integrated (Docker) |
| MinIO | Latest | Object storage | Planned (Phase 03) |
| ClamAV | Latest | Virus scanning | Planned (Phase 03) |
| HashiCorp Vault | 1.x | Secrets management | Integrated (Docker) |
| RabbitMQ | 4.x | Message broker | Integrated (Docker) |
| OpenTelemetry | 1.15 | Distributed tracing | Integrated |
| Serilog | 4.3 | Structured logging | Integrated |
| Testcontainers | Latest | Integration test DB isolation | Integrated |

---

## Risk Assessment

| Risk | Severity | Mitigation | Owner |
|---|---|---|---|
| VNeID integration not ready on time | HIGH | Start integration early (Q4 2025), maintain stub for testing | Lead |
| Performance degradation under load | MEDIUM | Load testing Phase 06, optimize queries early | Tech Lead |
| Security compliance gap | HIGH | Monthly compliance audit, pentest Q2 2026 | Security Lead |
| Database migration complexity | MEDIUM | Testcontainers for safe testing, rollback procedures documented | DBA |
| Module interdependency violation | LOW | Architecture tests block builds, code review gates | Tech Lead |

---

## Development Roadmap

### Q4 2025 (Current)
- [x] Phase 01: Foundation complete (Identity, SharedKernel)
- [x] Phase 10: Testing infrastructure complete
- [ ] Phase 02: Cases module 50% (DDD example)
- [ ] Phase 08: Docker Compose dev setup complete

### Q1 2026
- [ ] Phase 02: Cases module complete (CQRS, state machine)
- [x] All 13 backend modules — COMPLETE (2026-03-18)
- [x] 224 tests passing (unit + contract + integration)
- [x] CI/CD, Docker, Helm, k6 — production-ready

### Q2 2026
- [ ] Frontend Core Base V1 (React + TypeScript + Ant Design)
- [ ] Security audit Groups E-F remediation
- [ ] Pilot deployment on target GOV project

### Q3 2026
- [ ] Frontend V1.5 (design system, workflow/audit UI packs)
- [ ] Microservices extraction planning (if scale demands)
- [ ] Performance optimization post-launch
- [ ] Phase 2 architecture decision

---

## Sign-Off & Approvals

| Role | Name | Approval | Date |
|---|---|---|---|
| Project Manager | TBD | Pending | TBD |
| Tech Lead | TBD | Pending | TBD |
| Security Lead | TBD | Pending | TBD |
| Government Stakeholder | TBD | Pending | TBD |

---

## Appendix: Related Documents

- [`docs/system-architecture.md`](./system-architecture.md) — Architecture deep-dive
- [`docs/code-standards.md`](./code-standards.md) — Coding conventions
- [`docs/codebase-summary.md`](./codebase-summary.md) — Module inventory
- [`docs/tech-stack.md`](./tech-stack.md) — Technology decisions
- [`docs/deployment-guide.md`](./deployment-guide.md) — Ops procedures
- [`docs/compliance-evidence-checklist.md`](./compliance-evidence-checklist.md) — Audit checklist
