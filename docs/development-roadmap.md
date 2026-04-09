# GSDT Development Roadmap

## Current Version: v2.46 COMPLETE (2026-04-09)

**Status:** Phase 2 Complete — 6 project types (Domestic, ODA, PPP, DNNN, NĐT, FDI) with 46 InvestmentProject entities, ~160 API endpoints, 28 Phase 2 FE components, 82 new tests (unit + integration), P01-P08 all delivered: Catalogs (2 entities) + PPP BE (12 entities, 25 endpoints) + PPP FE (7-tab form, 22 components) + DNNN BE (3 entities, 20 endpoints) + DNNN FE (6-tab form, 16 components) + NĐT+FDI BE (4 entities, ~40 endpoints) + NĐT+FDI FE (28 components, 5-tab forms) + Testing/Buffer verification.

---

## Previous Version: v2.40 COMPLETE (2026-04-02)

**Status:** Production-ready .NET 10 modular monolith + React 19 full-stack GOV platform + comprehensive test suite (2,300+ tests, 100% passing: 1,808 BE unit + 491 FE vitest) + Dapper RLS enforcement via sp_set_session_context + Audit v1 + v2 (5 phases) security remediation complete + 26 pages client-side search (fetch-all pattern) + Copilot/open-redirect/GUID validation + SignalR auth & ref-counting + Promise.allSettled bulk ops + EF Core dual provider fix (1,808→1,736 test fix) + vitest 2→4, orval 7→8 (28 npm vulns fixed) + full RLS deployment (40 table policies) + M12 Integration module + AI Copilot features (Sidebar, OCR, ReAct, auto-flagging) + Form Builder UI (@dnd-kit, 32 field types) + enhanced Forms module (conditional fields, file upload, CSV export, multi-step, approval workflow, template duplication, RichText) + Dynamic Workflow ALL Phases 1-7 Complete (CRUD, conditions, notifications, tenant assignment, React Flow designer, parallel branching, inbox/monitoring, advanced SLA/recall/attachments/auto-transition) + ~161 entities, 388+ APIs, 40 RLS policies.

---

## Phase Summary (v2.0 → v2.11)

