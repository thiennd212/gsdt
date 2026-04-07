
namespace GSDT.Identity.Application.Commands.CreateExternalIdentity;

public sealed record CreateExternalIdentityCommand(
    Guid UserId,
    ExternalIdentityProvider Provider,
    string ExternalId,
    string? DisplayName,
    string? Email,
    Guid ActorId) : ICommand<Guid>;
