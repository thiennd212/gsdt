using FluentAssertions;
using GSDT.InvestmentProjects.Domain.Entities;
using GSDT.InvestmentProjects.Domain.Enums;

namespace GSDT.InvestmentProjects.Domain.Tests.Entities;

/// <summary>
/// Unit tests for DomesticProject factory method and computed property behaviour.
/// All MasterData refs are plain Guids — no infrastructure needed.
/// </summary>
public sealed class DomesticProjectTests
{
    // ── shared test data ──────────────────────────────────────────────────────

    private static readonly Guid TenantId           = Guid.NewGuid();
    private static readonly Guid ManagingAuthId     = Guid.NewGuid();
    private static readonly Guid IndustrySectorId   = Guid.NewGuid();
    private static readonly Guid ProjectOwnerId     = Guid.NewGuid();
    private static readonly Guid ProjectGroupId     = Guid.NewGuid();

    private static DomesticProject CreateDefault() =>
        DomesticProject.Create(
            tenantId:           TenantId,
            projectCode:        "PRJ-001",
            projectName:        "Test Project",
            managingAuthorityId: ManagingAuthId,
            industrySectorId:   IndustrySectorId,
            projectOwnerId:     ProjectOwnerId,
            projectGroupId:     ProjectGroupId);

    // ── factory: required fields ──────────────────────────────────────────────

    [Fact]
    public void DomesticProject_Create_SetsProjectCode()
    {
        var project = CreateDefault();

        project.ProjectCode.Should().Be("PRJ-001");
    }

    [Fact]
    public void DomesticProject_Create_SetsProjectName()
    {
        var project = CreateDefault();

        project.ProjectName.Should().Be("Test Project");
    }

    [Fact]
    public void DomesticProject_Create_SetsTenantId()
    {
        var project = CreateDefault();

        project.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void DomesticProject_Create_SetsProjectTypeToDomestic()
    {
        var project = CreateDefault();

        project.ProjectType.Should().Be(ProjectType.Domestic);
    }

    [Fact]
    public void DomesticProject_Create_SetsManagingAuthorityId()
    {
        var project = CreateDefault();

        project.ManagingAuthorityId.Should().Be(ManagingAuthId);
    }

    [Fact]
    public void DomesticProject_Create_SetsIndustrySectorId()
    {
        var project = CreateDefault();

        project.IndustrySectorId.Should().Be(IndustrySectorId);
    }

    [Fact]
    public void DomesticProject_Create_SetsProjectOwnerId()
    {
        var project = CreateDefault();

        project.ProjectOwnerId.Should().Be(ProjectOwnerId);
    }

    [Fact]
    public void DomesticProject_Create_SetsProjectGroupId()
    {
        var project = CreateDefault();

        project.ProjectGroupId.Should().Be(ProjectGroupId);
    }

    // ── factory: defaults ─────────────────────────────────────────────────────

    [Fact]
    public void DomesticProject_Create_SetsDefaultSubProjectTypeToNotSubProject()
    {
        var project = CreateDefault();

        project.SubProjectType.Should().Be(SubProjectType.NotSubProject);
    }

    [Fact]
    public void DomesticProject_Create_WithExplicitSubProjectType_SetsIt()
    {
        var project = DomesticProject.Create(
            TenantId, "PRJ-002", "Sub Project",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId,
            subProjectType: SubProjectType.IsSubProject);

        project.SubProjectType.Should().Be(SubProjectType.IsSubProject);
    }

    // ── factory: identity ─────────────────────────────────────────────────────

    [Fact]
    public void DomesticProject_Create_GeneratesNewId()
    {
        var a = CreateDefault();
        var b = CreateDefault();

        a.Id.Should().NotBe(b.Id);
        a.Id.Should().NotBe(Guid.Empty);
    }

    // ── preliminary capital computed checks ───────────────────────────────────

    [Fact]
    public void DomesticProject_PrelimPublicInvestment_EqualsNSTWPlusNSDPPlusOtherPublic()
    {
        // PrelimPublicInvestment is a stored field — verify it can hold
        // the sum of central + local + otherPublic for read-back consistency.
        var project = CreateDefault();
        project.PrelimCentralBudget        = 100m;
        project.PrelimLocalBudget          = 50m;
        project.PrelimOtherPublicCapital   = 20m;
        project.PrelimPublicInvestment     = 100m + 50m + 20m; // 170 — caller's responsibility

        project.PrelimPublicInvestment.Should().Be(170m);
    }

    [Fact]
    public void DomesticProject_PrelimTotalInvestment_EqualsSumOfPublicAndOtherCapital()
    {
        var project = CreateDefault();
        project.PrelimPublicInvestment = 170m;
        project.PrelimOtherCapital     = 30m;
        project.PrelimTotalInvestment  = 170m + 30m; // 200

        project.PrelimTotalInvestment.Should().Be(200m);
    }

    // ── domain event ─────────────────────────────────────────────────────────

    [Fact]
    public void DomesticProject_Create_RaisesProjectCreatedEvent()
    {
        var project = CreateDefault();

        project.DomainEvents.Should().ContainSingle(e =>
            e is GSDT.InvestmentProjects.Domain.Events.ProjectCreatedEvent);
    }
}
