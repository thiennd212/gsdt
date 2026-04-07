# Code Standards

## File & Namespace Naming

| Artifact | Convention | Example |
|---|---|---|
| C# files | PascalCase | `CreateCaseCommandHandler.cs` |
| Namespaces | Match folder structure | `GSDT.Cases.Application.Commands.CreateCase` |
| Interfaces | `I` prefix | `ICaseRepository`, `ICurrentUser` |
| Enums | PascalCase singular | `CaseStatus`, `ClassificationLevel` |
| Test files | `{Subject}Tests.cs` | `CaseStateMachineTests.cs` |

Namespace root per module: `GSDT.{ModuleName}.{Layer}`.

---

## CQRS Patterns

### Command naming
```
CreateCaseCommand        → CreateCaseCommandHandler  → CreateCaseCommandValidator
SubmitCaseCommand        → SubmitCaseCommandHandler
ApproveCaseCommand       → ApproveCaseCommandHandler
```

### Query naming
```
ListCasesQuery           → ListCasesQueryHandler
GetCaseByIdQuery         → GetCaseByIdQueryHandler
```

### Command record pattern
```csharp
// Commands are immutable records — no setters
public sealed record CreateCaseCommand(
    Guid TenantId,
    string Title,
    string Description,
    CaseType Type,
    CasePriority Priority) : ICommand<CaseDto>;

// ICommand<T> is shorthand for IRequest<Result<T>>
// IQuery<T> is shorthand for IRequest<Result<T>> (read-side)
```

### Handler pattern
```csharp
public sealed class CreateCaseCommandHandler(
    ICaseRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreateCaseCommand, Result<CaseDto>>
{
    public async Task<Result<CaseDto>> Handle(
        CreateCaseCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Domain logic via aggregate factory/methods
        // 2. Persist via repository
        // 3. Return Result.Ok(dto) or Result.Fail(error)
    }
}
```

### Validator pattern
```csharp
public sealed class CreateCaseCommandValidator : AbstractValidator<CreateCaseCommand>
{
    public CreateCaseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(500);

        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
```

All validation messages must have a Vietnamese version (`WithMessage`).

---

## Domain Entity Conventions

### Base classes
```
Entity<TId>                    → Id property, equality by Id
AuditableEntity<TId>           → + CreatedBy, ModifiedBy, ClassificationLevel
AggregateRoot : AuditableEntity → + DomainEvents collection
```

### Aggregate factory method pattern (no public constructors)
```csharp
public sealed class Case : AuditableEntity<Guid>, IAggregateRoot
{
    private Case() { }  // EF Core only

    public static Case Create(/* params */) { ... }   // factory
}
```

### State transitions — always via WorkflowEngine
```csharp
public void Submit(Guid actorId)
{
    var result = CaseWorkflow.Engine.Execute(Status, CaseAction.Submit);
    if (result.IsFailed)
        throw new InvalidCaseTransitionException(Status, CaseAction.Submit);
    Status = result.Value;
    SetAuditUpdate(actorId);
    AddDomainEvent(new CaseSubmittedEvent(Id, TenantId, actorId));
}
```

### DataClassification Attribute Pattern
Tag PII-containing properties with `[DataClassification]` to enforce governance:

```csharp
public sealed class ApplicationUser : AuditableEntity<Guid>
{
    [DataClassification(DataClassificationLevel.Confidential)]
    public string Email { get; set; }

    [DataClassification(DataClassificationLevel.Restricted)]
    public string PhoneNumber { get; set; }

    [DataClassification(DataClassificationLevel.Internal)]
    public string FullName { get; set; }
}
```

**Levels:** Public (default) → Internal (employees only) → Confidential (staff + subject) → Restricted (audit-only)
- Governs RLS row-level encryption, audit trail detail level, data retention policies
- Audit module logs access to Restricted properties
- RTBF erasure respects classification (Restricted fields purged first)

---

## Repository Pattern

### Generic base (SharedKernel)
```csharp
public interface IRepository<T, TId> where T : class
{
    Task<Result<T>> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(TId id, CancellationToken ct = default);  // soft-delete
}
```

