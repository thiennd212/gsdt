# System Architecture

## Overview

GSDT is a **production-ready full-stack** application combining:
- **Backend:** .NET 10 modular monolith (8 core modules + InvestmentProjects optional module, Clean/Onion, CQRS, DDD)
- **Frontend:** React 19 + TypeScript 5.7 (Vite, Ant Design, 90+ routes)
- **Auth Server:** Separate `GSDT.AuthServer` (OpenIddict OIDC/OAuth2)
- **Infrastructure:** Docker Compose (dev), Kubernetes + Helm (prod), Vault secrets

**Deployment:** Phase 1 = Monolith (single API + auth server + SPA). Phase 2 = Microservices (strangler pattern).

**Core Hosts:**
- **GSDT.Api** — All module controllers, 393+ REST endpoints, 13-layer middleware
- **GSDT.AuthServer** — OpenIddict 7.4.0, OIDC/OAuth2, external federation (JIT provisioning)
- **Web Frontend** — React 19 SPA, Vite build, Nginx reverse proxy, CSP headers

---

## Layer Model

```
┌─────────────────────────────────────────────┐
│              Presentation Layer              │  HTTP controllers, SignalR hubs
│    (GSDT.*.Presentation)               │  Inherits ApiControllerBase → ApiResponse<T>
├─────────────────────────────────────────────┤
│              Application Layer              │  CQRS commands/queries, MediatR handlers
│    (GSDT.*.Application)                │  FluentValidation, FluentResults, pipeline behaviors
├─────────────────────────────────────────────┤
│               Domain Layer                  │  Entities, value objects, domain events
│    (GSDT.*.Domain)                     │  Pure C# — zero infrastructure references
├─────────────────────────────────────────────┤
│           Infrastructure Layer              │  EF Core DbContexts, Dapper queries, repos
│    (GSDT.*.Infrastructure)             │  MassTransit consumers, external service clients
└─────────────────────────────────────────────┘
         ▲ all layers depend on ▼
┌─────────────────────────────────────────────┐
│              SharedKernel                   │  Interfaces, base classes, domain primitives
│    (GSDT.SharedKernel)                 │  NO infrastructure dependencies
└─────────────────────────────────────────────┘
```

**Rule:** Dependency arrows point inward only. Domain never references Application or Infrastructure.

---

## Full-Stack Component Map

```
Browser (Chrome, Safari, Mobile)
    │
    ├─ OIDC Auth Code Flow (PKCE)
    └─► GSDT.AuthServer (OpenIddict 7.4)
             ├─ External OIDC federation (JIT provisioning)
             ├─ MFA (TOTP via Google/Microsoft Authenticator)
             └─ Token issuance (JWT access + refresh)

    ├─ REST API + WebSocket (Bearer JWT)
    └─► GSDT.Api (ASP.NET Core 10)
             ├─ 13-layer middleware (security, correlation, rate limit, auth)
             ├─ 393+ REST endpoints (CRUD per module)
             ├─ SignalR hubs (real-time notifications)
             └─ 9 Modules:
                  • Identity (RBAC+ABAC, MFA, delegations, consent)
                  • Audit (HMAC-chained, RLS, RTBF, compliance)
                  • Cases (DDD example, state machine, CQRS)
                  • Files (MinIO, ClamAV, digital signatures)
                  • Notifications (Email, SMS, SignalR, templates)
                  • Integration (Partners, webhooks, YARP)
                  • MasterData (Hierarchies, dictionaries, GovernmentAgency, Investor)
                  • Organization (OrgUnits, staff, positions)
                  • InvestmentProjects (Domestic, ODA, PPP types; DesignEstimate shared)

    ├─ React SPA (Vite + TS + Ant Design)
    └─► 90+ Routes, 46 Feature Modules
             • Admin pages, dashboards, workflows
             • Dark mode + i18n (vi/en)
             • TanStack Query + Router
             • Form Builder (32 field types)
```

**Data Layer:** SQL Server 2022 + EF Core 10 (writes) + Dapper (reads) with global query filters (tenant + soft-delete isolation)

**Event Bus:** MassTransit → RabbitMQ (prod), in-memory (dev) — domain events trigger audit, notifications, workflows

**Cache:** Redis (distributed) + 2-tier strategy (L1 memory + L2 Redis) via TwoTierCacheService

**RLS:** TenantSessionContextInterceptor + 40 SQL policies + Dapper tenant validation

**Frontend (React SPA - V2.0):**
- Vite dev server (HMR), React 19 + TypeScript 5.7
- Ant Design 5.x with dynamic i18n locale switching (vi↔en, react-i18next)
- **Dark mode toggle** (reactive React context, localStorage persistence)
- TanStack Router with 18 lazy-loaded routes (code splitting, includes /profile)
- TanStack Query v5 (server state management)
- Zustand (client state), React Hook Form + Zod (forms)
- oidc-client-ts (PKCE OAuth2 flow to AuthServer)
- Axios HTTP client (auth token + correlation ID interceptors)
- Print-friendly CSS + Responsive Grid on 4 key pages
- ARIA accessibility on layout components (Sidebar, Topbar, AppLayout)
- 32 orval SDK modules auto-generated from OpenAPI
- Sidebar + core pages (Audit, Cases) fully translated
- LanguageSwitcher in topbar for vi/en toggle
- **SignalR notification toasts** (real-time push notifications)
- **Session timeout warning** (2 min before token expiry)
- **Error boundary + 404/403 error pages** with recovery options
- **Notification toasts** for user actions (create, update, delete)
- **Batch case operations** (checkbox selection, approve/reject multiple)
- **Announcement banner** on dashboard (configurable content)
- **Dark mode CSS variables** (navy, blue, red high-contrast variants)
- **User profile page** (/profile) with role assignment view
- CSP headers on nginx reverse proxy
- 75+ files, 13+ tests passing, Vite build time ~2.5s

