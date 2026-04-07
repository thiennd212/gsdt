
namespace GSDT.Identity.Application.Commands.UpdateJitProviderConfig;

public sealed record UpdateJitProviderConfigCommand(
    Guid Id,
    string DisplayName,
    bool JitEnabled,
    string DefaultRoleName,
    bool RequireApproval,
    string? ClaimMappingJson,
    Guid? DefaultTenantId,
    string? AllowedDomainsJson,
    int MaxProvisionsPerHour,
    Guid ActorId) : ICommand;
