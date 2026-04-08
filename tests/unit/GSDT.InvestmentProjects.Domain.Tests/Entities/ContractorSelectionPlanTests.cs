using FluentAssertions;
using GSDT.MasterData.Entities;

namespace GSDT.InvestmentProjects.Domain.Tests.Entities;

/// <summary>
/// Unit tests for ContractorSelectionPlan factory method.
/// Entity lives in MasterData but is referenced by investment project workflows.
/// </summary>
public sealed class ContractorSelectionPlanTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly DateTime SignedDate = new(2024, 6, 15);

    private static ContractorSelectionPlan CreateDefault() =>
        ContractorSelectionPlan.Create(
            tenantId:   TenantId,
            nameVi:     "Kế hoạch lựa chọn nhà thầu số 1",
            nameEn:     "Contractor Selection Plan No. 1",
            signedDate: SignedDate,
            signedBy:   "Nguyen Van A");

    // ── factory: field assignment ─────────────────────────────────────────────

    [Fact]
    public void ContractorSelectionPlan_Create_SetsTenantId()
    {
        var plan = CreateDefault();

        plan.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void ContractorSelectionPlan_Create_SetsNameVi()
    {
        var plan = CreateDefault();

        plan.NameVi.Should().Be("Kế hoạch lựa chọn nhà thầu số 1");
    }

    [Fact]
    public void ContractorSelectionPlan_Create_SetsNameEn()
    {
        var plan = CreateDefault();

        plan.NameEn.Should().Be("Contractor Selection Plan No. 1");
    }

    [Fact]
    public void ContractorSelectionPlan_Create_SetsSignedDate()
    {
        var plan = CreateDefault();

        plan.SignedDate.Should().Be(SignedDate);
    }

    [Fact]
    public void ContractorSelectionPlan_Create_SetsSignedBy()
    {
        var plan = CreateDefault();

        plan.SignedBy.Should().Be("Nguyen Van A");
    }

    // ── factory: defaults ─────────────────────────────────────────────────────

    [Fact]
    public void ContractorSelectionPlan_Create_IsActiveByDefault()
    {
        var plan = CreateDefault();

        plan.IsActive.Should().BeTrue();
    }

    // ── factory: identity ─────────────────────────────────────────────────────

    [Fact]
    public void ContractorSelectionPlan_Create_GeneratesNewId()
    {
        var a = CreateDefault();
        var b = CreateDefault();

        a.Id.Should().NotBe(b.Id);
        a.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ContractorSelectionPlan_Create_TwoInstances_HaveDifferentIds()
    {
        var ids = Enumerable.Range(0, 5)
            .Select(_ => CreateDefault().Id)
            .ToList();

        ids.Distinct().Should().HaveCount(5, because: "each Create call must produce a unique Guid");
    }
}