**Backend Modules** communicate via:
1. **Domain Events (MassTransit)** — Async, decoupled, non-blocking (Audit log creation, Notifications, Webhooks)
2. **SharedKernel Interfaces** — Sync cross-module calls for critical workflows (e.g., RTBF data erasure)
   - Example: `IIdentityModuleClient` (defined in SharedKernel.Contracts, implemented by Identity.Infrastructure)
   - Used by Audit module to trigger PII erasure during RTBF requests
   - In-process (Phase 1) → RPC/HTTP (Phase 2 microservices)
   - **Forms Enhancement (2026-03-28):** Added `IConsentModuleClient` (for PDPL Article 11 consent recording), `IWorkflowModuleClient` (for automatic workflow instance creation on form submission), `IFormSubmissionStatusClient` (for updating form submission status from workflow state transitions)

**AI Module** provides LLM integration (Microsoft.Extensions.AI), vector embeddings (Qdrant), and semantic search with 4-layer data sovereignty.

**Reporting Module** provides async report generation (Hangfire), template-driven export (ClosedXML Excel, QuestPDF PDF), and compliance evidence collection.

**Authorization Architecture (v2.11 — 6 Phases A-F, COMPLETE):**

**Permission Resolution Layer:**
- **Phase A:** PermissionCode enum defines all system permissions (MODULE.RESOURCE.ACTION)
- **Phase B:** DataScopeService resolves 7 scope types (SELF, ASSIGNED, ORG_UNIT, ORG_TREE, CUSTOM_LIST, ALL, BY_FIELD)
  - ResolvedDataScope combines permissions + filtered record IDs
  - Query filters applied in handler layer (CommandHandler, QueryHandler)
- **Phase C:** PolicyRuleEvaluator evaluates NCalc expressions at runtime
  - EffectivePermissionService combines RBAC + scope + policy results
  - PermissionAuthorizationHandler enforces checks
- **Phase D:** 16 admin API endpoints for groups, data scopes, policy rules management
- **Phase E:** UserDelegation enhanced with approval flow + Redis-backed version caching (IPermissionVersionService)
- **Phase F:** SodConflictRule prevents segregation-of-duties violations; IMenuService resolves sidebar per user

**Row-Level Security (RLS) Architecture (v2.40 Enhanced):**
- **TenantSessionContextInterceptor:** EF Core interceptor called on every DbContext to set SQL Server SESSION_CONTEXT
  - Configured in shared `GSDT.Infrastructure` → all 16 modules register via DI
  - Execution: `sp_set_session_context(N'TenantId', @tenantId)` on connection open
  - **v2.40 Extension:** DapperReadDbConnection also enforces `sp_set_session_context` with connection leak fix (proper disposal on SESSION_CONTEXT failure)
- **SQL RLS Policies:** `fn_TenantFilter()` function enforces row visibility at DB layer
  - Applied to all audit-critical tables (AuditLogEntry, LoginAttempt, PersonalDataProcessingLog, RtbfRequest)
  - Query predicate: `WHERE org_id = CONVERT(uniqueidentifier, SESSION_CONTEXT(N'TenantId'))`
  - Zero-cost for non-sensitive tables; only active where tagged [DataClassification]
- **Dapper RLS (v2.40):** All Dapper queries enforcing tenant filter via `QueryWithTenantFilter` extension
  - Validates tenant GUID (non-empty) in all command handlers
  - BackgroundJobTenantContext ensures Hangfire jobs execute with correct tenant context
- **Outbox Interceptor (v2.40):** Preserves domain events during transaction rollback, ensuring no event loss
  - IntegrationEvent re-published to event bus after database commit
- **Audit Trail:** RLS policy violations logged as security events (SQL Server Audit + application audit log)

**Compliance Gap Fixes (v2.10-v2.11):**
- **nginx security headers:** `server_tokens off` suppresses version enumeration (QĐ742 §4.1)
- **Consent endpoints:** ConsentController grant/withdraw + domain events (PDPL Art. 11)
- **RTBF POST handler:** CreateRtbfRequestCommand calls IIdentityModuleClient for cross-module PII erasure (PDPL Art. 17)
- **Lockout signal:** LoginPost extends error response with `lockout_remaining_seconds` for UI countdown (QĐ742 §5.2)
- **Forms CRUD:** Update/Delete endpoints + field reorder for template management

**Security Hardening (v2.40 Audit Remediation — 2026-04-02):**

**Audit v1 Fixes:**
- **Dapper Tenant Isolation:** 3 queries + 2 controllers (ReadDbConnection enforces session context)
- **Hangfire Queue Migration:** Queue name normalization + dashboard middleware ordering fix
- **Auth Session UX:** onSilentRenewError notification prevents infinite redirect loops
- **Chat SignalR Auth:** Auth token validation on hub connection + pagination reset on search/filter
- **403 Notification, Error Boundaries, validateSearch:** UX improvements

**Audit v2 Fixes (Phases 1-5 COMPLETE):**
- **Phase 1:** Dapper RLS via `sp_set_session_context`, OutboxInterceptor event preservation, BackgroundJobTenantContext
- **Phase 2:** Copilot SSE auth token validation, open redirect validation (LocalRedirect), tenant GUID validation (non-empty Guid checks)
- **Phase 3:** 26 pages fetch-all for client-side search (no pagination for 300-item limits)
- **Phase 4:** AI controller tenant spoof prevention, SSRF validation (URI scheme/host checks), CaseRepository filter enforcement, bulk import limit (500 items)
- **Phase 5:** Report download auth (execution record validation), 401 redirect dedup, SignalR ref-counting (prevent double-subscribe), 20 pages Promise.allSettled for bulk ops, i18n completeness

