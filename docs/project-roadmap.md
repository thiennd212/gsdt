# GSDT Project Roadmap

## Phase Overview — GSDT P1 + E2E + P2-01 to P2-06 COMPLETE (2026-04-09)

| Phase | Module | Status | Completion | Tests | Last Updated |
|-------|--------|--------|------------|-------|--------------|
| **F-01** | SharedKernel + Infrastructure | Complete | 100% | — | 2026-03-18 |
| **F-02** | Identity (OIDC, RBAC, ABAC, MFA, Delegation) | Complete | 100% | 33 unit | 2026-03-18 |
| **F-03** | Notifications (Email, SMS, In-app, 11 templates) | Complete | 100% | 21 unit | 2026-03-18 |
| **F-04** | Audit (HMAC-chained, RTBF, NĐ53) | Complete | 100% | 34 unit | 2026-03-18 |
| **F-05** | MasterData (Province/District/Ward, caching) | Complete | 100% | — | 2026-03-18 |
| **F-06** | AI (LLM, Qdrant, embeddings, data sovereignty) | Complete | 100% | — | 2026-03-18 |
| **F-07** | Forms (Dynamic DDL, dual-mode storage, complex fields) | Complete | 100% | 76 unit | 2026-03-18 |
| **F-08** | Workflow (WorkflowEngine<S,A>, state machine, branching) | Complete | 100% | — | 2026-03-18 |
| **F-09** | Cases (DDD example, workflow, export PDF/QR) | Complete | 100% | — | 2026-03-18 |
| **F-10** | Files (MinIO, ClamAV, digital signature, encryption) | Complete | 100% | — | 2026-03-18 |
| **F-11** | Integration (YARP, webhooks, API keys, M2M) | Complete | 100% | — | 2026-03-18 |
| **F-12** | Organization (Hierarchy, departments, classification) | Complete | 100% | — | 2026-03-18 |
| **F-13** | SystemParams (System config, cached lookups) | Complete | 100% | — | 2026-03-18 |
| **F-14** | Reporting (KPI dashboard, Excel/PDF export, query catalog) | Complete | 100% | 16 unit | 2026-03-18 |
| **P1** | Production Readiness (CI/CD, Vault, Docker, k6) | Complete | 100% | 18 contract | 2026-03-18 |
| **P2** | Operational Excellence (Helm, Prometheus, Redis, Serilog) | Complete | 100% | 102 integration | 2026-03-18 |
| **V1.5** | Gap Closure (Feature flags, contracts, AsyncAPI, perf) | Complete | 100% | — | 2026-03-18 |
| **Phase 15-01** | Frontend Bootstrap (Vite, React, TS, Auth, API SDK) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-02** | Admin Pages (Forms, Cases, Reports, Settings) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-03** | API SDK + Data Layer (Axios, Query, pagination) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-04** | Audit Log Viewer (3-tab page, filters, export) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-05** | Admin Pages (Users, System, Organization, API Keys) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-06** | Cases + Workflow (List, detail, actions, inbox) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-07** | Reporting + Dashboard (KPI, charts, execution, download) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-08** | Remaining Features (Forms, Files, Notifications, AI) | Complete | 100% | 13 unit | 2026-03-18 |
| **Phase 15-09** | Code Review + Design System (FE fixes, BE modularization, Institutional Modern) | Complete | 100% | 87 FE vitest | 2026-03-21 |
| **Phase 15-10** | Compliance Gap Fixes (nginx header, consent endpoints, RTBF handler, lockout signal) | Complete | 100% | 48 compliance + 23 E2E browser/WebSocket | 2026-03-21 |
| **Phase 1** | Foundation Hardening (16 entities, 10 APIs, 3 jobs, 37 handlers) | Complete | 100% | 250+ unit tests | 2026-03-23 |
| **Phase 2** | M09 Digital Signature + M04 Rule Engine + M06 Collaboration | Complete | 100% | 161 unit tests | 2026-03-24 |
| **Phase 3** | M03 Views + M10 Search + M11 Dashboard (6 entities, SQL FTS) | Complete | 100% | — | 2026-03-24 |
| **Phase 4** | M16 AI Upgrade + M15 Governance + M17 Extensions (8 entities, Azure OpenAI, PII) | Complete | 100% | — | 2026-03-24 |
| **Phase 5** | Frontend Pages (8 new pages, 3 shared components, 48 smoke tests) | Complete | 100% | 48 smoke | 2026-03-24 |
| **Phase 6** | Security Hardening (RLS, Semgrep, NetworkPolicy, SLO, DataClassification) | Complete | 100% | — | 2026-03-24 |
| **Dynamic Workflow 1-3.5** | Definition CRUD, Condition Evaluation, Notifications, Assignment Config | Complete | 100% | 260 unit tests | 2026-03-27 |
| **Forms Enhancement T1** | Rate Limiting (5/min per IP), PDPL Consent (Article 11), Workflow Integration | Complete | 100% | Unit tested | 2026-03-28 |
| **Backlog Batch** | JSON perf indexes, auto-resolve tests, auto-assignment, font embed, warnings | Complete | 100% | 15 new unit | 2026-03-30 |
| **Forms FE Gap Closure** | Field renderers, admin editors, public form enhancements, submissions, Views module | Complete | 100% | TS 0 errors | 2026-04-01 |
| **GSDT-P1-01** | DTC Clone & Setup (AqtCoreFW cleanup, build fix, GlobalUsings) | Complete | 100% | Build 0 errors | 2026-04-07 |
| **GSDT-P1-02** | DTC MasterData Catalogs (14 seed + 10 dynamic + KHLCNT) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-03** | DTC BE Domain & Infrastructure (24 entities, TPT, DbContext) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-04** | DTC BE CQRS Commands & Queries (23 handlers, 2 controllers, Dapper queries) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-05** | DTC FE Domestic Project (4 pages, 6-tab form, shared components) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-06** | DTC FE ODA Project (4 pages, ODA tabs, reuses shared) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-07** | DTC Admin CRUD Catalogs (11 dynamic catalogs UI) | Complete | 100% | Smoke | 2026-04-08 |
| **GSDT-P1-08** | DTC Auth & Roles (ICurrentUser extended, query scoping, role-based authz) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-09** | DTC Testing (35 domain unit + 5 arch tests) | Complete | 100% | 40 unit+arch | 2026-04-07 |
| **GSDT-P1-10** | DTC Buffer & Polish (loading spinner, empty states, breadcrumbs) | Complete | 100% | — | 2026-04-07 |
| **E2E-PW** | E2E Playwright Tests (8 phases, ~120 browser tests, all GSDT modules) | Complete | 100% | 120 E2E | 2026-04-08 |
| **GSDT-P2-01** | Catalogs & Migration (GovernmentAgency tree, Investor, Province/Ward extended, MasterData merged) | Complete | 100% | — | 2026-04-08 |
| **GSDT-P2-02** | PPP BE Domain (PppProject TPT, 10 sub-entities, CQRS 25+ endpoints, 11 new tables) | Complete | 100% | — | 2026-04-08 |
| **GSDT-P2-03** | PPP FE (22 components, 7-tab form, DesignEstimate popup, contract cascading) | Complete | 100% | — | 2026-04-08 |
| **GSDT-P2-04** | DNNN BE Domain (DnnnProject TPT, 3 sub-entities, CQRS 20 endpoints, RegistrationCertificate shared) | Complete | 100% | — | 2026-04-08 |
| **GSDT-P2-05** | DNNN FE (16 new components, 6-tab form, GCNĐKĐT inline CRUD, KKT/KCN location field) | Complete | 100% | — | 2026-04-08 |
| **GSDT-P2-06** | NĐT + FDI BE/FE (4 entities, 24 API endpoints, 20 FE components, 5-tab form, ~85% code reuse) | Complete | 100% | — | 2026-04-09 |
| **Phase 16** | Microservices Extraction (Strangler, RabbitMQ, YARP) | Pending | 0% | — | — |
| **Phase 17** | V2 Strategic (OPA policy-as-code, Backstage catalog) | Pending | 0% | — | — |