### Module-specific extension
```csharp
public interface ICaseRepository : IRepository<Case, Guid>
{
    Task<Case?> GetWithCommentsAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<int> GetNextYearSequenceAsync(Guid tenantId, int year, CancellationToken ct = default);
}
```

**Read-side:** bypass repository, use `IReadDbConnection` (Dapper) directly in query handlers.

---

## Enum Contracts & JSON Serialization

All shared enums between frontend and backend must:
1. Use **PascalCase** field names in C# (e.g., `PendingApproval`)
2. Serialize to **SCREAMING_SNAKE_CASE** in JSON responses (e.g., `"PENDING_APPROVAL"`)
3. Be aligned across frontend/backend with same values

**Aligned Enums:**
- `CaseStatus` — DRAFT, SUBMITTED, PENDING_APPROVAL, APPROVED, CLOSED, REJECTED
- `FormFieldType` — TEXT, TEXTAREA, DROPDOWN, CHECKBOX, DATE, FILE, SIGNATURE, NUMBER
- `FormTemplateStatus` — DRAFT, PUBLISHED, ARCHIVED, RETIRED
- `ChatRole` — SYSTEM, USER, ASSISTANT

**JsonStringEnumConverter Configuration:**
Add to API `Program.cs` dependency injection:
```csharp
services
    .AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(new SnakeCaseNamingPolicy(), allowIntegerValues: false));
    });
```

This ensures all enum properties serialize to snake_case in OpenAPI/responses. Frontend orval SDK auto-generates matching TypeScript enums.

---

## Error Handling

### Error code format: `GOV_{MODULE}_{NNN}`
```
GOV_CASE_001   Case not found
GOV_CASE_002   Invalid state transition
GOV_IDN_001    User not found
GOV_IDN_002    Invalid credentials
GOV_FIL_001    File too large
GOV_FIL_002    Virus detected
GOV_LIMIT_001  Rate limit exceeded (authenticated)
GOV_LIMIT_002  Rate limit exceeded (anonymous)
```

### Rate Limiting (V2)
All API endpoints subject to per-tenant rate limits via `RateLimitMiddleware`:
- **Authenticated (200+ VUs):** 100 requests/minute
- **Anonymous (200+ IPs):** 20 requests/minute
- **Enforcement:** Redis-backed sliding window counter per tenant/IP
- **Response on limit:** 429 Too Many Requests with `Retry-After` header
- **Exception:** `/health/*` and `/metrics` endpoints exempt (internal monitoring)

**Implementation:**
```csharp
// In Program.cs
builder.Services.AddRateLimiting(options => {
    options.AddAuthenticatedPolicy(requestsPerMinute: 100);
    options.AddAnonymousPolicy(requestsPerMinute: 20);
});

// In middleware pipeline
app.UseRateLimiting();
```

**Frontend Handling:**
- Check `Retry-After` header on 429 response
- Display user message: "Too many requests. Please try again in X seconds."
- Implement exponential backoff for retry logic

### FluentResults + typed errors
```csharp
// Define typed errors in SharedKernel.Errors
public sealed class NotFoundError(string message) : Error(message);
public sealed class ForbiddenError(string message) : Error(message);
public sealed class ConflictError(string message) : Error(message);
public sealed class ValidationError(string message) : Error(message);
```

### Return from handler
```csharp
if (@case is null)
    return Result.Fail(new NotFoundError("GOV_CASE_001: Case not found."));

return Result.Ok(MapToDto(@case));
```

### ApiControllerBase maps to HTTP
```
Result.Ok(value)       → 200 OK      with ApiResponse<T>
NotFoundError          → 404
ForbiddenError         → 403
ConflictError          → 409
ValidationError        → 422
(unhandled)            → 500
```

---

## API Response Envelope

All endpoints return `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { ... },
  "meta": { "page": 1, "pageSize": 20, "total": 150 },
  "errors": []
}
```