**v2.38-v2.40 Tenant & User Resolution Pattern:**
- **ResolveTenantId() + ResolveUserId()** in `ApiControllerBase` extract identity from JWT claims only
  - 41 controllers updated: removed tenantId/createdBy/updatedBy/reviewedBy from query params and request DTOs
  - SystemAdmin cross-tenant access via `X-Tenant-Id` header (not query param)
  - All Extensions/ExtControllers (analytics, diff, bulk operations, PDF export) now enforce tenant isolation
- **SQL Injection Prevention:** Forms module `ListFormSubmissionsQueryHandler` validates field keys via regex pattern before dynamic SQL
- **CORS Hardening:** Restricted headers/methods enforcement
- **Open Redirect Prevention:** LocalRedirect validation, protocol-relative URL blocking
- **Path Traversal Defense:** Input validation + symbolic link checking
- **HTTPS Enforcement:** Redirect + HSTS headers
- **SQL Identifier Hardening:** Parameterized queries for all user input
- **Development Credentials:** Removed hardcoded passwords, moved to secure config (Vault)
- **Frontend Security:** Removed identity fields from 15+ API hooks (forms, workflow, cases, etc.); added `AdminTenantSelector` component for SystemAdmin cross-tenant debugging
- **Test Coverage:** All 41 controller changes verified with unit + integration tests

**JIT SSO Provisioning Authentication (v2.45):**
- **External OIDC Schemes:** AuthServer registers external providers via appsettings (OpenID Connect, Google, Microsoft, custom IdP)
- **Just-In-Time Provisioning:** JitProvisioningService auto-creates ApplicationUser + ExternalIdentity on first external login
- **Security Defenses (4 Red Team Mitigations):**
  - **RT-01 Email Squatting:** ExternalIdentity by (Provider, ExternalId) prevents email-based account takeover
  - **RT-02 Domain Whitelist:** JitProviderConfig.AllowedEmailDomains validates email domain (regex pattern) if configured
  - **RT-03 Tenant Requirement:** JitProviderConfig.RequireTenant enforces user must specify TenantId (prevents cross-tenant abuse)
  - **RT-04 Rate Limiting:** In-memory ConcurrentDictionary tracks provisioning attempts per provider scheme, hourly window, max 100 attempts/hour
- **Admin Configuration:** JitProviderConfigsController (5 REST endpoints) manages per-provider config (enabled, domain whitelist, auto-create domain, max hourly limit)
- **Admin UI:** /admin/jit-provider-configs React page (table, create/edit modals, delete confirmation)
- **Error Handling:** JitResult enum codes (deactivated, pending_approval, provision_failed, jit_disabled, domain_not_allowed, rate_limited, no_tenant)
- **Audit Trail:** JitProvisioningService logs provision attempts + failures to Audit module via IAuditService
- **ExternalLogin Integration:** ExternalLoginController.Callback() invokes JitProvisioningService before returning token

**V2 Features:**
- **Health Check Dashboard** (/admin/health) — monitor API, AuthServer, DB, Redis, RabbitMQ, MinIO health
- **User Role Assignment** (POST /admin/users/{id}/roles) — admins grant/revoke roles dynamically
- **Form Field Reorder** (PATCH /form-templates/{id}/fields/{fieldId}/order) — reorder fields with position index
- **Workflow Progress UI** — Steps component in case-detail showing workflow state machine progression
- **Audit Log CSV Export** (GET /audit/logs/export) — batch download logs with tenant isolation
- **API Rate Limiting** — 100 req/min (authenticated), 20 req/min (anonymous), per-tenant tracking
- **Frontend Env Validation** — startup check for required env vars (BASE_URL, OIDC configs)
- **JIT SSO** — Auto-provision external OIDC users on first login with 4-layer security (email squatting, domain whitelist, tenant requirement, rate limiting)

---

## Phase 1: Domain Model Expansion (v2.14)

**22 new entities across 7 modules** add critical governance, compliance, and automation capabilities:

**External Identity & Access Control (M01: Identity)**
- ExternalIdentity, CredentialPolicy, ExternalMapping — Enable federated identity, policy-driven credential management, cross-system user resolution

**Reference Data Management (M02: MasterData, M05: SystemParams)**
- Dictionary, DictionaryItem, DictionaryItemVersion — Centralized reference lists (status codes, classifications) with versioning