---

---

## Foundation Phases 1-6: Hardening, Extensions, Advanced Features (ALL COMPLETE)

Consolidated summary of early foundation work. Details: `./phases/phase-1-details.md`, `./phases/phase-2-details.md`.

| Phase | Focus | Status | Tests |
|-------|-------|--------|-------|
| **Phase 1** | 16 core entities, 37 handlers, 10 APIs, 3 Hangfire jobs (SSO, Reference lists, Task mgmt, Document lifecycle) | ✓ 2026-03-23 | 250+ unit |
| **Phase 2** | 3 new modules (M09 Signature PKI, M04 RulesEngine, M06 SignalR chat) | ✓ 2026-03-24 | 161 unit |
| **Phase 3** | 3 modules (M03 Views, M10 SQL FTS Search, M11 Dashboard KPIs) | ✓ 2026-03-24 | — |
| **Phase 4** | M16 Azure OpenAI + Copilot, M15 PII Governance, M17 Extension Framework | ✓ 2026-03-24 | — |
| **Phase 5** | 8 FE pages (Views, Search, Dashboard, AI chat, Governance, Violations, Extensions), 3 shared components | ✓ 2026-03-24 | 48 vitest |
| **Phase 6** | RLS enforced, 12 Semgrep rules, K8s NetworkPolicy, SLO definitions, DataClassification tags | ✓ 2026-03-24 | — |

