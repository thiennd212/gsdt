using FluentValidation;

namespace GSDT.Identity.Application.Commands.UpdateCredentialPolicy;

public sealed class UpdateCredentialPolicyCommandValidator : AbstractValidator<UpdateCredentialPolicyCommand>
{
    public UpdateCredentialPolicyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinLength).InclusiveBetween(8, 128);
        RuleFor(x => x.MaxLength).GreaterThanOrEqualTo(x => x.MinLength).LessThanOrEqualTo(256);
        RuleFor(x => x.RotationDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxFailedAttempts).InclusiveBetween(1, 100);
        RuleFor(x => x.LockoutMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PasswordHistoryCount).InclusiveBetween(0, 24);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