**Workflow & Task Automation (M07: Workflow)**
- **CRUD + Versioning (Phase 1):** WorkflowDefinition with DefinitionKey, Version, IsLatest tracking; UpdateWorkflowDefinition, DeleteWorkflowDefinition, CloneWorkflowDefinition commands; PUT/DELETE/POST-clone API endpoints
- **Condition Evaluation (Phase 2):** DeclarativeConditionEvaluator with 11 operators (equals, notEquals, greaterThan, lessThan, gte, lte, in, notIn, contains, isNull, isNotNull); WorkflowCondition value object; metadata dict in ExecuteTransitionCommand
- **Notification Integration (Phase 3):** WorkflowNotificationConfig entity (per-definition, per-action rules); WorkflowTransitionedNotificationHandler (MediatR → INotificationModuleClient); CRUD API endpoints
- **Tenant-Workflow Assignment (Phase 3.5):** WorkflowAssignmentRule with 4-level specificity (SystemDefault < Tenant < TenantAndEntity < TenantEntitySubType); WorkflowAssignmentResolver with ICacheService (24h TTL); auto-resolve in CreateWorkflowInstance; GET/POST/DELETE /workflow/assignments + GET /resolve
- **Frontend Definition Management (Phase 4):** Visual designer for workflow states/transitions via React Flow canvas; SaveDefinitionGraphCommand replaces states+transitions (PUT /definitions/{id}/graph); Activate/Deactivate endpoints (POST /definitions/{id}/activate, /deactivate); 20 FE components (WorkflowDesigner, StateNodeComponent, TransitionEdgeComponent, StatePropertiesModal, TransitionPropertiesModal, DefinitionToolbar, etc.)
- **Parallel Branching (Phase 5):** 5 new entities (WorkflowParallelBranch, WorkflowBranchChild, WorkflowBranchResolution, WorkflowInstanceBranchStatus, WorkflowBranchChildStatus); ParallelBranchService with AND/OR/N-of-M join logic; BranchTimeoutCheckerJob for deadline enforcement; ExecuteTransition integrated with parallel branch resolution; ResolveBranchChildCommand for manual branch completion; GetBranchStatusQuery for monitoring branch state; 6 new API endpoints
- **Frontend Inbox & Monitoring (Phase 6):** Generic WorkflowInstance inbox page (replaces Cases-only view); Instance detail page with transition timeline; KPI monitoring dashboard with instance state metrics; 7 new React hooks for tasks/instances (useWorkflowTasks, useWorkflowInstances, useBranchStatus, etc.)
- **Advanced Features (Phase 7):** SlaHours per WorkflowState (per-state deadline tracking); IsRecall on WorkflowTransition (creator-only withdraw); WorkflowTransitionAttachment entity linking Files module; AutoTransitionOnTimeoutId for auto-escalation; SlaBreachCheckerJob extended with per-state breach detection; IWorkflowResolver interface + DynamicWorkflowResolver (Cases↔Dynamic bridge); **E2E Playwright tests:** 28 workflow tests covering CRUD, graph save, activate/deactivate, parallel branching, transitions, SLA monitoring, recall operations, attachment handling; **EF Concurrency Fixes:** SaveGraph uses raw SQL bypass for bulk upsert (EF SaveChanges limitations), WorkflowInstanceHistory detected-as-Modified→Added state fix (EF entity tracking scope), auto-retry on RowVersion conflicts
- **Task Management:** WorkflowTask, TaskAssignment, EscalationRule, WorkflowProcessVersion — deadline tracking, escalation triggers, version history

**Document Lifecycle Management (M08: Files)**
- DocumentTemplate, DocumentTemplateVersion, DocumentArchive, FileVersion, RetentionPolicy, RecordLifecycle — End-to-end document governance: creation → retention → disposal, version control, compliance tracking

**Integration & Monitoring (M13: Integration, M14: Notifications)**
- EventCatalogEntry — Event schema registry for inter-module contracts
- AntiDoubleSubmitMiddleware — Idempotency protection via request tokens
- AlertRule, Runbook — Condition-based alerts and automated remediation

**Background Job Support (3 Hangfire Jobs)**
- EscalationCheckJob (15m), RetentionPolicyEnforcementJob (daily), AlertEvaluationJob (5m)

**10 New REST APIs** across document management, workflow, external identity, and admin monitoring domains. All Phase 1 entities auto-inherit TenantId multi-tenancy + soft-delete support.

---

## Infrastructure Components

| Component | Role | Dev Setup |
|---|---|---|
| SQL Server 2022 | Primary OLTP store | Docker Compose |
| Redis | Distributed cache, SignalR backplane | Docker Compose |
| MinIO | Object storage (files, exports) | Docker Compose |
| RabbitMQ 4.x | Async messaging (MassTransit) | Docker Compose |
| HashiCorp Vault | Secrets management | Docker Compose (required in dev) |
| Hangfire | Background job scheduling | SQL Server store |
| OpenTelemetry | Traces + metrics export | OTLP → Grafana/Prometheus |
| Serilog | Structured JSON logging | Seq in dev |

---

## Typical GOV Request Flow (Frontend → Backend)

```
React SPA (Browser)
    │
    ├─ User triggers action (submit form, list cases, etc.)
    │
    ├─ React Hook Form validates locally (Zod schema)
    │
    └─► Axios HTTP Request
        ├─ Authorization header: "Bearer {JWT access token}"
        ├─ X-Correlation-ID: {UUID for tracing}
        ├─ Content-Type: application/json
        │
        ▼
    [HTTPS] GSDT.Api
        │
        ├─ SecurityHeaders middleware (CSP, HSTS, X-Frame-Options)
        ├─ CorrelationId middleware (enriches logs)
        ├─ RateLimit middleware (per tenant)
        ├─ JWT validation (Bearer token → OpenIddict introspection)
        │       └─ extracts claims: sub (user ID), tenant_id, roles, dept_code, clearance_level
        │
        ├─ Controller (ApiControllerBase)
        │       └─ mediator.Send(command)
        │
        ├─ MediatR Pipeline
        │       ├─ TenantAwareBehavior → injects ITenantContext from JWT claim
        │       └─ ValidationBehavior → FluentValidation, returns 422 on failure
        │
        ├─ CommandHandler / QueryHandler
        │       ├─ Domain logic (aggregate methods, business rules)
        │       ├─ Repository.AddAsync (EF Core write) or IReadDbConnection (Dapper read)
        │       └─ Returns Result<T>
        │
        ├─ ApiControllerBase.ToApiResponse(result)
        │       └─ maps Result errors → HTTP status codes
        │
        ▼
    ApiResponse<T> JSON envelope (RFC 9457 format)
        {
          "success": true,
          "data": {...},
          "meta": {...},
          "errors": []
        }
        │
        ├─ [200] Success response
        ├─ [401] Unauthorized (token expired/invalid)
        ├─ [403] Forbidden (insufficient clearance/role)
        ├─ [422] Validation error (detail_vi: Vietnamese error text)
        └─ [500] Server error
        │
        ▼
    React SPA
        │
        ├─ TanStack Query (axios interceptor) unwraps ApiResponse envelope
        ├─ Zustand state update (store response data)
        ├─ Component re-renders with fresh state
        └─ Ant Design Table / Form / Drawer displays result
```

