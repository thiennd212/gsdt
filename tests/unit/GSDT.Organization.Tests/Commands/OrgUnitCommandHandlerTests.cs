using GSDT.Organization.Commands;
using GSDT.Organization.Entities;
using GSDT.Organization.Persistence;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace GSDT.Organization.Tests.Commands;

/// <summary>
/// Unit tests for OrgUnit CRUD command handlers using EF InMemory.
/// TC-ORG-006: CreateOrgUnit conflict on duplicate code per tenant.
/// TC-ORG-007: CreateOrgUnit computes level from parent.
/// TC-ORG-008: CreateOrgUnit NotFound for missing parent.
/// TC-ORG-009: UpdateOrgUnit updates name and invalidates cache.
/// TC-ORG-010: UpdateOrgUnit NotFound for non-existent.
/// TC-ORG-011: DeleteOrgUnit blocks when active children exist.
/// TC-ORG-012: DeleteOrgUnit deactivates and sets successor.
/// TC-ORG-013: DeleteOrgUnit NotFound for wrong tenant.
/// </summary>
public sealed class OrgUnitCommandHandlerTests : IDisposable
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly OrgDbContext _db;
    private readonly OrgUnitService _orgService;

    public OrgUnitCommandHandlerTests()
    {
        // ITenantContext: IsSystemAdmin=true bypasses tenant global filter so all data visible
        var tenantCtx = Substitute.For<ITenantContext>();
        tenantCtx.TenantId.Returns((Guid?)TenantId);
        tenantCtx.IsSystemAdmin.Returns(true);

        var options = new DbContextOptionsBuilder<OrgDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new OrgDbContext(options, tenantCtx);

        var cache = Substitute.For<GSDT.SharedKernel.Application.Caching.ICacheService>();
        var readDb = Substitute.For<GSDT.SharedKernel.Application.Data.IReadDbConnection>();
        _orgService = new OrgUnitService(_db, readDb, cache);
    }

    public void Dispose() => _db.Dispose();

    // --- TC-ORG-006: Conflict on duplicate code per tenant ---

    [Fact]
    public async Task CreateOrgUnit_DuplicateCodeSameTenant_ReturnsConflictError()
    {
        _db.OrgUnits.Add(OrgUnit.Create("Existing", "Existing", "MOJ", TenantId));
        await _db.SaveChangesAsync();

        var handler = new CreateOrgUnitCommandHandler(_db, _orgService);
        var cmd = new CreateOrgUnitCommand(TenantId, "New Ministry", "New Ministry EN", "MOJ", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("MOJ"));
    }

    [Fact]
    public async Task CreateOrgUnit_SameCodeDifferentTenant_Succeeds()
    {
        _db.OrgUnits.Add(OrgUnit.Create("Existing", "Existing", "MOJ", OtherTenantId));
        await _db.SaveChangesAsync();

        var handler = new CreateOrgUnitCommandHandler(_db, _orgService);
        var cmd = new CreateOrgUnitCommand(TenantId, "My Ministry", "My Ministry EN", "MOJ", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // --- TC-ORG-007: Computes level from parent ---

    [Fact]
    public async Task CreateOrgUnit_WithParent_ComputesLevelAsParentLevelPlusOne()
    {
        var parent = OrgUnit.Create("Ministry", "Ministry", "MIN", TenantId, null, level: 1);
        _db.OrgUnits.Add(parent);
        await _db.SaveChangesAsync();

        var handler = new CreateOrgUnitCommandHandler(_db, _orgService);
        var cmd = new CreateOrgUnitCommand(TenantId, "Department", "Department EN", "DEPT-01", parent.Id);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(2);
        result.Value.ParentId.Should().Be(parent.Id);
    }

    [Fact]
    public async Task CreateOrgUnit_WithoutParent_DefaultsToLevelOne()
    {
        var handler = new CreateOrgUnitCommandHandler(_db, _orgService);
        var cmd = new CreateOrgUnitCommand(TenantId, "Root Ministry", "Root Ministry EN", "ROOT", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(1);
    }

    // --- TC-ORG-008: NotFound for missing parent ---

    [Fact]
    public async Task CreateOrgUnit_MissingParentId_ReturnsNotFoundError()
    {
        var handler = new CreateOrgUnitCommandHandler(_db, _orgService);
        var missingParentId = Guid.NewGuid();
        var cmd = new CreateOrgUnitCommand(TenantId, "Division", "Division EN", "DIV-01", missingParentId);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(missingParentId.ToString()));
    }

    // --- TC-ORG-009: UpdateOrgUnit updates name ---

    [Fact]
    public async Task UpdateOrgUnit_ExistingUnit_UpdatesNameAndNameEn()
    {
        var unit = OrgUnit.Create("Old Name", "Old EN", "CODE-01", TenantId);
        _db.OrgUnits.Add(unit);
        await _db.SaveChangesAsync();

        var handler = new UpdateOrgUnitCommandHandler(_db, _orgService);
        var cmd = new UpdateOrgUnitCommand(unit.Id, TenantId, "New Name", "New EN");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.NameEn.Should().Be("New EN");
    }

    [Fact]
    public async Task UpdateOrgUnit_ExistingUnit_InvalidatesCacheOnce()
    {
        var cache = Substitute.For<GSDT.SharedKernel.Application.Caching.ICacheService>();
        var readDb = Substitute.For<GSDT.SharedKernel.Application.Data.IReadDbConnection>();
        var svc = new OrgUnitService(_db, readDb, cache);

        var unit = OrgUnit.Create("Original", "Original EN", "ORIG", TenantId);
        _db.OrgUnits.Add(unit);
        await _db.SaveChangesAsync();

        var handler = new UpdateOrgUnitCommandHandler(_db, svc);
        var cmd = new UpdateOrgUnitCommand(unit.Id, TenantId, "Updated", "Updated EN");

        await handler.Handle(cmd, CancellationToken.None);

        await cache.Received(1).RemoveAsync(
            Arg.Is<string>(k => k.Contains(TenantId.ToString())),
            Arg.Any<CancellationToken>());
    }

    // --- TC-ORG-010: UpdateOrgUnit NotFound ---

    [Fact]
    public async Task UpdateOrgUnit_NonExistentId_ReturnsNotFoundError()
    {
        var handler = new UpdateOrgUnitCommandHandler(_db, _orgService);
        var cmd = new UpdateOrgUnitCommand(Guid.NewGuid(), TenantId, "Name", "Name EN");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }

    // --- TC-ORG-011: DeleteOrgUnit blocks when active children ---

    [Fact]
    public async Task DeleteOrgUnit_WithActiveChildren_ReturnsValidationError()
    {
        var parent = OrgUnit.Create("Parent", "Parent EN", "PAR", TenantId, null, 1);
        var child = OrgUnit.Create("Child", "Child EN", "CHI", TenantId, parent.Id, 2);
        _db.OrgUnits.AddRange(parent, child);
        await _db.SaveChangesAsync();

        var handler = new DeleteOrgUnitCommandHandler(_db, _orgService);
        var cmd = new DeleteOrgUnitCommand(parent.Id, TenantId, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("active children"));
    }

    // --- TC-ORG-012: DeleteOrgUnit deactivates and sets successor ---

    [Fact]
    public async Task DeleteOrgUnit_NoChildren_DeactivatesUnit()
    {
        var unit = OrgUnit.Create("Leaf Unit", "Leaf EN", "LEAF", TenantId);
        _db.OrgUnits.Add(unit);
        await _db.SaveChangesAsync();

        var handler = new DeleteOrgUnitCommandHandler(_db, _orgService);
        var cmd = new DeleteOrgUnitCommand(unit.Id, TenantId, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.OrgUnits.IgnoreQueryFilters().FirstAsync(x => x.Id == unit.Id);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteOrgUnit_WithSuccessorId_SetsSuccessorBeforeDeactivating()
    {
        var unit = OrgUnit.Create("Old Unit", "Old EN", "OLD", TenantId);
        var successor = OrgUnit.Create("Successor", "Successor EN", "NEW", TenantId);
        _db.OrgUnits.AddRange(unit, successor);
        await _db.SaveChangesAsync();

        var handler = new DeleteOrgUnitCommandHandler(_db, _orgService);
        var cmd = new DeleteOrgUnitCommand(unit.Id, TenantId, successor.Id);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.OrgUnits.IgnoreQueryFilters().FirstAsync(x => x.Id == unit.Id);
        updated.SuccessorId.Should().Be(successor.Id);
        updated.IsActive.Should().BeFalse();
    }

    // --- TC-ORG-013: DeleteOrgUnit NotFound for wrong tenant ---

    [Fact]
    public async Task DeleteOrgUnit_WrongTenant_ReturnsNotFoundError()
    {
        var unit = OrgUnit.Create("Other Tenant Unit", "Other EN", "OTHER", OtherTenantId);
        _db.OrgUnits.Add(unit);
        await _db.SaveChangesAsync();

        var handler = new DeleteOrgUnitCommandHandler(_db, _orgService);
        // Request with TenantId but unit belongs to OtherTenantId
        var cmd = new DeleteOrgUnitCommand(unit.Id, TenantId, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }
}