---

## Dynamic Workflow Phases 1-3.5 (COMPLETE - 2026-03-27)

**Status:** 4 commits, 82 files modified, +7,128 LOC, 260 unit tests passing. Production-ready workflow CRUD, condition evaluation engine, notification integration, tenant-workflow assignment config.

**Phase 1 — Definition CRUD + Versioning (a27cf88b)**
- WorkflowDefinition: DefinitionKey, Version, IsLatest tracking for multi-version management
- Commands: UpdateWorkflowDefinition, DeleteWorkflowDefinition, CloneWorkflowDefinition
- API endpoints: PUT/DELETE /definitions/{id}, POST /definitions/{id}/clone
- EF migration with backfill for versioning columns

**Phase 2 — Condition Evaluation Engine (69784556)**
- DeclarativeConditionEvaluator service with 11 operators: equals, notEquals, greaterThan, lessThan, gte, lte, in, notIn, contains, isNull, isNotNull
- WorkflowCondition value object for type-safe conditions
- Metadata dictionary in ExecuteTransitionCommand for dynamic condition values
- GOV_WFL_012 error code for ConditionNotMet scenarios

**Phase 3 — Notification Integration (113aaf5a)**
- WorkflowNotificationConfig entity: per-definition, per-action notification rules
- WorkflowTransitionedNotificationHandler: MediatR integration with INotificationModuleClient
- CRUD API endpoints: GET/POST /definitions/{id}/notification-configs
- Full audit trail for notification routing

**Phase 3.5 — Tenant-Workflow Assignment Config (989e8613)**
- WorkflowAssignmentRule entity with 4-level specificity: SystemDefault < Tenant < TenantAndEntity < TenantEntitySubType
- WorkflowAssignmentResolver with ICacheService (24h TTL) for performance
- Auto-resolve in CreateWorkflowInstance when DefinitionId is null
- CRUD API: GET/POST/DELETE /workflow/assignments + GET /resolve for assignment lookup

**Tests:** 260 unit tests covering versioning, condition evaluation, notification routing, assignment resolution, edge cases.

**Next Phase:** Frontend Definition UI + React Flow Designer integration.

---

## Phase 15-10: Compliance Gap Fixes (COMPLETE - 2026-03-21)

48 compliance tests + 915+ total tests (533 unit + 164 FE vitest + 53+ E2E). Backend integration (32), FE unit (168), module integration (33), architecture (16), contract+regression (32), security tests (69: file upload/SSRF/headers/error leakage/PII/RBAC). k6 performance baseline (8 scripts, thresholds: p95<500ms read/<1000ms write/<0.1% error).

---

