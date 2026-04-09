using FluentAssertions;
using FluentValidation.TestHelper;
using GSDT.InvestmentProjects.Application.Commands.CreateDnnnProject;

namespace GSDT.InvestmentProjects.Domain.Tests.Validators;

/// <summary>
/// Tests DNNN capital validation rule: Total == CSH + ODA + TCTD.
/// Same rule applies to NĐT/FDI (identical capital structure).
/// </summary>
public sealed class DnnnCapitalValidationTests
{
    private readonly CreateDnnnProjectCommandValidator _validator = new();

    private static CreateDnnnProjectCommand BuildCommand(
        decimal total = 100, decimal csh = 50, decimal oda = 30, decimal tctd = 20) =>
        new(ProjectCode: "DNNN-VAL-01",
            ProjectName: "Capital Test",
            ManagingAuthorityId: Guid.NewGuid(),
            IndustrySectorId: Guid.NewGuid(),
            ProjectOwnerId: Guid.NewGuid(),
            ProjectGroupId: Guid.NewGuid(),
            SubProjectType: InvestmentProjects.Domain.Enums.SubProjectType.NotSubProject,
            StatusId: Guid.NewGuid(),
            CompetentAuthorityId: null,
            InvestorName: null,
            StateOwnershipRatio: null,
            Objective: null,
            PrelimTotalInvestment: total,
            PrelimEquityCapital: csh,
            PrelimOdaLoanCapital: oda,
            PrelimCreditLoanCapital: tctd,
            AreaHectares: null,
            Capacity: null,
            MainItems: null,
            ImplementationTimeline: null,
            ProgressDescription: null,
            ProjectManagementUnitId: null,
            PmuDirectorName: null,
            PmuPhone: null,
            PmuEmail: null,
            ImplementationPeriod: null,
            PolicyDecisionNumber: null,
            PolicyDecisionDate: null,
            PolicyDecisionAuthority: null,
            PolicyDecisionPerson: null,
            PolicyDecisionFileId: null);

    [Fact]
    public void Validate_CapitalBalances_Passes()
    {
        var cmd = BuildCommand(total: 100, csh: 50, oda: 30, tctd: 20);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimTotalInvestment);
    }

    [Fact]
    public void Validate_CapitalMismatch_Fails()
    {
        var cmd = BuildCommand(total: 100, csh: 50, oda: 30, tctd: 10); // 90 != 100
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PrelimTotalInvestment)
            .WithErrorMessage("Tong von so bo phai bang von CSH + von vay ODA + von vay TCTD.");
    }

    [Fact]
    public void Validate_AllZeros_Passes()
    {
        var cmd = BuildCommand(total: 0, csh: 0, oda: 0, tctd: 0);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimTotalInvestment);
    }

    [Fact]
    public void Validate_NegativeCapital_Fails()
    {
        var cmd = BuildCommand(total: -10, csh: -10, oda: 0, tctd: 0);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PrelimTotalInvestment);
    }
}
