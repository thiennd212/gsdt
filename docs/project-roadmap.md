# GSDT Project Roadmap

## Phase Overview — v2.29 BACKLOG BATCH COMPLETE (2026-03-30)

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
| **GSDT-P1-05** | DTC FE Domestic Project | Pending | 0% | — | — |
| **GSDT-P1-06** | DTC FE ODA Project | Pending | 0% | — | — |
| **GSDT-P1-07** | DTC Admin CRUD Catalogs (11 dynamic catalogs UI) | Complete | 100% | Smoke | 2026-04-08 |
| **GSDT-P1-08** | DTC Auth & Roles (ICurrentUser extended, query scoping, role-based authz) | Complete | 100% | — | 2026-04-07 |
| **GSDT-P1-09** | DTC Testing | Pending | 0% | — | — |
| **GSDT-P1-10** | DTC Buffer & Polish | Pending | 0% | — | — |
| **Phase 16** | Microservices Extraction (Strangler, RabbitMQ, YARP) | Pending | 0% | — | — |
| **Phase 17** | V2 Strategic (OPA policy-as-code, Backstage catalog) | Pending | 0% | — | — |

---

## Phase 1: Foundation Hardening (COMPLETE - 2026-03-23)

16 new entities (post-YAGNI), 37 handlers, 10 REST APIs, 3 Hangfire jobs. 250+ unit tests passing.

| Module | Entities | Purpose |
|--------|----------|---------|
| M01 Identity | ExternalIdentity, CredentialPolicy, ExternalMapping | SSO/VNeID |
| M02 MasterData | Dictionary, DictionaryItem | Reference lists |
| M05 Workflow | WorkflowTask, TaskAssignment, EscalationRule | Task mgmt |
| M08 Files | FileVersion, Template, Retention, Lifecycle | Document lifecycle |
| M13 Integration | EventCatalogEntry | Event schema |
| M14 Notifications | AlertRule | Alert management |

---

## Phase 2: Digital Signature + Rule Engine + Collaboration (COMPLETE - 2026-03-24)

3 new modules (M09, M04, M06), 15 new entities, 20+ APIs, 161 unit tests.

| Module | Entities | Features |
|--------|----------|----------|
| M09 Signature | SignatureRequest, SignatureResult, CertificateSnapshot, Signer | PKI/X.509, CRL/OCSP, batch signing |
| M04 Rules | Rule, RuleSet, RuleVersion, DecisionTable, RuleTestCase | Microsoft.RulesEngine, decision tables |
| M06 Collaboration | Conversation, ConversationMember, Message, MessageReadState | SignalR chat, threading, presence |

---

## Phase 3: M03 Views + M10 Search + M11 Dashboard (COMPLETE - 2026-03-24)

**Status:** 3 new modules, 6 entities, SQL FTS (default search), dashboard aggregates.

| Module | Entities | Purpose |
|--------|----------|---------|
| M03 Views | ViewDefinition, ViewFilter, ViewColumn | Saved views, column configuration, filter presets |
| M10 Search | SearchIndex | Full-text search (SQL FTS default, Elasticsearch optional) |
| M11 Dashboard | DashboardWidget, DashboardLayout | KPI tiles, configurable widget grid |

---

## Phase 4: M16 AI Upgrade + M15 Governance + M17 Extensions (COMPLETE - 2026-03-24)

**Status:** 3 modules, 8 entities, Azure OpenAI integration, PII detection, SharedKernel extension framework.

| Module | Entities | Key Features |
|--------|----------|-------------|
| M16 AI | AiConversation, AiUsageLog | Azure OpenAI, usage tracking, data sovereignty |
| M15 Governance | GovernancePolicy, PolicyViolation, PiiScanResult, DataRetentionRule | PII detection, policy enforcement, audit hooks |
| M17 Extensions | ExtensionPoint, ExtensionRegistration | SharedKernel plug-in framework for module extension |

---

## Phase 5: Frontend Pages (COMPLETE - 2026-03-24)

**Status:** 8 new FE pages, 3 shared components, 48 smoke tests.