**Async Post-Response (Backend):**
After the HTTP response is sent, domain events are dispatched to MassTransit consumers:
- Audit log creation (every state change)
- Email/SMS notifications (if subscribed)
- Cross-module workflows (Workflow engine state machine)
- External webhooks (Webhook engine)

**Error Handling Flow:**
- Validation errors: FluentValidation → 422 with field-level detail_vi messages
- Not Found: 404 with custom error code (e.g., `GOV_CASE_001`)
- Authorization: 403 Forbidden (ABAC clearance cap or RBAC role check failed)
- Rate limit: 429 Too Many Requests (per tenant rate limiter)

---

## Event-Driven Architecture

**Domain events** flow through MassTransit to module consumers (in-memory in Phase 1, RabbitMQ in Phase 2). Event contracts defined in `SharedKernel.Contracts`; full AsyncAPI 3.0 schema documented in [`docs/asyncapi.yaml`](asyncapi.yaml).

**Events enable:**
- Audit log capture (every domain event → audit record)
- Notifications (case updated → email/SMS subscribers; FormSubmitted/FormApproved/FormRejected → subscribers)
- Cross-module workflows (case created → assign to workflow engine; FormSubmitted → auto-create workflow instance)
- External webhooks (via Webhook Engine)

**Core Domain Events:**
- **Identity:** UserCreated, UserUpdated, LoginAttempted, LogoutCompleted, MfaEnabled
- **Audit:** AuditLogCreated, RtbfRequestSubmitted
- **Notifications:** NotificationSent, EmailDelivered, SmsDelivered
- **Cases:** CaseCreated, CaseStatusChanged, CaseCommented, CaseApproved
- **Workflow:** WorkflowInstanceCreated, WorkflowTransitioned, WorkflowCompleted
- **Forms:** FormSubmitted, FormApproved, FormRejected (new in v2.20.1)

---

## CQRS Split

**Writes:** EF Core aggregate repositories (`IRepository<T, TId>`).
**Reads:** Dapper via `IReadDbConnection` — raw SQL, no N+1, no change tracking overhead.

```
POST /cases          → CreateCaseCommand → EF Core (write)
GET  /cases          → ListCasesQuery    → Dapper  (read, optimised SQL)
```

**Forms Module Exception:** Dual-mode storage pattern:
- **StorageMode.Json:** Write via EF Core SubmissionData column (JSONB), read via Dapper from single SubmissionData field
- **StorageMode.Table:** Write via Dapper parameterized INSERT to dynamic `forms.Submissions_{templateId:N}` table, read via Dapper raw SQL with type-safe columns
- Both modes use FormMaterializationService for async DDL creation at publish time

### Global Query Filters (Multi-tenancy Enforcement - 2026-03-21)

**EF Core Global Query Filter:**
All ITenantScoped entities automatically filtered by TenantId at query time (2026-03-21 implementation):

```csharp
// In DbContext OnModelCreating:
builder.Entity<Case>()
    .HasQueryFilter(c => c.TenantId == _tenantContext.TenantId);

builder.Entity<User>()
    .HasQueryFilter(u => u.TenantId == _tenantContext.TenantId);
```

**Benefits:**
- Zero-configuration tenant isolation (no manual WHERE TenantId filters needed)
- Prevents accidental cross-tenant queries
- Filter applies to: EF Core writes, IRepository implementations, related entity loads
- Dapper queries remain explicit (developer responsibility via @TenantId parameter)

**Combined Pattern (Soft-Delete + TenantId):**
```csharp
builder.Entity<Case>()
    .HasQueryFilter(c => c.TenantId == _tenantContext.TenantId && !c.IsDeleted);
```
Ensures deleted records filtered per tenant, not globally.

### Known EF Core Concurrency Patterns (v2.24)

**Workflow Module — Documented Patterns:**
1. **SaveDefinitionGraph (Phase 4):** Raw SQL `MERGE` statement bypasses EF SaveChanges for bulk upsert of states/transitions (EF limitation: cannot efficiently upsert multiple aggregates in single transaction). See `SaveDefinitionGraphCommandHandler` for pattern.
2. **ExecuteTransition History (Phase 7):** WorkflowInstanceHistory entries detected as Modified→Added by EF change tracker in multi-entity scope. Fixed via `StateManager.ChangeTracker.Clear()` before history insert. Auto-retry on RowVersion conflicts via `OptimisticConcurrencyException` handler.
3. **RowVersion Concurrency Token:** SQL Server `ROWVERSION` columns on aggregate roots; EF validates on Update/Delete. Known false positives when multiple entities modified in same DbContext scope—mitigated by entity-level change tracking inspection.

**Mitigation Strategy:**
- Unit tests verify RowVersion conflict detection (11 concurrency race condition tests)
- Integration E2E tests (28 workflow tests) verify retry logic under load
- No breaking changes to API contracts; handled internally in handler layer

---

## Authorization Architecture (v2.11 Complete)

### Permission Resolution Flow

