namespace GSDT.InvestmentProjects.Application.Commands.CreateFdiProject;

public sealed class CreateFdiProjectCommandValidator
    : AbstractValidator<CreateFdiProjectCommand>
{
    public CreateFdiProjectCommandValidator()
    {
        RuleFor(x => x.ProjectCode)
            .NotEmpty().WithMessage("Ma du an la bat buoc.")
            .MaximumLength(50).WithMessage("Ma du an khong qua 50 ky tu.");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Ten du an la bat buoc.")
            .MaximumLength(500).WithMessage("Ten du an khong qua 500 ky tu.");

        RuleFor(x => x.ManagingAuthorityId)
            .NotEmpty().WithMessage("Co quan chu quan la bat buoc.");

        RuleFor(x => x.IndustrySectorId)
            .NotEmpty().WithMessage("Nganh/linh vuc la bat buoc.");

        RuleFor(x => x.ProjectOwnerId)
            .NotEmpty().WithMessage("Chu dau tu la bat buoc.");

        RuleFor(x => x.ProjectGroupId)
            .NotEmpty().WithMessage("Nhom du an la bat buoc.");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Trang thai du an la bat buoc.");

        RuleFor(x => x.PrelimTotalInvestment)
            .GreaterThanOrEqualTo(0).WithMessage("Tong von so bo phai >= 0.");

        RuleFor(x => x.PrelimEquityCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von chu so huu so bo phai >= 0.");

        RuleFor(x => x.PrelimOdaLoanCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von vay ODA so bo phai >= 0.");

        RuleFor(x => x.PrelimCreditLoanCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von vay TCTD so bo phai >= 0.");

        RuleFor(x => x.PrelimTotalInvestment)
            .Must((cmd, total) => total == cmd.PrelimEquityCapital + cmd.PrelimOdaLoanCapital + cmd.PrelimCreditLoanCapital)
            .WithMessage("Tong von so bo phai bang von CSH + von vay ODA + von vay TCTD.")
            .When(x => x.PrelimTotalInvestment > 0);

        RuleFor(x => x.AreaHectares)
            .GreaterThan(0).WithMessage("Dien tich dat phai > 0.")
            .When(x => x.AreaHectares.HasValue);

        RuleFor(x => x.StateOwnershipRatio)
            .InclusiveBetween(0, 100).WithMessage("Ty le von NN phai tu 0 den 100%.")
            .When(x => x.StateOwnershipRatio.HasValue);

        RuleFor(x => x.PmuEmail)
            .EmailAddress().WithMessage("Email Ban QLDA khong hop le.")
            .When(x => !string.IsNullOrEmpty(x.PmuEmail));

        RuleFor(x => x.PmuPhone)
            .MaximumLength(20).WithMessage("So dien thoai Ban QLDA khong qua 20 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuPhone));

        RuleFor(x => x.InvestorName)
            .MaximumLength(500).WithMessage("Ten nha dau tu khong qua 500 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.InvestorName));

        RuleFor(x => x.Objective)
            .MaximumLength(2000).WithMessage("Muc tieu du an khong qua 2000 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.Objective));

        RuleFor(x => x.ImplementationTimeline)
            .MaximumLength(200).WithMessage("Thoi gian thuc hien khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ImplementationTimeline));

        RuleFor(x => x.ProgressDescription)
            .MaximumLength(1000).WithMessage("Mo ta tien do khong qua 1000 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ProgressDescription));

        RuleFor(x => x.ImplementationPeriod)
            .MaximumLength(200).WithMessage("Thoi ky thuc hien khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ImplementationPeriod));
    }
}