## P1: Production Readiness (COMPLETE - 2026-03-17)

**CI/CD:** GitHub Actions multi-stage (build→test→Docker→push), 224 tests passing (104 unit + 18 contract + 102 integration), SonarQube + Trivy ready.

**Secrets:** Vault + VaultSharp integration, no plaintext secrets in `appsettings.Production.json`, secret rotation (DB: 24h, API keys: 7d), K8s auth role configured.

**Infrastructure:** Multi-stage Dockerfile (dotnet:10 SDK→runtime), non-root uid 1001, HEALTHCHECK probe, OCI labels. Docker Compose hardened (restart on-failure, named volumes, env-based config). Helm charts (StatefulSet, HPA, Vault sidecar, probes, blue-green strategy).

**Cache & Performance:** IDistributedCache (memory dev/Redis prod), multi-tenancy-safe ABAC cache keys, 131K cases load test (0.03% error), baseline profiles (load/spike/soak, module-specific).

---

## Phase 10: Security Audit & RTBF (COMPLETE)

**10.1 Audit & Remediation:** 31/35 findings fixed (88.6%) — Token revocation, MFA rate limiting, ABAC clearance cap, cache invalidation, SSRF protection, test isolation, env var safety, audit schema validation, MFA seeding, cache invalidation. Groups A-D complete; E-F pending Q2.

**10.2 RTBF (Law 91/2025 Article 9):** ProcessRtbfRequestCommand + 4 PiiAnonymizer implementations (User/Case/Form/Document). Anonymization irreversible, audit trail preserved, soft-delete flag, HMAC-chained log tampering prevention.

---

## Phase 08: Forms Module (COMPLETE)

FormTemplate/FormField/FormSubmission aggregates, 32 field types, dual-mode storage (Json/Table), 76 domain tests passing.
- Supports complex fields: TableField (nested rows), AddressField (3-col expansion), DateRange (2-col)
- Formula evaluation (NCalc), reference resolution (EnumRef/InternalRef/ExternalRef with SSRF protection)
- FormMaterializationService for async DDL creation, IFormTemplateRepository interface-driven design

---

## V1 Compliance Summary

All core v1 requirements met: Modular Monolith (11 modules), CQRS (EF+Dapper), Clean Architecture (NetArchTest 16 tests), Multi-tenancy (TenantId filter), RBAC+ABAC (OpenIddict + delegation), RTBF (5 PiiAnonymizers), Audit (NĐ53 append-only), Security (Vault + 31/35 findings fixed), CI/CD (GitHub Actions + SonarQube), Performance (k6: 131K cases, 0.03% error).

---
1. Email password reset + F-25 verification email
2. Consent audit trail improvements (Law 91 Article 11)
3. Report module hardening (Group E findings)
4. Files module virus scanning defaults (Group F findings)
5. Performance optimization (caching, indexing)
6. Admin dashboard skeleton
7. DevOps automation (Helm, GitHub Actions enhancements)

### V2 Roadmap (Phase 2)
1. **Microservices Extraction:** Strangler pattern, RabbitMQ, YARP API gateway
2. **UI Template:** React/Angular admin + citizen portals

---

## Phase 11.1: V1.5 Gap Closure (COMPLETE)

**Status:** COMPLETE (2026-03-17). All deliverables shipped and tested.

### What Was Delivered

**1. CI/CD Security Enhancements:**
- SAST/SCA job in `.github/workflows/ci.yml` — SonarQube integration configured
- Supply chain security: SBOM generation (CycloneDX format), cosign artifact signing
- All stages passing in GitHub Actions pipeline

**2. Feature Flag Service (IFeatureFlagService):**
- Extracted to `SharedKernel.Contracts` — independent of module dependencies
- Wired into:
  - **Notifications:** SMS gate — disabled by flag `notifications.sms.enabled`
  - **Files:** Virus scan gate — disabled by flag `files.clamav.scan.enabled`
  - **Cases:** Full-text search gate — disabled by flag `cases.search.enabled`
- 4 operational flags seeded in database at startup
- Redis-cached evaluation, TTL 5 minutes