- **New pages (8):** Views manager, Search explorer, Dashboard builder, AI chat, Governance dashboard, Policy violations, PII scan results, Extensions registry
- **Shared components (3):** WidgetGrid (drag-drop), PiiHighlighter, ExtensionSlot
- **Tests:** 48 Vitest smoke tests — each page mounts without runtime errors

---

## Phase 6: Security Hardening (COMPLETE - 2026-03-24)

**Status:** RLS foundation, Semgrep SAST rules, K8s NetworkPolicy, SLO definitions, DataClassification tagging.

- **RLS:** Row-level security tenant filter validated via EF Core global query filters
- **Semgrep:** 12 custom rules covering SQL injection, PII logging, hardcoded secrets, insecure deserialization
- **NetworkPolicy:** Kubernetes manifests — ingress/egress rules per service (API, AuthServer, Workers)
- **SLO definitions:** Availability 99.5%, p95 latency <500ms, error rate <0.1% — Prometheus alert rules
- **DataClassification:** Entity-level classification tags (Public, Internal, Confidential, Restricted) for audit and governance

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

**Status:** 48 compliance tests passing. 915+ total tests (533 unit + 164 FE vitest + 53+ E2E). 

**Backend Integration (32 tests):** Files, Notifications, Organization, Cache, Messaging, BackgroundJobs. **Frontend Unit (168 tests):** Organization, Reports, Delegations, API Keys, System Params, MFA/Profile, Webhooks, Workflow, Smoke tests. **Module Integration (33 tests):** WebAppFixture + SqlServerFixture with Testcontainers. **Architecture (16 tests):** NetArchTest.Rules Clean Architecture enforcement.

**Contract & Regression (32 tests):** Smoke suite (10), Regression suite (10), API contract (24), Event contract (8). 100% pass rate.

**Security Tests (69 tests):** File upload (16), SSRF (12), Headers (15), Error leakage (10), PII masking (8), RBAC (8). 100% pass rate.

**k6 Performance (8 scripts):** smoke-test, cases-load, forms-load, workflow-load, organization-load, reporting-load, search-load, db-performance. Thresholds: p95<500ms (read), <1000ms (write), <0.1% error.

---

## P1: Production Readiness (COMPLETE)

**Status:** COMPLETE (2026-03-17). Production deployment ready.

### What Was Delivered

**CI/CD Pipeline (GitHub Actions):**
- `.github/workflows/ci.yml` — multi-stage pipeline: build → test → Docker build → push to registry
- All stages passing green (0 failures)
- Tests: 224 tests (104 unit + 18 contract + 102 integration) all pass
- SAST/SCA configured (SonarQube + Trivy integration ready)

**Secrets Management (HashiCorp Vault):**
- VaultSharp configuration provider integrated
- `appsettings.Production.json` — sanitized, no plaintext secrets
- Vault Kubernetes auth role configured for prod deployment
- Secret rotation policies: database credentials 24h, API keys 7d

**Docker & Kubernetes:**
- **Dockerfile:** Multi-stage build optimized for size/speed
  - Stage 1: `dotnet/sdk:10.0` — restore, publish
  - Stage 2: `dotnet/aspnet:10.0` — minimal runtime
  - Non-root user (uid 1001) for security hardening
  - HEALTHCHECK: `GET /health/ready` (30s interval, 3 retries)
  - OCI labels: org.opencontainers.image.title, version, created
  - Full restore cache: RUN dotnet restore → COPY [csproj] → RUN dotnet publish
- **Docker Compose (hardened):**
  - SQL Server 2022: HEALTHCHECK via TSQL
  - All services: `restart: on-failure`
  - Environment variables via .env (not hardcoded)
  - Volumes: named (persistence), read-only where possible
- **Helm Charts:** `helm/gsdt-api/` — production-grade K8s deployment
  - StatefulSet for API (replicas, resource limits, affinity rules)
  - ConfigMap for non-secret config
  - Vault agent sidecar for secret injection
  - Liveness/readiness/startup probes configured
  - Blue-green deployment strategy documented

