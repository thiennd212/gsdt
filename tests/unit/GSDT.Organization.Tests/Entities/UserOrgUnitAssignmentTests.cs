using GSDT.Organization.Entities;
using FluentAssertions;

namespace GSDT.Organization.Tests.Entities;

/// <summary>
/// Unit tests for UserOrgUnitAssignment entity.
/// TC-ORG-004: Create IsActive=true by default.
/// TC-ORG-005: Close deactivates and sets ValidTo.
/// </summary>
public sealed class UserOrgUnitAssignmentTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OrgUnitId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid TenantId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    // --- TC-ORG-004: Create IsActive=true by default ---

    [Fact]
    public void Create_SetsAllRequiredFields()
    {
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Member", TenantId);

        assignment.UserId.Should().Be(UserId);
        assignment.OrgUnitId.Should().Be(OrgUnitId);
        assignment.RoleInOrg.Should().Be("Member");
        assignment.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Create_IsActiveByDefault()
    {
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Lead", TenantId);

        assignment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ValidTo_IsNullByDefault()
    {
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Deputy", TenantId);

        assignment.ValidTo.Should().BeNull();
    }

    [Fact]
    public void Create_WithNoValidFrom_DefaultsToApproximatelyUtcNow()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Member", TenantId);

        assignment.ValidFrom.Should().BeAfter(before);
        assignment.ValidFrom.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Create_WithExplicitValidFrom_UsesProvidedDate()
    {
        var validFrom = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Lead", TenantId, validFrom);

        assignment.ValidFrom.Should().Be(validFrom);
    }

    [Fact]
    public void Create_GeneratesNonEmptyId()
    {
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Member", TenantId);

        assignment.Id.Should().NotBeEmpty();
    }

    // --- TC-ORG-005: Close deactivates ---

    [Fact]
    public void Close_SetsIsActiveFalse()
    {
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Member", TenantId);

        assignment.Close();

        assignment.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Close_SetsValidToApproximatelyUtcNow_WhenNoArgumentProvided()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Member", TenantId);

        assignment.Close();

        assignment.ValidTo.Should().NotBeNull();
        assignment.ValidTo.Should().BeAfter(before);
        assignment.ValidTo.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Close_WithExplicitDate_SetsValidToProvidedDate()
    {
        var closedAt = new DateTimeOffset(2026, 1, 31, 17, 0, 0, TimeSpan.Zero);
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Lead", TenantId);

        assignment.Close(closedAt);

        assignment.ValidTo.Should().Be(closedAt);
    }

    [Fact]
    public void Close_SetsUpdatedAt()
    {
        var assignment = UserOrgUnitAssignment.Create(UserId, OrgUnitId, "Member", TenantId);
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        assignment.Close();

        assignment.UpdatedAt.Should().NotBeNull();
        assignment.UpdatedAt.Should().BeAfter(before);
    }
}