**3. Contract Tests:**
- New test project: `tests/contracts/GSDT.Tests.Contracts`
- 13 contract tests validating API response envelopes, domain event schemas, error codes
- All passing in CI/CD

**4. AsyncAPI Documentation:**
- Full schema: `docs/asyncapi.yaml` (AsyncAPI 3.0 format)
- Documents all 18+ domain events across modules (PermitCreated, UserConsented, etc.)
- References: command-to-event mappings, event subscribers, payload schemas
- Searchable via AsyncAPI Studio (online or local)

**5. Module Template (dotnet new):**
- Location: `templates/aqt-module/`
- Scaffold new modules with: `dotnet new aqt-module --name MyModule --output src/modules/mymodule`
- Pre-generates: Domain/Application/Infrastructure/Presentation layers, DI registration, sample entity + command handler, unit test stubs

**6. Performance Optimizations:**
- **Output Caching:** GET endpoints tagged with duration (5 min for MasterData, 10 min for queries)
- **Connection Pool Warming:** Startup check ensures SQL Server connection pool initialized
- **Environment-aware k6 thresholds:** Local thresholds relaxed (500ms p95), prod thresholds strict (200ms p95)

### Test Evidence
- CI/CD: All stages green (security-scan, contract-tests, load-tests)
- Contracts: 13 tests passing
- Feature flags: Verified in Notifications, Files, Cases modules
- AsyncAPI: Valid schema (checked against AsyncAPI 3.0 spec)
- Module template: Successfully creates Permits sample module

### Key Achievements
- Template accelerates module creation (8 boilerplate files → 1 command)
- Feature flags decouple operational toggles from code deployment
- Contract tests catch API breaking changes early
- AsyncAPI provides single source of truth for event contracts
- SBOM + cosign enable supply chain traceability (compliance: NĐ85/TT12 requirement)

---

## F-08: Workflow Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- WorkflowEngine<TState, TAction> state machine in SharedKernel
- Workflow definition schema and executor
- Step routing with async task support
- Signal wait/resume capability
- Parallel/conditional branching support
- Integration with MassTransit for async event publishing

---

## F-09: Cases Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- Case aggregate with workflow state transitions
- Case type enumerations (linked to MasterData)
- Case priority, assignment, and routing
- Document attachment (Files module integration)
- PDF export with QR code generation
- Audit trail per case transition
- CQRS read-side queries

---

## F-10: Files Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- MinIO object storage integration
- ClamAV virus scanning (async background job via Hangfire)
- Digital signature support (NĐ68) with certificate validation
- File encryption for PII (Always Encrypted columns)
- S3-compatible API for file operations
- Feature flag gate for ClamAV scan

---

## F-11: Integration Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- YARP API gateway with routing and composition
- Webhook engine for external HTTP callbacks with retry logic
- Error code catalog (standardized error responses with Vietnamese localization)
- API Key M2M authentication (SHA-256 hashed, Redis cached)

---

## F-12: Organization Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- Organization hierarchy management
- Department/unit CRUD operations
- Classification level assignment per department
- Organizational unit tree traversal and caching
- Integration with RBAC+ABAC authorization

---

## F-13: SystemParams Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- System configuration parameters (key-value store)
- Dynamic lookup with Redis caching (TTL 5 minutes)
- Admin-configurable settings (rate limits, feature flags, timeouts)
- Safe parameter validation and type conversion

---

## F-14: Reporting Module (COMPLETE)

**Status:** COMPLETE (2026-03-18).

**What was built:**
- KPI dashboard endpoint (cached 5 minutes, no query execution)
- Report execution via async Hangfire jobs (returns 202 Accepted + executionId)
- Template-driven export:
  - ClosedXML for Excel (GOV-styled, 50k row limit)
  - QuestPDF for PDF (GOV-branded Vietnamese)
- Query Catalog: saved queries with SQL validation (@TenantId required, SELECT-only)
- Compliance PDF generation with audit evidence (Admin-only endpoint)
- Report polling via ExecutionStatus query (PENDING → COMPLETED/FAILED)
- Report download with file cleanup
- Test coverage: 16 unit tests