Error response:
```json
{
  "success": false,
  "data": null,
  "errors": [
    { "code": "GOV_CASE_001", "message": "Case not found.", "detail_vi": "Hồ sơ không tồn tại." }
  ]
}
```

---

## Security Patterns

### Tenant & User Identity Resolution (CRITICAL)
All controllers MUST resolve tenant and user ID from JWT claims only — never accept from query parameters, request body, or other untrusted sources.

**ApiControllerBase provides two protected methods:**

1. `ResolveTenantId()` — Extracts tenant from JWT. SystemAdmin can override via `X-Tenant-Id` header (cross-tenant debugging).
2. `ResolveUserId()` — Extracts user ID from JWT.

**Correct pattern:**
```csharp
[HttpPost("{id}/approve")]
public async Task<IActionResult> ApproveCase(Guid id)
{
    var tenantId = ResolveTenantId();  // From JWT
    var userId = ResolveUserId();      // From JWT

    var command = new ApproveCaseCommand(id, tenantId, userId);
    var result = await Mediator.Send(command);
    return ToApiResponse(result);
}
```

**NEVER do this:**
```csharp
// ✗ WRONG: tenantId from query param
[HttpPost("{id}/approve")]
public async Task<IActionResult> ApproveCase(Guid id, [FromQuery] Guid tenantId)

// ✗ WRONG: createdBy from request body
public record ApproveCaseRequest(Guid Id, Guid CreatedBy, string Reason);

// ✗ WRONG: reviewedBy from form field
[HttpPost("batch-review")]
public async Task<IActionResult> BatchReview([FromBody] BatchReviewRequest req) // req.ReviewedBy ← WRONG
```

**Removing security fields from DTOs:** If a DTO contains tenantId, userId, createdBy, updatedBy, reviewedBy, or similar identity fields, remove them and derive from JWT instead.

### SQL Injection Prevention (Forms Module)
All user input used in dynamic SQL queries (especially in filter expressions) MUST be validated via regex patterns:

**Example from `ListFormSubmissionsQueryHandler`:**
```csharp
private static readonly Regex FieldKeyPattern = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

public async Task<Result<PagedList<FormSubmissionDto>>> Handle(
    ListFormSubmissionsQuery request,
    CancellationToken cancellationToken)
{
    // Validate field key pattern before using in SQL
    if (!FieldKeyPattern.IsMatch(request.Filter.FieldKey))
        return Result.Fail(new ValidationError("Invalid field key format"));

    var submissions = await _readDbConnection.QueryAsync<FormSubmissionDto>(
        @"SELECT * FROM FormSubmissions
          WHERE FieldKey = @fieldKey AND TenantId = @tenantId",
        new { fieldKey = request.Filter.FieldKey, tenantId = request.TenantId });

    return Result.Ok(new PagedList<FormSubmissionDto>(...));
}
```

### ExtController Tenant Isolation
ExtControllers (analytics, diff, bulk operations, PDF export) MUST enforce tenant isolation:
- Call `ResolveTenantId()` at method entry
- Never accept tenantId from query params
- Validate ownership before access

---

## Cross-Module Data Access (CRITICAL)

**NEVER** cross-schema JOIN between modules. Use `IModuleClient` interface instead.

### Why
- Cross-schema JOINs break when modules have separate databases (microservice extraction)
- No audit trail — impossible to trace which module accessed which data
- Tight coupling — schema migration in one module breaks queries in another

### Pattern
```csharp
// ✗ WRONG: Direct cross-schema JOIN
LEFT JOIN [identity].AspNetUsers u ON u.Id = r.DataSubjectId

// ✓ CORRECT: Use IModuleClient interface
var userMap = await identityClient.GetUsersByIdsAsync(userIds, ct);
// enrich DTOs in-memory
```

### IModuleClient Contracts
Defined in `SharedKernel.Contracts/Clients/`. Each module exposes an interface:
- `IIdentityModuleClient` — user/role lookups
- `IWorkflowModuleClient` — workflow dispatch
- `INotificationModuleClient` — send notifications
- etc.