**NuGet Package Management:**
- Removed `D:\NuGetPackages` from NuGet.config (broke Linux CI)
- Package restore from nuget.org + internal Nexus feed
- CI/CD now Linux-compatible (Ubuntu runner)

**Distributed Cache:**
- `IDistributedCache` registered: `AddDistributedMemoryCache()` (dev) / Redis (prod)
- Cache invalidation patterns implemented in all query handlers
- ABAC cache key prefixed with TenantId (multi-tenancy safe)

**Security Audit Results:**
- 35/35 findings identified, 31/35 remediated (88.6%)
- Groups A-D complete (critical + high issues all fixed)
- Groups E-F pending Q2 2026 (Reporting + Files modules)
- All remediations validated via unit tests

### Test Breakdown
- **104 unit tests:** Auth flows, consent, RTBF anonymization, ABAC rules, form field types, notification templates, reporting KPI
- **18 contract tests:** API response envelopes, domain event schemas, error code contracts
- **102 integration tests:** WebAppFixture + SqlServerFixture (Testcontainers), real database, module interactions
- **Total:** 224 tests, all passing (committed to repo)

### k6 Performance Testing
- Tier 500 load: 131K cases created, 0.03% error rate
- Latency breached on local Docker (expected) — production Kubernetes will meet SLOs
- Baseline profiles: load-test (steady-state 50 VUs), spike-test (100→500 VUs), soak-test (10 VUs, 70 min)
- Module-specific tests: identity, cases, files, notifications, audit

---

## Phase 10.1: Security Audit & Remediation (COMPLETE)

**Status:** 31/35 findings remediated (88.6%). Groups A-D complete, Groups E-F pending Q2 2026.

### Findings Summary

| Group | Module | Finding | Severity | Status |
|-------|--------|---------|----------|--------|
| A | Identity | Token revocation on password reset | CRITICAL | ✓ Fixed |
| A | Identity | MFA rate limiting (brute force) | CRITICAL | ✓ Fixed |
| B | ABAC | Role delegation clearance cap | HIGH | ✓ Fixed |
| B | Auth Cache | Clearance level cache invalidation | HIGH | ✓ Fixed |
| C | ExternalRef | DNS rebinding (SSRF) | HIGH | ✓ Fixed |
| C | Test Infra | TestAuthHandler auth bypass | CRITICAL | ✓ Fixed |
| D | CI/CD | Env var config injection | CRITICAL | ✓ Fixed |
| D | Audit | Migration schema validation | HIGH | ✓ Fixed |
| D | MFA | User seeding defaults | MEDIUM | ✓ Fixed |
| D | Cache | ABAC cache invalidator | MEDIUM | ✓ Fixed |
| E | Reporting | TBD (Q2 2026) | 5 findings | Pending |
| F | Files | ClamAV defaults (virus scan) | 4 findings | Pending |

### Remediation Details
- **Token Revocation:** RevokeTokenOnPasswordChange handler, integration tests in Identity module
- **MFA Rate Limiting:** Redis-backed attempt counter, 5-min lockout, monitoring
- **ABAC Clearance Cap:** Query filter prevents >= clearance delegation, unit tests
- **Cache Invalidation:** TenantAware cache key prefix, explicit invalidation on ABAC changes
- **SSRF Protection:** ExternalRef URL whitelist, no DNS rebinding, HTTPS only
- **Test Isolation:** TestAuthHandler scoped to integration tests only, no production impact
- **Env Vars:** DatabaseFixture accepts overrides, CI/CD passes MSSQL_SA_PASSWORD safely
- **Audit Safety:** Migration scripts validate schema version before applying changes
- **MFA Seeding:** Default backup codes generated, stored in vault (not hardcoded)
- **Cache Invalidator:** IAbacCacheInvalidator service on authorization changes

---

## Phase 10.2: RTBF Implementation - F-04 (COMPLETE)

**Status:** Live. All test green. Law 91/2025 Article 9 compliance achieved.

### What Was Built

**ProcessRtbfRequestCommand:**
- Async request processing for right-to-be-forgotten (deletion request)
- Anonymization instead of hard deletion (compliance requirement)
- Audit log of anonymization action (who, when, reason)

