## Testing Infrastructure

### Test Project Organization
```
tests/
├── Directory.Build.props              # Global usings, shared test config
├── unit/
│   ├── GSDT.Identity.Tests/     # NSubstitute mocks, in-memory logic
│   ├── GSDT.Cases.Tests/
│   └── ... (21 unit tests total)
├── integration/
│   ├── GSDT.Identity.Integration.Tests/
│   └── ... (WebAppFixture + Testcontainers SQL Server)
└── architecture/
    └── GSDT.Architecture.Tests/  # NetArchTest.Rules layer enforcement
```

### Test Infrastructure Components

**Global setup (Directory.Build.props):**
- `using Xunit;`
- `using FluentAssertions;`
- `using NSubstitute;`
- Shared ItemGroup for test dependencies (xUnit, NSubstitute, FluentAssertions, Testcontainers)

**Unit test pattern:**
- NSubstitute for mocking repositories, services
- FluentAssertions for readable assertions
- No database or HTTP calls
- Test naming: `{Method}_{Scenario}_{ExpectedOutcome}`

**Integration test pattern:**
- **WebAppFixture:** Wraps WebApplicationFactory<Program>
  - Applies all module registrations
  - Configures TestAuthHandler (replaces JWT validation)
  - Exposes HttpClient, CreateScope() for DI access
- **SqlServerFixture:** Manages Testcontainers SQL Server
  - Fresh container per test class lifecycle
  - Runs all migrations before tests start
  - Disposes container on cleanup
- **TestAuthHandler:** Middleware that shortcuts JWT validation
  - Allows tests to set claims directly: `GetAuthToken(userId, roles, tenantId)`
  - No external auth server required

**Architecture test pattern:**
- NetArchTest.Rules fluent API
- Asserts Clean Architecture boundaries:
  - Domain ← Application ← Infrastructure ← Presentation
  - Domain has zero outbound dependencies
  - Application depends on interfaces only (ICacheService, not Redis directly)
  - Presentation references only Application (via MediatR)

### Comprehensive Test Coverage

**Backend Unit Tests: 1,808 total**
- All 8 core + 5 phase modules covered (Identity, Cases, Files, Notifications, Audit, Integration, MasterData, Organization, AI, Forms, Workflow, Reporting, SystemParams)
- Test naming: `{Method}_{Scenario}_{ExpectedOutcome}`
- NSubstitute mocks for dependencies, FluentAssertions for readability

**Backend Integration Tests: 32 total**
- Testcontainers SQL Server + WebAppFixture pattern
- Files: Upload, download, delete, validation, virus scanning, cross-tenant isolation
- Notifications: In-app/email persistence, mark as read, template rendering
- Organization: Create/update/delete org units, hierarchy validation
- Cache: Two-tier cache (L1/L2), hit/miss validation, Redis degradation
- Messaging: MassTransit event publishing, message routing, dead-letter handling
- BackgroundJobs: Hangfire job scheduling, execution, cleanup patterns

**Frontend Unit Tests: 491 vitest**
- React components: Organization, Reports, Delegations, API Keys, System Params
- Hooks: renderHook pattern for state updates, side effects
- Routes: Page smoke renders (each page mounts without crash)
- Utilities: format-date, format-file-size, data transformers

**Frontend E2E Tests: 140+ Playwright**
- Login flows, page navigation, dark mode, profile workflows
- Form submission, table operations, CSV export
- SignalR WebSocket: Real-time notification delivery, reconnection handling
- Page Object Models with Vietnamese label matching, self-cleanup via afterAll

**Compliance & Security Tests: 229 total**
- Contract tests: 24 API contracts (Case, User, Audit, SystemParams, Organization, MasterData, Notification, Report, Workflow, Files) + 8 event contracts
- Regression tests: 20 smoke suite (10 critical API paths) + domain operations
- Security tests: 69 (file upload security, SSRF prevention, security headers, PII masking, RBAC)
- Compliance tests: 48 (QĐ742 + PDPL: security headers, lockout signal, consent workflows, RTBF, audit trail)
- Authorization tests: 34 (permission codes, data scope filtering, policy evaluation)
- Architecture tests: 16 (Clean Architecture layer isolation via NetArchTest.Rules)

**Status (2026-04-07):** All **2,029+ tests passing** (100% pass rate)