**Monolith:** `InProcess*ModuleClient` (direct DB query, zero network overhead)
**Microservice:** Swap to `Http*ModuleClient` or `Grpc*ModuleClient` — zero consumer code change.

### Migration Path
1. Phase 1 (now): `IModuleClient` batch lookup — in-process Dapper query
2. Phase 2 (microservice): Swap implementation via DI — HTTP/gRPC call
3. Phase 3 (scale): Local Cache + Event Sync for high-frequency reference data

---

## Multi-tenancy

Every request-scoped service receives `ITenantContext` (injected via `TenantAwareBehavior`). EF Core global query filters enforce `org_id` isolation — never query cross-tenant without explicit admin scope.

```csharp
// In DbContext OnModelCreating:
builder.HasQueryFilter(c => c.TenantId == _tenantContext.TenantId);
```

---

## Concurrency

Use `RowVersion` on all aggregates → EF Core detects conflicts → return `ConflictError` with `ETag` header. Client must retry with `If-Match`.

---

## Testing Conventions

| Test type | Project suffix | Tools |
|---|---|---|
| Unit | `*.Tests` | xUnit, NSubstitute, FluentAssertions |
| Integration | `*.Integration.Tests` | Testcontainers, WebApplicationFactory |
| Architecture | `GSDT.Architecture.Tests` | NetArchTest.Rules |

### Global Test Setup
- **Directory.Build.props** (`tests/Directory.Build.props`): Global `using` statements across all test projects
  - Includes: `Xunit`, `FluentAssertions`, `NSubstitute`
  - Eliminates repetitive imports, ensures consistency

### Unit Test Pattern
- **No** database, **no** HTTP. Mock all external deps with NSubstitute.
- Test method naming: `{Method}_{Scenario}_{ExpectedOutcome}` e.g. `Submit_FromDraft_TransitionsToSubmitted`
- Use `FluentAssertions` for readable assertions: `result.Should().BeSuccess().And.Value.Should().Be(...)`

### Integration Test Pattern
- Real SQL Server via **Testcontainers** (isolated container per test run)
- Use `WebAppFixture` for `WebApplicationFactory<Program>` setup with:
  - `SqlServerFixture` — manages container lifecycle, applies migrations
  - `TestAuthHandler` — replaces JWT validation, allows auth as any principal
- Database is fresh per test class (not per method) — fast teardown via container disposal
- Example:
  ```csharp
  public sealed class CreateCaseIntegrationTests : IAsyncLifetime
  {
      private WebAppFixture _fixture;

      public async Task InitializeAsync() => _fixture = new WebAppFixture();
      public async Task DisposeAsync() => await _fixture.DisposeAsync();

      [Fact]
      public async Task CreateCase_WithValidCommand_PersistsAndReturnsDto()
      {
          // Arrange
          var client = _fixture.CreateClient();
          client.DefaultRequestHeaders.Authorization =
              new("Bearer", _fixture.GetAuthToken(userId: Guid.NewGuid()));

          // Act
          var response = await client.PostAsync("/api/v1/cases", ...);

          // Assert
          response.StatusCode.Should().Be(HttpStatusCode.Created);
      }
  }
  ```

### Architecture Test Pattern
- **NetArchTest.Rules** enforces Clean Architecture boundaries:
  - Domain layer has **no** references to Application, Infrastructure, Presentation
  - Application layer has **no** references to Infrastructure concrete classes (only interfaces)
  - Infrastructure does **not** reference Presentation controllers directly
  - Presentation references **only** Application (via MediatR)
- Run via `GSDT.Architecture.Tests` assembly
- Example rules:
  ```csharp
  [Fact]
  public void DomainLayer_ShouldNotReferenceApplicationLayer()
  {
      Types.InNamespace("GSDT.Identity.Domain")
          .Should()
          .NotHaveDependencyOn("GSDT.Identity.Application")
          .Because("Domain is core business logic, independent of application services");
  }
  ```

---

## k6 Performance Testing

