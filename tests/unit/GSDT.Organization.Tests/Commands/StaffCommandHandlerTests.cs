using GSDT.Organization.Commands;
using GSDT.Organization.Entities;
using GSDT.Organization.Persistence;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace GSDT.Organization.Tests.Commands;

/// <summary>
/// Unit tests for staff assignment command handlers using EF InMemory.
/// TC-ORG-017: AssignStaff creates assignment + position history.
/// TC-ORG-018: TransferStaff closes old assignment/history, creates new ones.
/// </summary>
public sealed class StaffCommandHandlerTests : IDisposable
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId   = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly OrgDbContext _db;

    public StaffCommandHandlerTests()
    {
        var tenantCtx = Substitute.For<ITenantContext>();
        tenantCtx.TenantId.Returns((Guid?)TenantId);
        tenantCtx.IsSystemAdmin.Returns(true);

        var options = new DbContextOptionsBuilder<OrgDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new OrgDbContext(options, tenantCtx);
    }

    public void Dispose() => _db.Dispose();

    // Helper: seed an active org unit and return its Id
    private async Task<Guid> SeedActiveOrgUnitAsync(string code = "UNIT-01")
    {
        var unit = OrgUnit.Create("Unit", "Unit EN", code, TenantId);
        _db.OrgUnits.Add(unit);
        await _db.SaveChangesAsync();
        return unit.Id;
    }

    // --- TC-ORG-017: AssignStaff creates assignment + position history ---

    [Fact]
    public async Task AssignStaff_ValidOrgUnit_CreatesActiveAssignment()
    {
        var orgUnitId = await SeedActiveOrgUnitAsync();
        var handler = new AssignStaffCommandHandler(_db);
        var cmd = new AssignStaffCommand(TenantId, UserId, orgUnitId, "Member", "Officer II");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(UserId);
        result.Value.OrgUnitId.Should().Be(orgUnitId);
        result.Value.RoleInOrg.Should().Be("Member");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AssignStaff_ValidOrgUnit_PersistsAssignmentToDb()
    {
        var orgUnitId = await SeedActiveOrgUnitAsync();
        var handler = new AssignStaffCommandHandler(_db);
        var cmd = new AssignStaffCommand(TenantId, UserId, orgUnitId, "Lead", "Division Lead");

        await handler.Handle(cmd, CancellationToken.None);

        var assignment = await _db.Assignments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.UserId == UserId && a.TenantId == TenantId);
        assignment.Should().NotBeNull();
        assignment!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AssignStaff_ValidOrgUnit_PersistsPositionHistoryWithOpenEndDate()
    {
        var orgUnitId = await SeedActiveOrgUnitAsync();
        var handler = new AssignStaffCommandHandler(_db);
        var cmd = new AssignStaffCommand(TenantId, UserId, orgUnitId, "Member", "Senior Inspector");

        await handler.Handle(cmd, CancellationToken.None);

        var history = await _db.PositionHistories.IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.UserId == UserId && h.TenantId == TenantId);
        history.Should().NotBeNull();
        history!.PositionTitle.Should().Be("Senior Inspector");
        history.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task AssignStaff_InactiveOrgUnit_ReturnsNotFoundError()
    {
        // Seed an inactive unit by deactivating it after creation
        var unit = OrgUnit.Create("Inactive", "Inactive EN", "INACT", TenantId);
        _db.OrgUnits.Add(unit);
        await _db.SaveChangesAsync();
        unit.Deactivate();
        await _db.SaveChangesAsync();

        var handler = new AssignStaffCommandHandler(_db);
        var cmd = new AssignStaffCommand(TenantId, UserId, unit.Id, "Member", "Officer");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task AssignStaff_NonExistentOrgUnit_ReturnsNotFoundError()
    {
        var handler = new AssignStaffCommandHandler(_db);
        var cmd = new AssignStaffCommand(TenantId, UserId, Guid.NewGuid(), "Member", "Officer");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    // --- TC-ORG-018: TransferStaff closes old, creates new ---

    [Fact]
    public async Task TransferStaff_ClosesExistingAssignmentAndCreatesNew()
    {
        var fromUnitId = await SeedActiveOrgUnitAsync("FROM");
        var toUnitId   = await SeedActiveOrgUnitAsync("TO");

        // First assign the user to fromUnit
        var assignHandler = new AssignStaffCommandHandler(_db);
        await assignHandler.Handle(
            new AssignStaffCommand(TenantId, UserId, fromUnitId, "Member", "Officer"),
            CancellationToken.None);

        // Now transfer to toUnit
        var transferHandler = new TransferStaffCommandHandler(_db);
        var cmd = new TransferStaffCommand(TenantId, UserId, toUnitId, "Lead", "Team Lead");

        var result = await transferHandler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrgUnitId.Should().Be(toUnitId);
        result.Value.RoleInOrg.Should().Be("Lead");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task TransferStaff_ClosesOldAssignment()
    {
        var fromUnitId = await SeedActiveOrgUnitAsync("FROM2");
        var toUnitId   = await SeedActiveOrgUnitAsync("TO2");

        var assignHandler = new AssignStaffCommandHandler(_db);
        await assignHandler.Handle(
            new AssignStaffCommand(TenantId, UserId, fromUnitId, "Member", "Officer"),
            CancellationToken.None);

        var transferHandler = new TransferStaffCommandHandler(_db);
        await transferHandler.Handle(
            new TransferStaffCommand(TenantId, UserId, toUnitId, "Lead", "Team Lead"),
            CancellationToken.None);

        // Old assignment should be closed
        var oldAssignment = await _db.Assignments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.OrgUnitId == fromUnitId && a.UserId == UserId);
        oldAssignment.Should().NotBeNull();
        oldAssignment!.IsActive.Should().BeFalse();
        oldAssignment.ValidTo.Should().NotBeNull();
    }

    [Fact]
    public async Task TransferStaff_ClosesOpenPositionHistory()
    {
        var fromUnitId = await SeedActiveOrgUnitAsync("FROM3");
        var toUnitId   = await SeedActiveOrgUnitAsync("TO3");

        var assignHandler = new AssignStaffCommandHandler(_db);
        await assignHandler.Handle(
            new AssignStaffCommand(TenantId, UserId, fromUnitId, "Member", "Inspector"),
            CancellationToken.None);

        var transferHandler = new TransferStaffCommandHandler(_db);
        await transferHandler.Handle(
            new TransferStaffCommand(TenantId, UserId, toUnitId, "Lead", "Senior Inspector"),
            CancellationToken.None);

        // Old history should be closed
        var oldHistory = await _db.PositionHistories.IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.OrgUnitId == fromUnitId && h.UserId == UserId);
        oldHistory.Should().NotBeNull();
        oldHistory!.EndDate.Should().NotBeNull();
    }

    [Fact]
    public async Task TransferStaff_CreatesNewPositionHistory()
    {
        var fromUnitId = await SeedActiveOrgUnitAsync("FROM4");
        var toUnitId   = await SeedActiveOrgUnitAsync("TO4");

        var assignHandler = new AssignStaffCommandHandler(_db);
        await assignHandler.Handle(
            new AssignStaffCommand(TenantId, UserId, fromUnitId, "Member", "Officer"),
            CancellationToken.None);

        var transferHandler = new TransferStaffCommandHandler(_db);
        await transferHandler.Handle(
            new TransferStaffCommand(TenantId, UserId, toUnitId, "Lead", "Division Head"),
            CancellationToken.None);

        // New open history for toUnit
        var newHistory = await _db.PositionHistories.IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.OrgUnitId == toUnitId && h.UserId == UserId);
        newHistory.Should().NotBeNull();
        newHistory!.PositionTitle.Should().Be("Division Head");
        newHistory.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task TransferStaff_TargetOrgUnitNotFound_ReturnsNotFoundError()
    {
        var transferHandler = new TransferStaffCommandHandler(_db);
        var cmd = new TransferStaffCommand(TenantId, UserId, Guid.NewGuid(), "Lead", "Head");

        var result = await transferHandler.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not found"));
    }
}