```
User Request
    │
    ├─ JWT claims extracted: sub (userId), roles, tenant_id, dept_code
    │
    ├─ Phase A: Permission Codes
    │   └─ Map request action to PermissionCode (e.g., GET /cases → AUTH.CASE.READ)
    │
    ├─ Phase B: Data Scope Resolution (IDataScopeService)
    │   ├─ Resolve scope type: SELF, ASSIGNED, ORG_UNIT, ORG_TREE, CUSTOM_LIST, ALL, BY_FIELD
    │   └─ Return ResolvedDataScope (effective permissions + filtered records)
    │
    ├─ Phase C: Policy Rule Evaluation (IPolicyRuleEvaluator)
    │   ├─ Load PolicyRules matching user role + resource type
    │   ├─ Evaluate NCalc conditions (e.g., department_code == "CUSTOMS" AND classification_level >= 3)
    │   └─ Combine results: ALLOW if RBAC + scope + policy all pass
    │
    ├─ Phase E: Delegation Check (IPermissionVersionService)
    │   ├─ Check active delegations (UserDelegation.Status == Active)
    │   ├─ Verify delegation not expired
    │   └─ Include delegated roles in effective permissions
    │
    ├─ Phase F: SoD Conflict Check (ISodConflictChecker)
    │   ├─ Verify role combination doesn't violate SodConflictRule
    │   └─ Reject if conflict detected
    │
    └─► PermissionAuthorizationHandler
        └─ Return 403 Forbidden if any check fails, else allow request

Query Handler (Phase B Applied)
    │
    ├─ Apply ResolvedDataScope.FilteredRecordIds to WHERE clause
    │ (Dapper: WHERE id IN @recordIds AND TenantId = @tenantId)
    │
    └─► Return filtered result set
```

### 6 Authorization Phases (A-F)

| Phase | Component | Details |
|-------|-----------|---------|
| **A** | PermissionCode | MODULE.RESOURCE.ACTION enum; AUTH.USER.READ, AUTH.ROLE.ASSIGN, etc.; RoleType enum (Admin, Officer, Citizen) |
| **B** | DataScopeService | 7 scope types; ResolvedDataScope DTO; automatic query filtering |
| **C** | PolicyRuleEvaluator | NCalc expression engine; EffectivePermissionService combines RBAC+scope+policy |
| **D** | Admin APIs | 16 endpoints: GroupsAdminController (8), DataScopeAdminController (4), PolicyRulesAdminController (4) |
| **E** | Delegation+ | DelegationStatus enum; IPermissionVersionService (Redis version cache); DelegationExpiryJob |
| **F** | SoD + Menu | SodConflictRule detection; AppMenu + MenuRolePermission; IMenuService sidebar resolution |

### Data Scope Types (Phase B)

| Type | Description | Example |
|------|-------------|---------|
| **SELF** | User's own records only | User can see only their own profile |
| **ASSIGNED** | Records assigned to user | Cases assigned to me (case.assignedUserId == me) |
| **ORG_UNIT** | Same organizational unit | Can see all cases in my department |
| **ORG_TREE** | Organizational hierarchy | Manager sees reports + reports' subordinates |
| **CUSTOM_LIST** | User-specific list (DB config) | VIP clients, priority cases |
| **ALL** | All records (no filter) | System admin sees everything |
| **BY_FIELD** | Dynamic field-based filter | Records where field_x = user.attribute_y |

### Policy Rule Examples (Phase C)

```csharp
// Rule 1: High-security access
PolicyRule:
  Description: "Users can only view cases with classification ≤ their clearance"
  Condition: "case.classification_level <= user.clearance_level"
  PermissionCode: "AUTH.CASE.READ"

// Rule 2: Department-based access
PolicyRule:
  Description: "Officers can only view cases in their department"
  Condition: "case.department_code == user.dept_code"
  PermissionCode: "AUTH.CASE.VIEW_DETAIL"

// Rule 3: Time-based access
PolicyRule:
  Description: "Reports available only during business hours (8am-6pm)"
  Condition: "NOW().HOUR() >= 8 AND NOW().HOUR() < 18"
  PermissionCode: "AUTH.REPORT.EXECUTE"
```

---

## Repository Pattern Enforcement

**Identity Module** establishes the enforced repository pattern:

**Domain Layer** (GSDT.Identity.Domain/Repositories/):
- `IConsentRepository` — Consent aggregate persistence contract
- `IDelegationRepository` — Delegation aggregate persistence contract
- `IAttributeRuleRepository` — Attribute-based access control rules

**Infrastructure Layer** (GSDT.Identity.Infrastructure/Persistence/):
- `ConsentRepository` — EF Core implementation with Identity DbContext
- `DelegationRepository` — EF Core implementation with Identity DbContext
- `AttributeRuleRepository` — EF Core implementation with Identity DbContext

**Application Handlers** depend on **repository interfaces only**, not infrastructure concrete classes:
- `GrantConsentCommandHandler(IConsentRepository)` — no IdentityDbContext reference
- `WithdrawConsentCommandHandler(IConsentRepository)` — no IdentityDbContext reference
- `DelegateRoleCommandHandler(IDelegationRepository)` — no IdentityDbContext reference
- `RevokeDelegationCommandHandler(IDelegationRepository, ICacheService)` — uses SharedKernel abstraction for cache
- `AbacAuthorizationHandler(IAttributeRuleRepository)` — no IdentityDbContext reference

This pattern is the template for all new modules — define repository interfaces in Domain, implement in Infrastructure, inject into Application handlers.

---

## Authentication Architecture

```
Client ──[OIDC auth code flow + PKCE]──► GSDT.AuthServer
                                              │ (OpenIddict 7.4.0 + ASP.NET Identity)
                                              │ AuthorizationController (PKCE + ROPC + login page)
                                              │ VNeID federation (NĐ59)
                                              │ MFA/TOTP (QĐ742)
                                              ▼
                                      Access Token (JWT)
                                              │
Client ──[Bearer token]────────► GSDT.Api
                                       │
                               JWT introspection validation (AuthServer endpoint)
                               RBAC: role claims
                               ABAC: dept_code + ClassificationLevel
                               Token revocation on role change
```