| Phase | Version | Date | Focus | Status |
|-------|---------|------|-------|--------|
| **Phase 1-6** | v1.0 | 2026-01-15 | Modular Monolith foundation, 13 modules, DDD/CQRS/Clean | ✓ Complete |
| **Phase 7-8** | v1.5 | 2026-02-20 | Feature flags, contract tests, module template | ✓ Complete |
| **Phase 9-10** | v2.0 | 2026-02-28 | React 19 frontend (20 pages), TanStack routing/query, i18n | ✓ Complete |
| **Phase 11** | v2.1 | 2026-03-05 | Profile page, password change, E2E Playwright, WCAG 2.2 AA | ✓ Complete |
| **Phase 12** | v2.2-2.4 | 2026-03-08 to 2026-03-12 | Case comments, webhooks, security audit (35/35 findings fixed) | ✓ Complete |
| **Phase 13** | v2.5-2.7 | 2026-03-14 to 2026-03-17 | Admin pages (users, params, organization), Docker full-stack, UI polish | ✓ Complete |
| **Phase 14** | v2.8-2.9 | 2026-03-19 to 2026-03-20 | 5 CRITICAL security fixes, test expansion to 824 tests | ✓ Complete |
| **Phase 15** | v2.10 | 2026-03-21 | Frontend code review, compliance tests (48), design system, Scriban upgrade | ✓ Complete |
| **Phase 16** | v2.11 | 2026-03-22 | Authorization upgrade (6 phases A-F), CRUD gaps, production hardening | ✓ Complete |
| **Phase 17** | v2.12 | 2026-03-23 | Comprehensive test plan Phases 1-3 (482 new tests: 274 BE unit + 32 BE integration + 168 FE unit) | ✓ Complete |
| **Phase 18** | v2.13 | 2026-03-23 | Comprehensive test plan Phase 4 (20 new contract/regression tests, trait-based selective execution) | ✓ Complete |
| **Phase 19** | v2.14 | 2026-03-23 | Core Entities & API Expansion Phase 1 (22 entities, 10 APIs, 3 Hangfire jobs, idempotency middleware) | ✓ Complete |
| **Phase 20** | v2.15 | 2026-03-24 | Tier 2 Stabilization (FE test fixes, TenantSessionContextInterceptor in 16 DbContexts, DataClassification on 19 entities, Architecture tests 11/11, Playwright E2E smoke tests 12/12, k6 perf tests 20 scripts) | ✓ Complete |
| **Phase 21** | v2.16 | 2026-03-25 | Tier 3 Production Readiness (Fix Files integration test duplicate CreateTable, Full RLS deployment 15 migrations 40 tables 15 schemas, Architecture tests expansion 13 modules, Module isolation cross-dep fixes) | ✓ Complete |
| **Phase 22** | v2.17 | 2026-03-25 | M12 Integration Module (Partner/Contract/MessageLog entities, 14 REST APIs, 75 unit tests, 3 React list pages, RLS policies, architecture tests 14/14) | ✓ Complete |
| **Phase 23** | v2.18 | 2026-03-25 | AI Copilot Features (Copilot Sidebar, OCR Document Extractor, ReAct Agent, YARP fix, Workflow TenantId standardization) | ✓ Complete |
| **Phase 24** | v2.20 | 2026-03-26 | Form Builder UI + Forms Enhancements (dnd-kit, 32 field types, conditional fields, file upload, CSV export, multi-step, approval workflow, template duplication, RichText) + Form Builder Tests (18+ files) | ✓ Complete |
| **Phase 25-28** | v2.21-v2.24 | 2026-03-27 | Dynamic Workflow Phases 4-7: Frontend visual designer + React Flow, parallel branching with 5 entities, generic instance inbox & KPI monitoring, advanced SLA/recall/attachments/auto-transition | ✓ Complete |
| **Phase 25** | v2.21 | 2026-03-27 | Dynamic Workflow Phase 4: Frontend Definition Management (React Flow visual designer with 20 FE components, SaveDefinitionGraphCommand, Activate/Deactivate endpoints) | ✓ Complete |
| **Phase 26** | v2.22 | 2026-03-27 | Dynamic Workflow Phase 5: Parallel Branching (5 new entities, ParallelBranchService AND/OR/N-of-M logic, BranchTimeoutCheckerJob, ResolveBranchChildCommand, GetBranchStatusQuery, 6 API endpoints) | ✓ Complete |
| **Phase 27** | v2.23 | 2026-03-27 | Dynamic Workflow Phase 6: Frontend Inbox & Monitoring (generic WorkflowInstance inbox, instance detail page with timeline, KPI dashboard, 7 new React hooks) | ✓ Complete |
| **Phase 28** | v2.24 | 2026-03-27 | Dynamic Workflow Phase 7: Advanced Features (SlaHours per state, IsRecall on transitions, WorkflowTransitionAttachment entity, AutoTransitionOnTimeoutId, SlaBreachCheckerJob per-state logic, IWorkflowResolver bridge) + E2E Playwright tests (28 workflow tests) + EF concurrency fixes (SaveGraph raw SQL, WorkflowInstanceHistory tracking, RowVersion auto-retry) | ✓ Complete |
| **Phase 29** | **v2.40** | **2026-04-02** | **Audit v1 + v2 Security Remediation: Dapper RLS (sp_set_session_context), Hangfire queue migration, Auth session UX, SignalR auth/pagination/ref-counting, 26 pages fetch-all search, Copilot GUID/SSE auth, open redirect validation, tenant GUID validation, AI SSRF/tenant spoof, CaseRepository filter, bulk import limit, report auth, 401 dedup, 20 pages allSettled, i18n, FormSettingsPanel closure, ViewManager scope, AddressField IsPii, EF dual provider fix (1,808 tests), vitest/orval upgrade (28 npm vulns)** | **✓ Complete** |
| **Phase 30** | **v2.41** | **2026-04-03** | **Server-Side Search: 8 new BE query handlers (Audit 5, Forms 2, Files 1) + SearchTerm DTO, Audit CONTAINS/FTS with IFeatureFlagService LIKE fallback, Forms/Files LIKE pattern, FE already migrated useServerPagination + useDebouncedValue (26 pages), 3 small-dataset pages (Signature, Rules, Backup) remain pageSize:9999, Integration/Workflow already supported** | **✓ Complete** |
| **Phase 31** | **v2.45** | **2026-04-05** | **JIT SSO Provisioning: External OIDC schemes (OpenID Connect, Google, Microsoft) in AuthServer, JitProvisioningService auto-creates users on first login, JitProviderConfig CRUD entity + admin APIs + React /admin/jit-provider-configs page, 4-layer security (email squatting RT-01, domain whitelist RT-02, tenant requirement RT-03, hourly rate limit RT-04), 21 unit tests (Create/Update/Delete/Provision/RateLimit/DomainWhitelist), ExternalIdentity+ApplicationUser linking** | **✓ Complete** |
| **Phase 32 (P2-P08)** | **v2.46** | **2026-04-09** | **Phase 2 Complete: Build verification, all 6 project types (Domestic/ODA/PPP/DNNN/NĐT/FDI) with ~160 API endpoints + 28 FE components, docs polish, 82 new tests, 4 NĐT/FDI DB tables pending migration, deferred items noted** | **✓ Complete** |

