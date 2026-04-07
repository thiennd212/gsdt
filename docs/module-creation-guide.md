# How to Create a New Module

This guide walks through creating a new module following the Cases module as the golden path example. All steps assume the module is named `Permits`.

---

## Quick Start: Use Module Template

**V1.5+:** Use the `dotnet new` template for fastest scaffolding:

```bash
# Install template (one-time)
dotnet new install ./templates/aqt-module/

# Create new module
dotnet new aqt-module --name Permits --output src/modules/permits

# Result: Complete project structure with DI registration, sample entity, test stub
```

Template generates all files below. Proceed to **Step 2 (Create Domain Entity)** to customize.

---

## Manual Setup: Create Project Structure

For customized setup or if template doesn't fit your use case:

```
src/modules/permits/
├── GSDT.Permits.Domain/
│   ├── GSDT.Permits.Domain.csproj
│   ├── Entities/
│   ├── Events/
│   ├── Exceptions/
│   └── Repositories/
├── GSDT.Permits.Application/
│   ├── GSDT.Permits.Application.csproj
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   └── ModuleRegistration.cs
├── GSDT.Permits.Infrastructure/
│   ├── GSDT.Permits.Infrastructure.csproj
│   ├── Persistence/
│   │   ├── PermitsDbContext.cs
│   │   └── Repositories/
│   └── ModuleRegistration.cs
└── GSDT.Permits.Presentation/
    ├── GSDT.Permits.Presentation.csproj
    └── Controllers/
```

### Domain .csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../../shared/GSDT.SharedKernel/GSDT.SharedKernel.csproj" />
    <PackageReference Include="FluentResults" />
  </ItemGroup>
</Project>
```

### Application .csproj
```xml
<ItemGroup>
  <ProjectReference Include=".../GSDT.Permits.Domain.csproj" />
  <PackageReference Include="MediatR" />
  <PackageReference Include="FluentValidation" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
</ItemGroup>
```

---

## 2. Create Domain Entity

```csharp
// src/modules/permits/GSDT.Permits.Domain/Entities/Permit.cs
namespace GSDT.Permits.Domain.Entities;

public sealed class Permit : AuditableEntity<Guid>, IAggregateRoot, ITenantScoped
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid TenantId { get; private set; }
    public string PermitNumber { get; private set; } = string.Empty;
    public string ApplicantName { get; private set; } = string.Empty;
    public PermitStatus Status { get; private set; } = PermitStatus.Pending;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Permit() { }  // EF Core

    public static Permit Create(
        Guid tenantId,
        string applicantName,
        Guid createdBy)
    {
        var permit = new Permit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicantName = applicantName,
            Status = PermitStatus.Pending,
        };
        permit.SetAuditCreate(createdBy);
        permit.AddDomainEvent(new PermitCreatedEvent(permit.Id, tenantId, createdBy));
        return permit;
    }

    public void AddDomainEvent(IDomainEvent e) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

---

## 3. Define IRepository in Domain

```csharp
// src/modules/permits/GSDT.Permits.Domain/Repositories/IPermitRepository.cs
namespace GSDT.Permits.Domain.Repositories;

public interface IPermitRepository : IRepository<Permit, Guid>
{
    Task<Permit?> GetByPermitNumberAsync(
        string permitNumber,
        Guid tenantId,
        CancellationToken ct = default);
}
```

---

## 4. Create CQRS Command + Handler

```csharp
// Commands/CreatePermit/CreatePermitCommand.cs
public sealed record CreatePermitCommand(
    Guid TenantId,
    string ApplicantName) : ICommand<PermitDto>;

// Commands/CreatePermit/CreatePermitCommandHandler.cs
public sealed class CreatePermitCommandHandler(
    IPermitRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreatePermitCommand, Result<PermitDto>>
{
    public async Task<Result<PermitDto>> Handle(
        CreatePermitCommand request,
        CancellationToken cancellationToken)
    {
        var permit = Permit.Create(
            request.TenantId,
            request.ApplicantName,
            currentUser.UserId);

        await repository.AddAsync(permit, cancellationToken);
        return Result.Ok(MapToDto(permit));
    }

    private static PermitDto MapToDto(Permit p) =>
        new(p.Id, p.TenantId, p.PermitNumber, p.ApplicantName, p.Status.ToString());
}

// Commands/CreatePermit/CreatePermitCommandValidator.cs
public sealed class CreatePermitCommandValidator : AbstractValidator<CreatePermitCommand>
{
    public CreatePermitCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ApplicantName)
            .NotEmpty().WithMessage("Tên người nộp đơn không được để trống.")
            .MaximumLength(200);
    }
}
```

