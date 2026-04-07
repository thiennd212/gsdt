using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.CreateCredentialPolicy;

public sealed class CreateCredentialPolicyCommandHandler(
    ICredentialPolicyRepository repository)
    : IRequestHandler<CreateCredentialPolicyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCredentialPolicyCommand cmd, CancellationToken ct)
    {
        var policy = CredentialPolicy.Create(
            cmd.Name, cmd.TenantId,
            cmd.MinLength, cmd.MaxLength,
            cmd.RequireUppercase, cmd.RequireLowercase,
            cmd.RequireDigit, cmd.RequireSpecialChar,
            cmd.RotationDays, cmd.MaxFailedAttempts,
            cmd.LockoutMinutes, cmd.PasswordHistoryCount,
            cmd.IsDefault, cmd.ActorId);

        await repository.AddAsync(policy, ct);
        return Result.Ok(policy.Id);
    }
}