---

## v2.11 Milestone Completion (2026-03-22)

### Identity Authorization Upgrade (6 Phases) — COMPLETE

**Phase A: Permission Codes & Enums**
- [x] PermissionCode enum (MODULE.RESOURCE.ACTION)
- [x] RoleType enum (Admin, Officer, Citizen, Delegate)
- [x] Group entities (UserGroup, UserGroupMembership, GroupRoleAssignment)

**Phase B: Data Scope Layer**
- [x] 7 DataScopeType values (SELF, ASSIGNED, ORG_UNIT, ORG_TREE, CUSTOM_LIST, ALL, BY_FIELD)
- [x] IDataScopeService + ResolvedDataScope DTO
- [x] Automatic query filtering per data scope

**Phase C: Policy Rules Engine**
- [x] PolicyRule entity with NCalc expression language
- [x] IPolicyRuleEvaluator + IEffectivePermissionService
- [x] PermissionAuthorizationHandler enforcement

**Phase D: Admin APIs (16 endpoints)**
- [x] GroupsAdminController (8: create, list, add/remove users, assign roles)
- [x] DataScopeAdminController (4: assign scope, list by role)
- [x] PolicyRulesAdminController (4: CRUD with expression validation)

**Phase E: User Delegation Upgrade**
- [x] DelegationStatus enum (Pending, Approved, Active, Revoked, Expired)
- [x] IPermissionVersionService (Redis-backed versioning)
- [x] DelegationExpiryJob (Hangfire automated cleanup)

**Phase F: Advanced Authorization**
- [x] SodConflictRule (Segregation of Duties detection)
- [x] AppMenu + MenuRolePermission (sidebar menu authorization)
- [x] IMenuService + MenuController (4 endpoints)

### CRUD Gaps — COMPLETE

- [x] Forms Update endpoint (PATCH /api/v1/form-templates/{id})
- [x] Forms Delete endpoint (DELETE /api/v1/form-templates/{id})
- [x] Field reorder endpoint (PATCH /form-templates/{id}/fields/{fieldId}/order)
- [x] ABAC Rules FE edit modal (clearance + department assignment)

### Compliance Gaps (v2.10) — COMPLETE

- [x] nginx server_tokens off (QĐ742 §4.1)
- [x] Account lockout signal (QĐ742 §5.2)
- [x] Consent endpoints GET/POST (PDPL Art. 11)
- [x] RTBF POST handler (PDPL Art. 17)

### Production Hardening — COMPLETE

- [x] AuthServer Dockerfile rewrite (Alpine, health check, non-root)
- [x] appsettings.Production.json (sanitized, Vault-ready)
- [x] .gitignore updates (local secrets, dev certs)

### Test Coverage (v2.11)

- [x] 533 BE unit tests → 574 BE unit tests (+274 new, Phase 1 complete)
- [x] 164 FE vitest tests → 332 FE vitest tests (+168 new, Phase 3 complete)
- [x] 147 E2E Playwright tests (unchanged, 27 core tests)
- [x] 0 BE integration tests → 32 BE integration tests (Phase 2 complete)
- [x] 48 compliance tests (QĐ742 + PDPL)
- [x] 34 authorization tests (Phases A-F)
- [x] 18 contract tests
- [x] **Total: 1,093 tests — 100% pass rate (Phases 1-3 complete)**

---

## Comprehensive Test Plan (Phases 1-10 Framework)

**Status:** Phases 1-3 complete (2026-03-23), implementing 10-layer test model per Tieu_chuan_Test_He_thong.md.

### Phase 1: Backend Unit Test Gaps — COMPLETE (2026-03-22)
- **Delivered:** 274 new tests across 9 projects (Organization, SharedKernel, MasterData, SystemParams, Files.Domain, Workflow.Domain, Reporting.Domain, Notifications.Domain, Integration.Domain)
- **Coverage:** All 13 modules now have unit tests (0 → complete)
- **Total:** 574 BE unit tests passing