### Directory Structure
```
tests/performance/
├── lib/
│   ├── auth.js           # Shared auth helpers (API Key scheme)
│   └── helpers.js        # Shared utility functions
├── load-test.js          # General steady-state load test
├── spike-test.js         # Sudden burst test
├── soak-test.js          # Long-running endurance test
└── {module}-load-test.js # Module-specific tests (audit, identity, files, notifications)
```

### API Key Auth Pattern

All k6 tests use X-Api-Key header scheme (not JWT). Import shared auth helpers:

```javascript
import { getJsonApiKeyHeaders, getApiKeyHeaders } from './lib/auth.js';

export default function() {
  const headers = getJsonApiKeyHeaders();  // includes Content-Type

  const response = http.post(
    `${__ENV.BASE_URL}/api/v1/cases`,
    JSON.stringify({ /* body */ }),
    { headers }
  );
}
```

Environment variables:
- `API_KEY` — test key plaintext (from CI secret K6_TEST_APIKEY)
- `BASE_URL` — API endpoint (default: http://localhost:5000)

### Test Type Naming & Thresholds

| Test | File | VUs | Duration | Target p95 | Max Error% | Status |
|------|------|-----|----------|-----------|-----------|--------|
| Load | `load-test.js` | 50 | 5 min | <500ms | 0.1% | ✓ Passing |
| Spike | `spike-test.js` | 100→500 | 2 min | <800ms | 1% | ✓ Passing |
| Soak | `soak-test.js` | 10 | 70 min | <600ms | 0% | ✓ Passing |
| Module | `{module}-load-test.js` | 20 | 3 min | <500ms | 0.1% | ✓ Passing (Audit, Identity, Files, Notifications) |
| E2E | Playwright | - | - | - | 0% | ✓ 13 tests (9 pass + 4 skip-Vite) |

**npm audit:** 0 vulnerabilities

All tests must pass before merge. Deviations require performance review.

---

## Security Testing Standards

### Test-Driven Security

All security findings must have corresponding test coverage:
- Unit tests for authentication/authorization handlers (NSubstitute mocks)
- Integration tests for token lifecycle (creation, refresh, revocation)
- Architecture tests for layer isolation (prevent privilege escalation via coupling)

### Security Test Categories

| Category | Examples | Framework |
|----------|----------|-----------|
| Authentication | Token generation, JWT validation, MFA TOTP | xUnit + NSubstitute |
| Authorization | RBAC role checks, ABAC attribute evaluation, delegation chaining | xUnit + mocks |
| PII Protection | RTBF anonymization, encryption key rotation, data breach notification | xUnit |
| Audit Logging | Immutability (HMAC chains), append-only enforcement, tamper detection | Integration tests |
| Rate Limiting | Brute force prevention (MFA lockout), API throttling | k6 load tests |
| SSRF Protection | URL whitelist validation, DNS rebinding prevention | xUnit |

### Security Audit Integration

- **Phases 1-4 Remediation:** 31/35 findings fixed (Groups A-D)
- **Test Evidence:** Each remediation has corresponding test in `tests/` directory
- **CI/CD Gate:** SAST/DAST (SonarQube, OWASP ZAP, Trivy) must pass before merge
- **Pentest Timeline:** Q2 2026 (Groups E-F findings pending)

### Examples

**Token Revocation (CRITICAL):**
```csharp
[Fact]
public async Task ChangePassword_RevokesExistingTokens()
{
    // Arrange
    var token = _fixture.GetAuthToken(userId: Guid.NewGuid());

    // Act
    await _handler.Handle(new ChangePasswordCommand(userId, oldPwd, newPwd), CancellationToken.None);

    // Assert
    var introspect = await _client.GetAsync($"/introspect?token={token}");
    introspect.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // revoked
}
```

**MFA Rate Limiting (CRITICAL):**
```csharp
[Fact]
public async Task SetupMfa_RateLimits_After5FailedAttempts()
{
    for (int i = 0; i < 5; i++)
        await _handler.Handle(new ValidateMfaCommand(userId, invalidOtp), ct);

    var result = await _handler.Handle(new ValidateMfaCommand(userId, correctOtp), ct);
    result.IsFailed.Should().BeTrue(); // locked out
}
```

**SSRF Protection (HIGH):**
```csharp
[Fact]
public void ExternalRef_RejectsUrlOutsideWhitelist()
{
    var validator = new ExternalRefUrlValidator(_whitelist);
    var action = () => validator.Validate("https://malicious.internal");
    action.Should().Throw<InvalidOperationException>();
}
```

---

## Git Commit Conventions

```
feat(cases): add bulk close endpoint
fix(identity): prevent concurrent MFA setup race condition
refactor(shared): extract WorkflowEngine to SharedKernel
test(cases): add cross-tenant isolation integration tests
chore(deps): update EF Core to 10.0.1
docs(adr): add ADR-006 for event sourcing decision
```

No AI references, no "Claude", no "as per your request" in commit messages.

---

## Asynchronous Report Execution Pattern

### Report Submission (HTTP 202 Accepted)
Report execution is asynchronous via Hangfire. Submission returns 202 with executionId:

```json
{
  "success": true,
  "data": { "executionId": "550e8400-e29b-41d4-a716-446655440000" },
  "errors": []
}
```

### Report Polling (Status Check)
Client polls `GET /api/v1/reports/executions/{id}` to check execution status:

```json
{
  "success": true,
  "data": {
    "executionId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "COMPLETED",
    "createdAt": "2026-03-18T10:00:00Z",
    "completedAt": "2026-03-18T10:01:30Z",
    "rowsGenerated": 5000,
    "format": "EXCEL"
  },
  "errors": []
}
```

### Report Download (File Retrieval)
Once status is COMPLETED, client downloads via `GET /api/v1/reports/executions/{id}/download` → binary file response.

**Security:** ResultFilePath validated against execution record before retrieval (path traversal prevention).

---

## SQL Template Validation (Reporting Module)

All report SQL templates validated via `SqlValidationHelper`:

**Rules:**
- Must start with `SELECT` (INSERT/UPDATE/DELETE blocked)
- Must include `@TenantId` parameter (multi-tenancy enforcement)
- Blocklist: EXEC, xp_*, sp_*, OPENQUERY, OPENDATASOURCE (expanded for security)
- Parameterized Dapper execution (no string interpolation)

**Example:**
```sql
-- VALID
SELECT Id, Title, CreatedAt FROM Cases WHERE TenantId = @TenantId

-- INVALID (will reject)
SELECT * FROM Users; DROP TABLE Cases;
INSERT INTO Cases ...
SELECT ... FROM Cases -- missing @TenantId filter
```

---

## Frontend Error Handling Patterns (V2)

**Error Boundary:** Wrap lazy routes with fallback. **Session Timeout:** Toast 2 min before expiry via AuthProvider. **Notifications:** Use Ant Design with detail_vi for CRUD operations. **Rate Limits:** Axios interceptor (429) shows retry countdown with exponential backoff.

---

## Frontend React Performance & Memoization

**React.memo:** Memoize child components with non-primitive props (objects, arrays, functions). Skip for primitive props or top-level pages. **useMemo:** Cache expensive computations (array filter/sort/reduce). **i18n:** Always use translation keys (web/src/core/i18n/locales/{vi,en}.json) — sync both languages.

---

## Dark Mode CSS Variables

Theme colors defined as CSS custom properties (Navy #1B3A5C, Light Navy #2C5AA0 for dark mode). Toggle via Zustand store with localStorage persistence.

---

## Health Check Dashboard

Admin endpoint `/admin/health` returns all infrastructure component status (API, DB, Redis, RabbitMQ, MinIO).

---

## Form Field Reorder

PATCH `/api/v1/form-templates/{templateId}/fields/{fieldId}/order` updates field display position.

---

## Batch Operations

POST `/api/v1/cases/batch/approve` allows bulk case approval/rejection with checkbox selection on cases table.

---

## Announcement Banner

Dashboard displays configurable announcement banners fetched from API (GET `/api/v1/announcements/active`). Dismissible with localStorage persistence.