**PiiAnonymizer Implementations:**
- `IUserPiiAnonymizer` (Identity): Clear name, email, phone
- `ICasePiiAnonymizer` (Cases): Clear case description, sensitive fields
- `IFormPiiAnonymizer` (Forms): Clear user-provided form data
- `IDocumentPiiAnonymizer` (Files): Clear file metadata, mark as anonymized
- Each anonymizer follows pattern: find entity by ID, clear PII fields, audit log

**Unit Tests (5dadc38):**
- PiiAnonymizer tests: verify fields cleared, audit record created
- ProcessRtbfRequestCommandHandler tests: orchestrate anonymizers, check idempotency
- Domain tests: RTBF aggregate, event emission

**Security Guarantees:**
- Anonymization is irreversible (data cannot be restored)
- Audit trail preserved (who/when, but not what for)
- Soft-delete flag on entity (never hard-deleted from DB)
- HMAC-chained audit log prevents tampering with anonymization records

---

## Phase 08: Forms Module (COMPLETE)

**Status:** Feature complete, 76/76 domain tests passing, build: 0 errors, 0 warnings.

### Completion Summary

Fully redesigned forms module with relational field definitions and dual-mode storage pattern.

**What was built:**
- **FormTemplate aggregate** — tracks template metadata, published status, field list
- **FormField aggregate** — encapsulates field definition with bilingual labels, validation rules, optional flags
- **FormSubmission aggregate** — captures submitted data in StorageMode.Json or StorageMode.Table
- **FormFields table** — relational schema replacing SchemaJson blob
- **Dual-mode storage:**
  - StorageMode.Json: SubmissionData column (JSONB), single-table model
  - StorageMode.Table: dynamic DDL table `forms.Submissions_{templateId:N}` with `forms.v_{code}` view
- **Complex field types:**
  - TableField: nested rows (JSON array in Json mode; child DDL table `forms.SubmTableRows_{tid}_{fid}` in Table mode)
  - AddressField: 3-column expansion (Street, District, Ward)
  - DateRange: 2-column expansion (Start, End)
  - Section, Label, Divider: UI-only, skipped in validation/storage
- **Formula fields:** NCalc expression evaluation (computed at read time, never persisted)
- **Reference fields:**
  - EnumRef: C# enum reflection with namespace whitelist
  - InternalRef: MediatR query handlers for cross-module lookups
  - ExternalRef: HTTP calls + distributed cache (SSRF protection via URL validation)
- **FormMaterializationService:** BackgroundService with Channel<Guid> queue for async DDL creation at publish time
- **Security hardening:**
  - SQL injection mitigation: GUID + regex validation on dynamic column names
  - SSRF protection: ExternalRef URL validation against whitelist
  - Enum namespace whitelist to prevent arbitrary enum reflection

**Architecture:**
- Domain layer: Aggregates, enums (StorageMode, FormFieldType, FormTemplateStatus), value objects
- Application layer: CQRS commands (PublishTemplateCommand, CreateSubmissionCommand), queries (GetTemplateQuery, ListSubmissionsQuery)
- Infrastructure layer: FormTableMigrator, data source resolvers, FormulaEvaluationService, TableFieldDataRepository, FormMaterializationService
- Repository pattern enforced: IFormTemplateRepository, ITableFieldDataRepository (interfaces in Domain, implementations in Infrastructure)

**Dependencies:**
- NCalcSync 5.12.0 (formula evaluation, no runtime eval)
- Existing: MediatR, FluentValidation, EF Core, Dapper

**Testing:**
- 76 domain unit tests covering all field types, storage modes, reference resolution, formula evaluation
- All tests passing, no build warnings

**Known Limitations (By Design):**
- Formula fields are computed at read time only (no stored computed columns to maintain flexibility)
- ExternalRef relies on distributed cache for performance (network calls only on cache miss)
- TableField child tables only in Table mode (Json mode uses JSON arrays for simplicity)

---

## Proposal v2.1 Compliance Assessment

**Status:** V1 compliance at 85%. V1.5 and V2 strategic planning complete.