### Phase 2: Backend Integration Test Gaps — COMPLETE (2026-03-23)
- **Delivered:** 32 new integration tests across 6 projects (Files, Notifications, Organization, Cache, Messaging, BackgroundJobs)
- **Infrastructure:** Testcontainers (SQL Server, Redis, RabbitMQ), WebAppFixture, IntegrationTestBase patterns
- **Coverage:** App+DB, App+Redis, App+Queue, App+FileStorage
- **Total:** 32 BE integration tests passing

### Phase 3: Frontend Unit Test Coverage — COMPLETE (2026-03-24)
- **Delivered:** 168 new tests across 43 files (8 test files, 37 smoke tests) + scrollIntoView polyfill fix
- **Coverage:** Organization, Reports, Delegations, API Keys, System Params, MFA, Webhooks, Workflow, Utilities, Layouts, Hooks, Routes, CopilotChat
- **Total:** 375 FE vitest tests passing (all 4 CopilotChatPage failures resolved)

### Phase 4: Contract & Regression Tests — PLANNED
- API envelope validation, domain event schemas, backward compatibility

### Phase 5: Security Tests — PLANNED
- Authorization boundaries, injection prevention, crypto validation

### Phase 6: Performance Tests — PLANNED
- Load testing, stress testing, baseline SLOs

### Phase 7: Reliability & Resilience Tests — PLANNED
- Retry logic, circuit breakers, graceful degradation

### Phase 8: Accessibility & Compatibility Tests — PLANNED
- WCAG 2.2 AA validation, browser compatibility

### Phase 9: Installation & Migration Tests — PLANNED
- Upgrade paths, schema migrations, data seeding

### Phase 10: CI/CD Gates — PLANNED
- Automated quality gates, coverage thresholds, security scanning

---

## Phase 17: Microservices Extraction (PLANNED - Phase 2)

**Goal:** Strangler pattern extraction of modules into separate services.

**Timeline:** Q2 2026 (post v2.11 stabilization)

### Modules Earmarked for Extraction
1. **Notifications Module** — Async, stateless, low coupling
2. **Audit Module** — Event-driven, append-only, standalone
3. **Files Module** — Self-contained (MinIO + ClamAV)
4. **Reporting Module** — Heavy compute, independent jobs
5. **Organization Module** — MasterData-adjacent, low inter-module deps

### Architecture Changes
- HTTP → RPC via YARP gateway
- In-process MassTransit → RabbitMQ transport (config-only)
- SharedKernel contracts remain (service discovery layer)
- Separate Docker images per service
- Kubernetes StatefulSet per module

### Success Criteria
- **Zero breaking changes** to API contracts
- **Sub-second RPC latency** (target: <100ms p99)
- **99.9% availability** (K8s rolling updates, service mesh observability)

---

## Phase 18: VNeID & Digital Signature Integration (PLANNED - Phase 2)

**Goal:** Replace stub connectors with real government integrations.

**Timeline:** Q3 2026 (compliance-driven)

### VNeID Integration (NĐ59)
- [ ] Replace stub with real VNeID OAuth2 federation
- [ ] eKYC level negotiation (1, 2, 3)
- [ ] Attributes import: name, DOB, ID number, address
- [ ] Session binding to NIN
- [ ] Audit trail for VNeID authentication events

### Digital Signature Service (NĐ68)
- [ ] Replace QuestPDF mock with real digital signature API
- [ ] CRL/OCSP validation
- [ ] Timestamp token verification
- [ ] Evidence archive (long-term validation)
- [ ] Signed export formats (PDF-A with embedded signatures)

### Compliance Evidence
- [ ] NĐ59 compliance checklist (eKYC integration)
- [ ] NĐ68 compliance checklist (digital signatures)
- [ ] Pentest evidence in `tests/pentest/`

---

## Phase 19: Advanced Observability (PLANNED - Phase 2)

**Goal:** Distributed tracing + custom metrics for production scale.

**Timeline:** Q3 2026 (operational excellence)

### Distributed Tracing
- [ ] OpenTelemetry 1.15 full integration
- [ ] Jaeger deployment (trace backend)
- [ ] Correlation ID propagation across services (Phase 17)
- [ ] Trace sampling (adaptive, 1% error traces + 0.1% success traces)

### Custom Metrics
- [ ] Prometheus metrics on authorization decisions (grant/deny counts)
- [ ] Data scope filtering metrics (scope type distribution)
- [ ] Policy rule evaluation latency (histogram)
- [ ] Delegation expiry job metrics (processed/failed)

