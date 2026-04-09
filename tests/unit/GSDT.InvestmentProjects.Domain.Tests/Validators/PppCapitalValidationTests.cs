using FluentAssertions;
using FluentValidation.TestHelper;
using GSDT.InvestmentProjects.Application.Commands.CreatePppProject;
using GSDT.InvestmentProjects.Domain.Enums;

namespace GSDT.InvestmentProjects.Domain.Tests.Validators;

/// <summary>
/// Tests PPP capital validation rules: non-negative values for Total, State, Equity, Loan.
/// </summary>
public sealed class PppCapitalValidationTests
{
    private readonly CreatePppProjectCommandValidator _validator = new();

    private static CreatePppProjectCommand BuildCommand(
        decimal total = 100, decimal state = 50, decimal equity = 30, decimal loan = 20) =>
        new("PPP-VAL-01", "Capital Test", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), PppContractType.BOT, SubProjectType.NotSubProject,
            Guid.NewGuid(), null, null, null,
            total, state, equity, loan,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null);

    [Fact]
    public void Validate_AllPositiveCapital_Passes()
    {
        var cmd = BuildCommand(total: 100, state: 50, equity: 30, loan: 20);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimTotalInvestment);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimStateCapital);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimEquityCapital);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimLoanCapital);
    }

    [Fact]
    public void Validate_NegativeTotal_Fails()
    {
        var cmd = BuildCommand(total: -10);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PrelimTotalInvestment);
    }

    [Fact]
    public void Validate_NegativeStateCapital_Fails()
    {
        var cmd = BuildCommand(state: -5);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PrelimStateCapital);
    }

    [Fact]
    public void Validate_AllZeros_Passes()
    {
        var cmd = BuildCommand(total: 0, state: 0, equity: 0, loan: 0);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.PrelimTotalInvestment);
    }

    [Fact]
    public void Validate_RequiredFields_MissingProjectCode_Fails()
    {
        var cmd = new CreatePppProjectCommand(
            "", "Test", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), PppContractType.BOT, SubProjectType.NotSubProject,
            Guid.NewGuid(), null, null, null,
            100, 50, 30, 20, null, null, null,
            null, null, null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ProjectCode);
    }
}