---

## 5. Register Module with DI

### Application layer
```csharp
// GSDT.Permits.Application/ModuleRegistration.cs
public static class ModuleRegistration
{
    public static IServiceCollection AddPermitsModule(this IServiceCollection services)
    {
        // MediatR picks up handlers + validators from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(ModuleRegistration).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
```

### Infrastructure layer
```csharp
// GSDT.Permits.Infrastructure/ModuleRegistration.cs
public static class ModuleRegistration
{
    public static IServiceCollection AddPermitsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PermitsDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IPermitRepository, PermitRepository>();
        return services;
    }
}
```

---

## 6. Create Controller

```csharp
// GSDT.Permits.Presentation/Controllers/PermitsController.cs
[Route("api/v1/permits")]
[Authorize]
public sealed class PermitsController(ISender mediator) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePermitCommand command,
        CancellationToken ct) =>
        ToApiResponse(await mediator.Send(command, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid tenantId,
        CancellationToken ct) =>
        ToApiResponse(await mediator.Send(new GetPermitByIdQuery(id, tenantId), ct));
}
```

---

## 7. Register in Host Program.cs

```csharp
// src/host/GSDT.Api/Program.cs

// Add controller assembly
builder.Services.AddControllers()
    .AddApplicationPart(
        typeof(GSDT.Permits.Presentation.Controllers.PermitsController).Assembly);

// Register module services
builder.Services
    .AddPermitsModule()
    .AddPermitsInfrastructure(builder.Configuration);
```

---

## 8. Add EF Core Migration

```bash
# From repo root — targets Infrastructure project, uses Api as startup
dotnet ef migrations add InitialPermits \
  --project src/modules/permits/GSDT.Permits.Infrastructure \
  --startup-project src/host/GSDT.Api \
  --context PermitsDbContext
```

---

## 9. Add Tests

Create test projects following the pattern:
```
tests/unit/GSDT.Permits.Domain.Tests/
tests/unit/GSDT.Permits.Application.Tests/
tests/integration/GSDT.Permits.Integration.Tests/
```

Add the new assemblies to `AssemblyFixtures.cs` in the architecture tests project and write an isolation rule for the new module.

---

## 10. Use Feature Flags (V1.5+)

Gate operational behavior with feature flags without code deployment:

```csharp
// Inject IFeatureFlagService (from SharedKernel.Contracts)
public sealed class CreatePermitCommandHandler(
    IPermitRepository repository,
    IFeatureFlagService featureFlags,
    ICurrentUser currentUser)
    : IRequestHandler<CreatePermitCommand, Result<PermitDto>>
{
    public async Task<Result<PermitDto>> Handle(
        CreatePermitCommand request,
        CancellationToken cancellationToken)
    {
        var permit = Permit.Create(
            request.TenantId,
            request.ApplicantName,
            currentUser.UserId);

        // Example: conditionally enable validation via flag
        if (await featureFlags.IsEnabledAsync(
            "permits.strict-validation.enabled",
            cancellationToken))
        {
            permit.ValidateStrict();
        }

        await repository.AddAsync(permit, cancellationToken);
        return Result.Ok(MapToDto(permit));
    }
}
```

**Flag management:**
- Defined in `Infrastructure/FeatureFlags/SeedFlags.cs`
- Toggled in admin UI (when available) or direct DB updates
- Cached in Redis (5 min TTL) — instant effect across instances
- Convention: `{module}.{feature}.{property}` (e.g., `permits.validation.strict-mode`)

---

## Checklist

- [ ] Domain entity extends `AuditableEntity<Guid>`, implements `IAggregateRoot`, `ITenantScoped`
- [ ] Factory method (`Create(...)`) — no `new` from outside Domain
- [ ] `IPermitRepository` defined in Domain, implemented in Infrastructure
- [ ] Command/Query records are immutable (`sealed record`)
- [ ] Handler returns `Result<T>` — never throws for business errors
- [ ] FluentValidation with Vietnamese error messages
- [ ] Controller inherits `ApiControllerBase`, uses `ToApiResponse()`
- [ ] Module registered in host `Program.cs`
- [ ] Architecture tests updated with new assembly references
- [ ] Unit tests cover: entity creation, state transitions, handler happy path + failure
- [ ] Use feature flags for operational toggles (not code-level conditionals)
