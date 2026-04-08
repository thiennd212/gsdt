namespace GSDT.InvestmentProjects.Application.Commands.CreatePppProject;

public sealed class CreatePppProjectCommandValidator
    : AbstractValidator<CreatePppProjectCommand>
{
    public CreatePppProjectCommandValidator()
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

        // Capital values must be non-negative
        RuleFor(x => x.PrelimTotalInvestment)
            .GreaterThanOrEqualTo(0).WithMessage("Tong von so bo phai >= 0.");

        RuleFor(x => x.PrelimStateCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von nha nuoc so bo phai >= 0.");

        RuleFor(x => x.PrelimEquityCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von chu so huu so bo phai >= 0.");

        RuleFor(x => x.PrelimLoanCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von vay so bo phai >= 0.");

        RuleFor(x => x.AreaHectares)
            .GreaterThan(0).WithMessage("Dien tich dat phai > 0.")
            .When(x => x.AreaHectares.HasValue);

        // PMU optional fields
        RuleFor(x => x.PmuEmail)
            .EmailAddress().WithMessage("Email Ban QLDA khong hop le.")
            .When(x => !string.IsNullOrEmpty(x.PmuEmail));

        RuleFor(x => x.PmuPhone)
            .MaximumLength(20).WithMessage("So dien thoai Ban QLDA khong qua 20 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuPhone));

        RuleFor(x => x.PmuDirectorName)
            .MaximumLength(200).WithMessage("Ten giam doc Ban QLDA khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuDirectorName));

        RuleFor(x => x.PreparationUnit)
            .MaximumLength(500).WithMessage("Don vi chuan bi du an khong qua 500 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PreparationUnit));

        RuleFor(x => x.Objective)
            .MaximumLength(2000).WithMessage("Muc tieu du an khong qua 2000 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.Objective));

        RuleFor(x => x.ImplementationPeriod)
            .MaximumLength(200).WithMessage("Thoi gian thuc hien khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ImplementationPeriod));
    }
}
