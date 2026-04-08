namespace GSDT.InvestmentProjects.Application.Commands.CreateOdaProject;

public sealed class CreateOdaProjectCommandValidator
    : AbstractValidator<CreateOdaProjectCommand>
{
    public CreateOdaProjectCommandValidator()
    {
        RuleFor(x => x.ProjectCode)
            .NotEmpty().WithMessage("Ma du an la bat buoc.")
            .MaximumLength(50).WithMessage("Ma du an khong qua 50 ky tu.");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Ten du an la bat buoc.")
            .MaximumLength(500).WithMessage("Ten du an khong qua 500 ky tu.");

        RuleFor(x => x.ShortName)
            .NotEmpty().WithMessage("Ten viet tat du an la bat buoc.")
            .MaximumLength(200).WithMessage("Ten viet tat khong qua 200 ky tu.");

        RuleFor(x => x.ManagingAuthorityId)
            .NotEmpty().WithMessage("Co quan chu quan la bat buoc.");

        RuleFor(x => x.IndustrySectorId)
            .NotEmpty().WithMessage("Nganh/linh vuc la bat buoc.");

        RuleFor(x => x.ProjectOwnerId)
            .NotEmpty().WithMessage("Chu dau tu la bat buoc.");

        RuleFor(x => x.OdaProjectTypeId)
            .NotEmpty().WithMessage("Loai du an ODA la bat buoc.");

        RuleFor(x => x.DonorId)
            .NotEmpty().WithMessage("Nha tai tro la bat buoc.");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Trang thai du an la bat buoc.");

        // Capital values must be non-negative
        RuleFor(x => x.OdaGrantCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von vien tro ODA phai >= 0.");

        RuleFor(x => x.OdaLoanCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von vay ODA phai >= 0.");

        RuleFor(x => x.CounterpartCentralBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Von doi ung ngan sach trung uong phai >= 0.");

        RuleFor(x => x.CounterpartLocalBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Von doi ung ngan sach dia phuong phai >= 0.");

        RuleFor(x => x.CounterpartOtherCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von doi ung khac phai >= 0.");

        // Mechanism percentages: 0–100
        RuleFor(x => x.GrantMechanismPercent)
            .InclusiveBetween(0, 100).WithMessage("Ti le co che cap phat phai tu 0 den 100.");

        RuleFor(x => x.RelendingMechanismPercent)
            .InclusiveBetween(0, 100).WithMessage("Ti le co che cho vay lai phai tu 0 den 100.");

        // Year range validation when both provided
        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .WithMessage("Nam ket thuc phai lon hon hoac bang nam bat dau.")
            .When(x => x.StartYear.HasValue && x.EndYear.HasValue);

        RuleFor(x => x.ProjectCodeQhns)
            .MaximumLength(100).WithMessage("Ma du an QHNS khong qua 100 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ProjectCodeQhns));

        RuleFor(x => x.CoDonorName)
            .MaximumLength(200).WithMessage("Ten dong tai tro khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.CoDonorName));

        RuleFor(x => x.PmuEmail)
            .EmailAddress().WithMessage("Email Ban QLDA khong hop le.")
            .When(x => !string.IsNullOrEmpty(x.PmuEmail));

        RuleFor(x => x.PmuPhone)
            .MaximumLength(20).WithMessage("So dien thoai Ban QLDA khong qua 20 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuPhone));

        RuleFor(x => x.PmuDirectorName)
            .MaximumLength(200).WithMessage("Ten giam doc Ban QLDA khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuDirectorName));

        RuleFor(x => x.ImplementationPeriod)
            .MaximumLength(200).WithMessage("Thoi gian thuc hien khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ImplementationPeriod));
    }
}