### SLO Monitoring
- [ ] API response time: p99 <200ms
- [ ] Error rate: <1%
- [ ] Availability: 99.9%
- [ ] Database latency: p99 <100ms

---

## Compliance Checklist (v2.11)

| Standard | Requirement | Status |
|----------|-------------|--------|
| **Law 91/2025 + Decree 356** | PII encryption (Always Encrypted), consent, RTBF, breach notification | ✓ Complete |
| **NĐ53** | Audit log retention (12mo online, 24mo archive), backup drill | ✓ Complete |
| **NĐ59** | VNeID stub present (replace in Phase 18) | ⚠ Stub (Phase 18) |
| **NĐ68** | Digital signature stub present (replace in Phase 18) | ⚠ Stub (Phase 18) |
| **NĐ85/TT12** | SAST/DAST/pentest, SBOM, security training | ✓ Complete (GitHub Actions) |
| **QĐ742** | Security headers, account management, audit trail | ✓ Complete |

---

## Known Limitations & Debt

### Phase 2 Blockers
1. **Microservices extraction**: Requires careful strangler implementation (Phase 17)
2. **Real VNeID/Digital Sig**: Blocked on government API access (Phase 18)
3. **Global cache invalidation**: Redis pub/sub strategy needed for Phase 17 scale

### Technical Debt
- [ ] Test fixtures (seeded data) could be more modular
- [ ] NCalc policy expressions need performance tuning for complex rules
- [ ] DataScopeType enum may need domain-specific custom types (Phase 18)

---

## Team Notes

- **Frontend:** React 19, Ant Design 5.x, TanStack ecosystem (routing, query, vitest)
- **Backend:** .NET 10, EF Core 10, Dapper, OpenIddict 7.x, MassTransit
- **Infrastructure:** Docker Compose (dev), Kubernetes Helm (prod), SQL Server 2022, Redis, MinIO, RabbitMQ
- **Compliance:** Full Vietnamese government stack (Law 91, NĐ53, QĐ59, QĐ68, NĐ85/TT12, QĐ742)
- **Testing:** 926 tests, 100% pass rate, all layers covered (unit, integration, E2E, compliance)
- **Production Ready:** ✓ Yes — CI/CD green, security audit complete, load tested, monitoring configured

---

## Dynamic Workflow Feature Complete (v2.24 — All Phases 1-7 Done)

**Milestone:** Dynamic Workflow engine fully implemented with all 7 phases complete as of 2026-03-27.

### Phase Completion Summary
| Phase | Feature | Status | Tests |
|-------|---------|--------|-------|
| **1** | CRUD + Versioning | ✓ Complete | 32 unit tests |
| **2** | Declarative Condition Evaluation (11 operators) | ✓ Complete | 28 unit tests |
| **3** | Notification Integration | ✓ Complete | 18 unit tests |
| **3.5** | Tenant-Workflow Assignment | ✓ Complete | 12 unit tests |
| **4** | React Flow Visual Designer (20 FE components) | ✓ Complete | 24 FE tests |
| **5** | Parallel Branching (AND/OR/N-of-M, 5 entities) | ✓ Complete | 35 unit tests |
| **6** | Generic Inbox & KPI Dashboard | ✓ Complete | 18 FE tests |
| **7** | SLA/Recall/Attachments/Auto-Transition + E2E | ✓ Complete | **28 Playwright E2E tests** |

### Key Deliverables
- **Backend:** 7 workflow modules with 250+ APIs, 40 RLS policies, 3 Hangfire jobs
- **Frontend:** Workflow definition designer, instance inbox, monitoring dashboard, 7 React hooks
- **Testing:** 28 E2E Playwright tests covering all phases, CRUD, branching, transitions, SLA
- **Known Patterns:** SaveGraph raw SQL (bulk upsert), WorkflowInstanceHistory concurrency fix, RowVersion auto-retry

### Readiness for Production
- ✓ All 7 phases complete with E2E test coverage
- ✓ 100% of workflow APIs tested (28 E2E + 150+ unit tests)
- ✓ EF concurrency patterns documented and mitigated
- ✓ Ready for tenant deployment (Phase 2 scaling)

---

## Success Metrics (v2.11 → v3.0)

- **Microservices adoption:** >50% of modules extracted (Phase 17)
- **RPC latency:** p99 <100ms (service-to-service)
- **Availability:** 99.9% uptime (Phase 2 SLOs)
- **Real integrations:** VNeID + Digital Sig (Phase 18)
- **Observability:** Distributed tracing on 100% of traces (Phase 19)