### Assessment Results
- **V1.0 Alignment:** 85% — Core modular monolith, CQRS, DDD, multi-tenancy, RBAC+ABAC complete
- **Gaps:** UI Template (Phase 2), Microservices Extraction (Phase 2), some reporting/files hardening
- **V1.5 Plan:** 7 phases planned for immediate follow-up (Q2 2026)
  - Link: `./plans/v1.5-roadmap/` (when created)
- **V2 Plan:** 2 strategic phases for major evolution (Phase 2 scope)
  - Link: `./plans/v2-roadmap/` (when created)

### V1 Requirements Met
| Requirement | Status | Evidence |
|---|---|---|
| Modular Monolith architecture | ✓ Complete | 11 modules (Identity, Audit, Forms, AI, etc.) |
| CQRS (read/write separation) | ✓ Complete | EF Core commands, Dapper queries, tests passing |
| Clean Architecture boundaries | ✓ Complete | NetArchTest.Rules enforce 16 tests passing |
| Multi-tenancy isolation | ✓ Complete | TenantId global filter, cache key prefix isolation |
| RBAC+ABAC authorization | ✓ Complete | OpenIddict + ABAC rules with delegation chains |
| PII anonymization (RTBF) | ✓ Complete | F-04 live, 5 module PiiAnonymizers |
| Audit logging (HMAC chain) | ✓ Complete | NĐ53 append-only, 399 tests passing |
| Security hardening | ✓ Complete | Vault integration, Docker hardened, 31/35 findings fixed |
| CI/CD pipeline | ✓ Complete | GitHub Actions multi-stage, 399 tests, SonarQube gate |
| Performance baseline | ✓ Complete | k6: 131K cases, 0.03% error rate, latency profiled |
| Testing infrastructure | ✓ Complete | 399 tests (269 unit + 33 module integration + 96 legacy + 16 arch) |

### V1.5 Roadmap (Q2 2026)
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

## GSDT Phase 1: Investment Projects Module (COMPLETE - 2026-04-07)

**Status:** Phases 01-04 + 08 complete. CQRS, authorization, and infrastructure ready for FE + testing phases.

### Phase 01: Clone & Setup (COMPLETE)
- AqtCoreFW cleanup, GlobalUsings added, Helm chart removed
- Zero build errors, solution compiles cleanly
- Branch: `feature/gsdt-phase1` tracking

### Phase 02: MasterData Catalogs (COMPLETE)
- **14 seed catalogs** (HasData): CapitalDecisionTypes, InvestmentDecisionTypes, ProgressStatuses, ProjectTypes, SubProjectTypes, etc.
- **10 dynamic catalogs**: Province/District/Ward hierarchies, service banks, procurement conditions
- **ContractorSelectionPlan (KHLCNT v1.1)** entity with seed data
- **3 new controllers** for catalog management

### Phase 03: InvestmentProjects Domain & Infrastructure (COMPLETE)
- **24 entities** across 4 layers:
  - **Base hierarchy:** InvestmentProject (base), DomesticProject, OdaProject (TPT inheritance)
  - **Shared children:** ProjectLocation, BidPackage, BidItem, Contract, ProjectDocument, InspectionRecord, EvaluationRecord, AuditRecord, ViolationRecord, OperationInfo
  - **Domestic-specific:** DomesticInvestmentDecision, DomesticCapitalPlan, DomesticExecutionRecord, DomesticDisbursementRecord
  - **ODA-specific:** OdaInvestmentDecision, OdaCapitalPlan, OdaExecutionRecord, OdaDisbursementRecord, LoanAgreement, ServiceBank, ProcurementCondition
- **6 enums:** CapitalDecisionType, InvestmentDecisionType, ProgressStatus, ProjectType, SubProjectType, TimeUnit
- **EF Core DbContext** with "investment" schema, TPT mapping, automatic migrations
- **TenantId filtering** via IModuleDbContext inheritance

