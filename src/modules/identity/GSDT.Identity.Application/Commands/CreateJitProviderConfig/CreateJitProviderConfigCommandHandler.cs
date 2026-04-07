using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.CreateJitProviderConfig;

public sealed class CreateJitProviderConfigCommandHandler(
    IJitProviderConfigRepository repository)
    : IRequestHandler<CreateJitProviderConfigCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateJitProviderConfigCommand cmd, CancellationToken ct)
    {
        // Enforce unique scheme constraint at application layer before hitting DB
        var existing = await repository.GetBySchemeAsync(cmd.Scheme, ct);
        if (existing is not null)
            return Result.Fail(new ConflictError(
                $"A JIT provider config with scheme '{cmd.Scheme}' already exists"));

        var entity = JitProviderConfig.Create(
            cmd.Scheme, cmd.DisplayName, cmd.ProviderType,
            cmd.JitEnabled, cmd.DefaultRoleName, cmd.RequireApproval,
            cmd.ClaimMappingJson, cmd.DefaultTenantId,
            cmd.AllowedDomainsJson, cmd.MaxProvisionsPerHour, cmd.ActorId);

        await repository.AddAsync(entity, ct);
        return Result.Ok(entity.Id);
    }
}
