
namespace GSDT.Identity.Application.Commands.CreateJitProviderConfig;

public sealed record CreateJitProviderConfigCommand(
    string Scheme,
    string DisplayName,
    ExternalIdentityProvider ProviderType,
    bool JitEnabled,
    string DefaultRoleName,
    bool RequireApproval,
    string? ClaimMappingJson,
    Guid? DefaultTenantId,
    string? AllowedDomainsJson,
    int MaxProvisionsPerHour,
    Guid ActorId) : ICommand<Guid>;