**Dev User:** admin@dev.local / DevAdmin@12345

**Docker Discovery:** AuthServer IssuerUri uses `host.docker.internal:5000` in containers; direct `localhost:5002` in dev.

**API Token Validation:** Calls AuthServer `/introspect` endpoint for active token verification (not just JWT decode).

API Key M2M auth (SHA-256 hashed, Redis cached) is available for service-to-service calls.

---

## Security Architecture

| Layer | Control | Standard | Status |
|---|---|---|---|
| Transport | TLS 1.2+ enforced | OWASP | ✓ Complete |
| Headers | SecurityHeadersMiddleware (CSP, HSTS, X-Frame-Options) | OWASP | ✓ Complete |
| Auth | OpenIddict OIDC, JWT Bearer, API Keys | NĐ59, QĐ742 | ✓ Complete (token revocation, MFA rate limiting) |
| Authorization | RBAC (roles) + ABAC (ClassificationLevel, DepartmentCode) | QĐ742 | ✓ Complete (clearance cap, cache invalidation) |
| PII at rest | Always Encrypted columns (SQL Server) | PDPL/Law 91 | ✓ Complete |
| Audit | HMAC-chained append-only log | NĐ53 | ✓ Complete (schema validation) |
| RTBF | PII anonymization via ProcessRtbfRequestCommand | Law 91 Art.9 | ✓ Complete (F-04) |
| Secrets | HashiCorp Vault (no plaintext secrets in config) | TT12 | ✓ Complete |
| SSRF | ExternalRef URL whitelist, DNS rebinding prevention | OWASP | ✓ Complete |
| Test Isolation | TestAuthHandler in integration tests only | Security | ✓ Complete |
| CI pipeline | SAST (SonarQube) + DAST (OWASP ZAP) + Trivy + SBOM | NĐ85/TT12 | ✓ Complete |
| Security Audit | 31/35 findings remediated (Groups A-D) | Phase Audit | 88% (E-F pending Q2) |

---

## Testing Architecture (Phases 1-4 Complete)

### Test Pyramid
```
┌─────────────────────────────────────────┐
│   End-to-End Tests (k6 load testing)    │  4 module profiles + baseline
├─────────────────────────────────────────┤
│   Integration Tests (33 module + 96 legacy)  │  Testcontainers SQL Server
├─────────────────────────────────────────┤
│   Unit Tests (269)                      │  NSubstitute mocks, no I/O
├─────────────────────────────────────────┤
│   Architecture Tests (16)                │  NetArchTest.Rules layer enforcement
└─────────────────────────────────────────┘
```

### Unit Testing (269 tests)
- NSubstitute mocks for all external dependencies (repositories, services)
- FluentAssertions for readable assertions
- Test naming: `{Method}_{Scenario}_{ExpectedOutcome}`
- Covers: Command/query handlers, domain aggregates, PII anonymization, RTBF workflows
- No database, no HTTP — fast feedback loop (~30 seconds)

### Integration Testing (102 tests verified Docker, 2026-03-22)
- **WebAppFixture:** Full DI container with all module registrations, real handler pipeline
  - **v2.11 Fix:** Gateway:Mode=Disabled in appsettings (prevents YARP routing conflicts in tests)
  - **v2.11 Fix:** DefaultTenantId mock for ITenantContext when not authenticated
- **SqlServerFixture:** Testcontainers SQL Server 5.2.1, fresh container per test class, migrations auto-applied
  - Supports cross-tenant isolation tests (7 tests) — verifies EF Core global query filters by TenantId
  - Supports concurrency race condition tests (11 tests) — verifies RowVersion optimistic locking under concurrent load
- **TestAuthHandler:** Middleware intercepts JWT validation, allows tests to auth as any principal (userId, roles, tenantId, department)
- **DatabaseFixture:** Legacy tests with env var overrides for CI/CD (no hardcoded connection strings)
- **Docker Integration Test Runner:** Standalone script executes full integration suite in Linux container environment
  - EF migrations regenerated via Docker `dotnet-ef` tooling (2026-03-21)
  - Enables CI/CD pipeline Docker-based test execution
  - All 102 integration tests verified passing (2026-03-22)
- Covers: API contracts, database persistence, event publishing, cross-module workflows, tenant isolation, race conditions
- Execution: ~1-2 minutes per test class (container startup amortized)

### Architecture Testing (16 tests)
- **NetArchTest.Rules** enforces Clean Architecture boundaries
- Domain layer: zero external dependencies
- Application layer: interfaces only, no concrete infrastructure
- Infrastructure layer: implementations, no presentation refs
- Presentation layer: only Application via MediatR
- Run at build time: violations block compilation

### k6 Performance Testing
- **Load test:** 50 VUs for 5 minutes, target p95 <500ms
- **Spike test:** 100→500 VUs over 2 minutes, max error 1%
- **Soak test:** 10 VUs for 70 minutes, max error 0% (endurance)
- **Module-specific:** Audit, Identity, Files, Notifications (20 VUs, 3 min each)
- API Key authentication (shared auth.js helpers)
- CI/CD gate: all thresholds must pass before merge

### Test Coverage Metrics (v2.24 Final - 2026-03-27)
| Category | Count | Status |
|----------|-------|--------|
| BE Unit tests | 293+ | ✓ All passing |
| BE Integration (Docker) | 102 | ✓ All passing (Testcontainers SQL Server 5.2.1) |
| Forms Integration | 7 | ✓ All passing (YARP + StorageMode validation) |
| FE vitest tests | 375+ | ✓ All passing |
| E2E Playwright tests | 81+ | ✓ All passing (compliance + browser + SignalR + **28 Workflow Phase tests**) |
| Architecture tests | 16+ | ✓ All passing |
| k6 profiles | 7 (4 module + load/spike/soak) | ✓ All passing |
| **Total** | **800+** | **100% passing** |