---

## Phase 15: Frontend Core Base (PHASES 01-08 COMPLETE 2026-03-18)

**Status:** Phases 01-08 COMPLETE. Production-ready admin portal.

### Phase 15-01: Project Bootstrap (COMPLETE)

**Location:** `C:\GSDT\web\` (same monorepo as backend)

**Stack:**
- Vite 6 (HMR, code splitting, ~248KB gzip initial JS)
- React 19 + TypeScript 5.7
- Ant Design 5.x (GOV theme: navy #1B3A5C, red #C8102E)
- TanStack Router + Query v5 (type-safe routing, server state)
- Zustand (client state), React Hook Form + Zod (forms)
- oidc-client-ts (PKCE OIDC flow)
- Apache ECharts via echarts-for-react (charting)

**Files:** 25 files covering Vite config, GOV theme, ESLint, Dockerfile

**Deliverables:**
- ✓ ESLint flat config + Prettier
- ✓ Vitest setup (13 tests passing)
- ✓ Dockerfile: multi-stage (node build → nginx:alpine)
- ✓ nginx.conf: SPA fallback + gzip + security headers
- ✓ Build: 0 errors, 0 warnings

### Phase 15-02: Auth + App Shell (COMPLETE)

**Files:** 15 files (AuthProvider, RouteGuard, AppLayout, hooks)

**Deliverables:**
- ✓ OIDC PKCE login/callback flow via oidc-client-ts
- ✓ AuthProvider context + useAuth() hook
- ✓ RouteGuard (redirect to login if unauthenticated)
- ✓ AppLayout: collapsible sidebar, topbar with user avatar/logout
- ✓ Delegation banner (acting-as mode indicator)
- ✓ Permission check from JWT claims (tenant_id, roles, dept_code)

### Phase 15-03: API SDK + Data Layer (COMPLETE)

**Files:** 7 files (api-client, axios config, hooks, types)

**Deliverables:**
- ✓ Axios instance with auth token + correlation ID interceptors
- ✓ ApiResponse<T> envelope unwrap (normalizes backend response)
- ✓ Error normalization per RFC 9457 (Vietnamese detail_vi support)
- ✓ TanStack QueryClient with sensible defaults
- ✓ useServerPagination hook (URL-synced, Ant Table compatible)

### Phase 15-04: Pilot — Audit Log Viewer (COMPLETE)

**Files:** 11 files (page, table, drawer, filters, types)

**Deliverables:**
- ✓ 3-tab page: Audit Logs / Login History / Security Incidents
- ✓ Server-side paginated table (backend /audit/logs with filters)
- ✓ Filters: date range, action type, module, user
- ✓ Detail drawer: pretty-printed JSON diff display
- ✓ CSV export: UTF-8 BOM for Vietnamese Excel compatibility
- ✓ Loading states, error boundaries, empty states

### Phase 15-05: Admin Pages (COMPLETE)

**Files:** 34 files, +2,079 LOC (commit `e6f14604`)

**Deliverables:**
- ✓ User Management: list, create/edit modal, role multiselect, MFA status
- ✓ System Parameters: inline-edit table, feature flags toggle, announcements CRUD
- ✓ MasterData: cascading Province → District → Ward selects
- ✓ Organization: Ant Tree + detail panel + staff assignment table
- ✓ API Key Management: masked prefix, one-time plaintext display

### Phase 15-06: Cases + Workflow (COMPLETE)

**Files:** 18 files, +990 LOC (commit `0f046058`)

**Deliverables:**
- ✓ Case list: paginated table with status/priority/type filters + search
- ✓ Case detail: info section, workflow action buttons, comments, timeline
- ✓ Workflow actions: Submit, Assign, Approve, Reject, Close with confirm modals
- ✓ Workflow Inbox: pending tasks table

### Phase 15-07: Reporting + Dashboard (COMPLETE)

**Files:** 20 files, +1,002 LOC (commit `087605f7`)

**Deliverables:**
- ✓ KPI Dashboard: 4 stat cards, 3 ECharts (pie, bar, line charts)
- ✓ Report Definitions: list + create definition
- ✓ Report Execution: run report, poll status, download when Done
- ✓ ECharts integration via echarts-for-react

### Phase 15-08: Remaining Features (COMPLETE)

**Files:** 28 files, +1,238 LOC (commit `252e92e2`)

**Deliverables:**
- ✓ Forms: template list, detail page with field table + submissions
- ✓ Files: list table with Upload.Dragger, download, virus scan status
- ✓ Notifications: SignalR real-time bell with badge, notification list, mark-as-read
- ✓ AI Search: NLQ input + saved query catalog + dynamic results table

### Phase 15-09: Planned — Testing + Polish (Phase 2)

**Estimated scope:**
- E2E tests (Playwright/Cypress)
- Performance optimization
- Accessibility audit (WCAG 2.1 AA)
- Mobile responsive refinement
- Deployment hardening
- Security audit (OWASP Top 10)

---

## Phase 16: Microservices Extraction (PENDING)

Strangler pattern extraction, RabbitMQ async messaging, YARP gateway refactoring.

## Phase 17: V2 Strategic (PENDING)

Policy-as-Code (OPA) + Backstage software catalog.

---

## Release Timeline (Summary)

- **v1.0 - v2.9:** Foundation phases (13 backend modules, React 19 frontend, authorization, security audit)
- **v2.10 (2026-03-21):** Compliance test suite + design system
- **v2.14 Phase 1 (2026-03-23):** Foundation hardening (16 entities, 250+ tests)
- **v2.15 Phase 2 (2026-03-24):** Digital Signature + Rule Engine + Collaboration (15 entities, 161 tests, 77 projects)
- **Phase 16 (Pending):** Microservices extraction (strangler pattern)

---

## Documentation Sync

Each phase completion triggers documentation updates:
- `codebase-summary.md` — add module section with architecture
- `system-architecture.md` — update module map and dependency graph
- `code-standards.md` — document module-specific patterns
- ADRs — record architectural decisions (if non-standard pattern)

---

## GSDT Phase 1-07: Investment Module + Catalogs UI (COMPLETE - 2026-04-08)

**Phase 01-04:** Clone & setup (zero build errors), 14 seed catalogs (CapitalDecisionTypes, etc.), 10 dynamic catalogs (Province/District/Ward). 24 entities (InvestmentProject TPT hierarchy, Domestic/ODA-specific), 6 enums. 19 command handlers, 4 Dapper queries, 2 controllers, FluentResults error handling.

**Phase 08:** ICurrentUser extended (ManagingAuthorityId, ProjectOwnerId), IProjectQueryScopeService for role+authority scoping, RBAC (BTC/CQCQ/CDT), GsdtRoleSeeder initialization.

**Phase 07:** Admin catalogs UI (9-file feature module: catalog-api, catalog-types, catalog-config, generic-catalog-list-page, catalog-form-modal, khlcnt-catalog-page, khlcnt-form-modal, catalog-index-page). 11 catalogs (10 generic + KHLCNT). Routes: /admin/catalogs, /admin/catalogs/:catalogType, /admin/catalogs/khlcnt. Reusable generic-catalog-list-page (70% boilerplate saved), KHLCNT hierarchical form with custom validators, admin/systemadmin auth, tenant-scoped queries.

---

## Compliance & Governance

**Standards tracked per phase:**
- **PDPL (Law 91/2025):** Consent, RTBF, breach notification (Identity, Audit, Files)
- **NĐ53:** Audit logging, backup procedures (Audit, DevSecOps)
- **NĐ59:** VNeID integration, eKYC levels (Identity)
- **NĐ68:** Digital signatures, CRL/OCSP (Files)
- **NĐ85/TT12:** SAST/DAST, pentest documentation (DevSecOps)
- **QĐ742:** Security headers, account management, audit (Identity, Infrastructure)

Last updated: 2026-03-21 (v2.10 COMPLETE with compliance suite — 892 tests, 4 gaps identified for v2.11, production-ready with remediation plan)