### Phase 04: CQRS Commands & Queries (COMPLETE)
- **19 command handlers:** Create/Update/Delete for domestic + ODA projects, sub-entity operations (Location, Decision, BidPackage, Document)
- **4 Dapper list queries:** DomesticProjects, OdaProjects, ProjectDocuments, ProjectLocations
- **DTOs:** CreateDomesticProjectCommand, UpdateDomesticProjectCommand, etc. with FluentValidation
- **2 REST controllers:** DomesticProjectsController, OdaProjectsController (POST/PUT/DELETE + list endpoints)
- **FluentResults error handling** across all handlers

### Phase 08: Auth & Roles (COMPLETE)
- **ICurrentUser extended:** `ManagingAuthorityId` (for CQCQ users), `ProjectOwnerId` (for CDT users)
- **IProjectQueryScopeService:** Query filtering by role + managing authority/project owner context
- **Role-based authorization:** BTC (admin), CQCQ (managing authority), CDT (project owner) with scoped query isolation
- **GsdtRoleSeeder:** System role initialization for phase-1 authorities

### Architecture Decisions
- **TPT inheritance** for InvestmentProject hierarchy (cleaner queries, separate tables per type)
- **Dapper for list queries** (performance) + EF for CRUD (cleaner code)
- **IProjectQueryScopeService** injected in handlers to filter by authority context
- **Tenant-scoped catalog lookups** via MasterData module (shared across modules)

---

## GSDT Phase 07: Admin CRUD Catalogs UI (COMPLETE - 2026-04-08)

**Status:** React 19 admin interface for 11 dynamic catalogs (10 generic + KHLCNT) — full CRUD with validation.

### Deliverables

**Feature Module: admin-catalogs** (9 files, `web/src/features/admin-catalogs/`)
- `catalog-api.ts` — Axios API client for CRUD operations (create, list, update, delete)
- `catalog-types.ts` — TypeScript interfaces (CatalogItem, CatalogItemUpdate)
- `catalog-config.ts` — Catalog metadata configuration (labels, API endpoints, field validation rules)
- `generic-catalog-list-page.tsx` — Reusable list page with pagination, search, filters, add/edit/delete buttons
- `catalog-form-modal.tsx` — Form modal for create/edit with inline validation
- `khlcnt-catalog-page.tsx` — Specialized KHLCNT form (hierarchical, multi-section layout)
- `khlcnt-form-modal.tsx` — KHLCNT create/edit modal with contract-specific fields
- `catalog-index-page.tsx` — Landing page with catalog selector (grid of 11 tiles)
- `index.ts` — Barrel export

**Routes Added (TanStack Router)**
- `/admin/catalogs` — Catalog index (list all 11 catalogs)
- `/admin/catalogs/:catalogType` — Generic catalog list (CapitalDecisionTypes, InvestmentDecisionTypes, etc.)
- `/admin/catalogs/khlcnt` — KHLCNT contractor selection plan editor

**Sidebar Navigation Updated**
- Menu entry: "Catalogs" under System category (admin-menu-entries.ts)
- Icon: BookOutlined (Ant Design)
- Breadcrumb category: nav.adminSystem

### Technical Highlights

1. **Dynamic Catalog Management:**
   - 11 catalogs: Province, District, Ward, CapitalDecisionTypes, InvestmentDecisionTypes, ProgressStatuses, ProjectTypes, SubProjectTypes, ServiceBanks, ProcurementConditions, KHLCNT
   - Unified API endpoints per catalog
   - Field validation at form level (required, length constraints)

2. **Reusable Components:**
   - `generic-catalog-list-page` — Template for most catalogs (saves 70% boilerplate)
   - Configurable columns via `catalog-config` (label, API key, validators)
   - Add/Edit/Delete modals with consistent UX

3. **KHLCNT Specialization:**
   - Separate form with hierarchical structure (ContractorSelectionPlan → Items → Details)
   - Multi-step form layout with collapsible sections
   - Custom validators for contract-specific rules

4. **Integration:**
   - Authorization enforced at route level (Admin/SystemAdmin only via adminRoute guard)
   - Tenant-scoped queries (ICurrentUser.TenantId)
   - Toast notifications (create/update/delete feedback)
   - Error handling with user-friendly messages

### Testing
- Smoke tests covering all 3 routes (renders without error)
- Component tests for modal validation logic
- Integration tests pending (P1-09)

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