**Frontend npm audit:** 0 vulnerabilities

**Integration Status:**
- All enum contracts aligned (CaseStatus, FormFieldType, FormTemplateStatus, ChatRole)
- JsonStringEnumConverter added to API (snake_case serialization)
- orval SDK: 32 API modules auto-generated from live OpenAPI

---

## Frontend Architecture (Phase 15, Bootstrap Complete)

### Component Structure (V1.5)

**src/:** auth (AuthProvider, useAuth, RouteGuard), api (Axios, queries, ApiResponse<T>), i18n (react-i18next vi/en), layout (AppLayout, Sidebar, Topbar, LanguageSwitcher), pages (Audit, Cases, 12+ lazy routes), hooks (useServerPagination, useQuery, useForm), theme (Ant Design GOV colors), router (TanStack 16 routes), styles (global.css, print.css).

**i18n Setup:** react-i18next with vi/en, Sidebar + Audit + Cases fully migrated, LanguageSwitcher in topbar, Ant Design locale sync (vi_VN ↔ en_US), detail_vi mapping from API.

### Data Flow Pattern

useQuery → TanStack Query cache → Axios (JWT + X-Correlation-ID) → Backend ApiResponse<T> → Response interceptor unwraps envelope → Zustand store → Component renders. **Code Splitting:** 16 lazy routes via React.lazy() + Suspense with Vite auto-chunking.

### Authentication Flow (OIDC PKCE)

User → OIDC authorize endpoint (code_challenge, PKCE) → AuthServer login form (admin@dev.local / DevAdmin@12345 + optional MFA) → Authorization code → oidc-client-ts exchanges code for JWT (stored in memory) → AuthProvider context updates → API calls include Bearer token → AuthServer /introspect validates → Silent refresh on 401 → Logout revokes tokens.

### State Management Strategy

**Client (Zustand):** User session, UI state (sidebar, theme, language, delegate-as mode). **Server (TanStack Query):** Audit, cases, forms, reports with automatic cache invalidation on mutation. **Form (React Hook Form + Zod):** Validation schema, field-level errors (detail_vi), optimistic updates.

### Error Handling Strategy

**HTTP:** 401 → refresh/login, 403 → "Access Denied", 422 → field errors (detail_vi), 500 → Sentry + generic message. **Component:** Error boundary, loading skeleton, empty state, retry button.

---

## AI Copilot Features (v2.18)

### Copilot Sidebar

**Architecture:** Collapsible Ant Design Drawer on all authenticated pages. Shared across app via CopilotProvider context and useCopilotChat hook.

- **Components:**
  - `CopilotProvider` (context wrapper at app root)
  - `useCopilotChat()` hook (access chat state + send message)
  - `CopilotSidebar` (collapsible drawer with message history)
  - `CopilotChatPage` (full-page view with scroll management)

- **State Management:** Zustand store persists conversation history (cleared on logout)
- **API:** POST /api/v1/ai/chat endpoint (receives message, returns streaming response)
- **UX:** Accessible via button in topbar; persistent across navigation; scrolls to latest message

### OCR Document Extractor

**Service:** `OllamaDocumentExtractor` (replaces stub implementation).

- **Endpoint:** POST /api/v1/ai/extract
- **Input:** Base64-encoded image, file metadata
- **Processing:** Multimodal vision model (Ollama with vision support)
- **Output:** Extracted text, detected fields, confidence scores
- **Use Case:** Scan government forms, extract data fields, pre-fill case forms

**Infrastructure:**
- Ollama service (Docker Compose) runs vision model
- Alternative: Azure.AI.OpenAI with Gpt4Vision model (cloud deployments)
- Response is cached per document hash (Redis)

### ReAct Agent

**Service:** `OllamaReActAgent` (replaces stub implementation).

- **Endpoint:** POST /api/v1/ai/agent/execute
- **Input:** User query (string), context (optional case/document ID)
- **Tool Set (4 built-in):**
  - `search_cases` — Query cases by status, assignee, date range
  - `get_case_detail` — Fetch case with comments, attachments, history
  - `classify_text` — Classify text snippet (urgency, category, sentiment)
  - `summarize` — Summarize case details or document content

- **Loop:** Thought → Action → Observation → Thought (max 5 iterations to prevent infinite loops)
- **Model:** Ollama text model (tuned for reasoning)
- **Output:** Structured response with reasoning trace + final answer
- **Use Case:** "Summarize urgent cases assigned to me" → agent searches, classifies, summarizes in one call

**Safety Constraints:**
- Action whitelist (only 4 tools callable)
- Iteration limit (5 max)
- Result cached per query hash (30 min TTL)

### Data Sovereignty & Compliance

- **Tenant Isolation:** All OCR/ReAct results tagged with TenantId (RLS policy enforces at DB layer)
- **PII Handling:** Extracted text scanned by Governance module before returning to user
- **Audit Trail:** AI operations logged to AuditLogEntry (request, model output, tool calls)
- **Retention:** Query cache expires after 30 minutes; full conversation history optional (user consent required)

---

## Phase 2 Microservices Path

Each module is already isolated: separate DbContext, no cross-module direct references, async messaging via MassTransit. Phase 2 extraction steps:

1. Move module to separate solution/repo
2. Expose module API over HTTP (already versioned)
3. Replace in-process MassTransit with RabbitMQ transport (config-only change)
4. Update API gateway routing (YARP config)

No domain logic changes required — architecture enforces this.
