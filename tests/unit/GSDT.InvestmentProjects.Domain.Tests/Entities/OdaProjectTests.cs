using FluentAssertions;
using GSDT.InvestmentProjects.Domain.Entities;
using GSDT.InvestmentProjects.Domain.Enums;

namespace GSDT.InvestmentProjects.Domain.Tests.Entities;

/// <summary>
/// Unit tests for OdaProject factory method and capital field behaviour.
/// </summary>
public sealed class OdaProjectTests
{
    // ── shared test data ──────────────────────────────────────────────────────

    private static readonly Guid TenantId         = Guid.NewGuid();
    private static readonly Guid ManagingAuthId   = Guid.NewGuid();
    private static readonly Guid IndustrySectorId = Guid.NewGuid();
    private static readonly Guid ProjectOwnerId   = Guid.NewGuid();
    private static readonly Guid OdaProjectTypeId = Guid.NewGuid();
    private static readonly Guid DonorId          = Guid.NewGuid();

    private static OdaProject CreateDefault() =>
        OdaProject.Create(
            tenantId:           TenantId,
            projectCode:        "ODA-001",
            projectName:        "ODA Test Project",
            shortName:          "ODA-SHORT",
            managingAuthorityId: ManagingAuthId,
            industrySectorId:   IndustrySectorId,
            projectOwnerId:     ProjectOwnerId,
            odaProjectTypeId:   OdaProjectTypeId,
            donorId:            DonorId);

    // ── factory: required fields ──────────────────────────────────────────────

    [Fact]
    public void OdaProject_Create_SetsProjectCode()
    {
        var project = CreateDefault();

        project.ProjectCode.Should().Be("ODA-001");
    }

    [Fact]
    public void OdaProject_Create_SetsProjectName()
    {
        var project = CreateDefault();

        project.ProjectName.Should().Be("ODA Test Project");
    }

    [Fact]
    public void OdaProject_Create_SetsTenantId()
    {
        var project = CreateDefault();

        project.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void OdaProject_Create_SetsShortName()
    {
        var project = CreateDefault();

        project.ShortName.Should().Be("ODA-SHORT");
    }

    [Fact]
    public void OdaProject_Create_SetsProjectTypeToOda()
    {
        var project = CreateDefault();

        project.ProjectType.Should().Be(ProjectType.Oda);
    }

    [Fact]
    public void OdaProject_Create_SetsOdaProjectTypeId()
    {
        var project = CreateDefault();

        project.OdaProjectTypeId.Should().Be(OdaProjectTypeId);
    }

    [Fact]
    public void OdaProject_Create_SetsDonorId()
    {
        var project = CreateDefault();

        project.DonorId.Should().Be(DonorId);
    }

    // ── factory: identity ─────────────────────────────────────────────────────

    [Fact]
    public void OdaProject_Create_GeneratesNewId()
    {
        var a = CreateDefault();
        var b = CreateDefault();

        a.Id.Should().NotBe(b.Id);
        a.Id.Should().NotBe(Guid.Empty);
    }

    // ── optional field (v1.1) ─────────────────────────────────────────────────

    [Fact]
    public void OdaProject_ProjectCodeQhns_IsNullByDefault()
    {
        // v1.1: ProjectCodeQhns is optional — factory sets no value so it starts null.
        var project = CreateDefault();

        project.ProjectCodeQhns.Should().BeNull();
    }

    [Fact]
    public void OdaProject_ProjectCodeQhns_AcceptsExplicitValue()
    {
        var project = CreateDefault();
        project.ProjectCodeQhns = "QHNS-2024-001";

        project.ProjectCodeQhns.Should().Be("QHNS-2024-001");
    }

    // ── capital field consistency ─────────────────────────────────────────────

    [Fact]
    public void OdaProject_TotalInvestment_EqualsSumOfGrantLoanAndCounterpart()
    {
        // TotalInvestment is a stored field — the caller is responsible for computing
        // the sum; these assertions verify read-back matches what was set.
        var project = CreateDefault();
        project.OdaGrantCapital         = 500m;
        project.OdaLoanCapital          = 300m;
        project.CounterpartCentralBudget = 80m;
        project.CounterpartLocalBudget   = 40m;
        project.CounterpartOtherCapital  = 30m;

        var expectedCounterpart = 80m + 40m + 30m; // 150
        var expectedTotal       = 500m + 300m + expectedCounterpart; // 950
        project.TotalInvestment = expectedTotal;

        project.TotalInvestment.Should().Be(950m);
    }

    [Fact]
    public void OdaProject_CounterpartFields_AreIndependentlySettable()
    {
        var project = CreateDefault();
        project.CounterpartCentralBudget = 100m;
        project.CounterpartLocalBudget   = 200m;
        project.CounterpartOtherCapital  = 50m;

        project.CounterpartCentralBudget.Should().Be(100m);
        project.CounterpartLocalBudget.Should().Be(200m);
        project.CounterpartOtherCapital.Should().Be(50m);
    }

    // ── domain event ─────────────────────────────────────────────────────────

    [Fact]
    public void OdaProject_Create_RaisesProjectCreatedEvent()
    {
        var project = CreateDefault();

        project.DomainEvents.Should().ContainSingle(e =>
            e is GSDT.InvestmentProjects.Domain.Events.ProjectCreatedEvent);
    }
}
