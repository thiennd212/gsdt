namespace GSDT.InvestmentProjects.Application.Commands.UpdateDomesticProject;

public sealed class UpdateDomesticProjectCommandValidator
    : AbstractValidator<UpdateDomesticProjectCommand>
{
    public UpdateDomesticProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id du an la bat buoc.");

        RuleFor(x => x.RowVersion)
            .NotNull().NotEmpty().WithMessage("RowVersion la bat buoc de kiem tra dong thoi.");

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

        RuleFor(x => x.PrelimCentralBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Von ngan sach trung uong so bo phai >= 0.");

        RuleFor(x => x.PrelimLocalBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Von ngan sach dia phuong so bo phai >= 0.");

        RuleFor(x => x.PrelimOtherPublicCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von dau tu cong khac so bo phai >= 0.");

        RuleFor(x => x.PrelimOtherCapital)
            .GreaterThanOrEqualTo(0).WithMessage("Von khac so bo phai >= 0.");

        RuleFor(x => x.PmuEmail)
            .EmailAddress().WithMessage("Email Ban QLDA khong hop le.")
            .When(x => !string.IsNullOrEmpty(x.PmuEmail));

        RuleFor(x => x.PmuPhone)
            .MaximumLength(20).WithMessage("So dien thoai Ban QLDA khong qua 20 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuPhone));

        RuleFor(x => x.PmuDirectorName)
            .MaximumLength(200).WithMessage("Ten giam doc Ban QLDA khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.PmuDirectorName));

        RuleFor(x => x.TreasuryCode)
            .MaximumLength(50).WithMessage("Ma Kho bac Nha nuoc khong qua 50 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.TreasuryCode));

        RuleFor(x => x.ImplementationPeriod)
            .MaximumLength(200).WithMessage("Thoi gian thuc hien khong qua 200 ky tu.")
            .When(x => !string.IsNullOrEmpty(x.ImplementationPeriod));

        // Stop fields: if any stop field is provided, StopDecisionNumber is required
        RuleFor(x => x.StopDecisionNumber)
            .NotEmpty().WithMessage("So quyet dinh dinh chi la bat buoc khi co noi dung dinh chi.")
            .When(x => !string.IsNullOrEmpty(x.StopContent) || x.StopDecisionDate.HasValue);
    }
}
