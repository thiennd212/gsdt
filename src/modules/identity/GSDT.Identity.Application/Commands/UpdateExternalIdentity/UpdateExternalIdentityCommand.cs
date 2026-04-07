
namespace GSDT.Identity.Application.Commands.UpdateExternalIdentity;

public sealed record UpdateExternalIdentityCommand(
    Guid Id,
    string? DisplayName,
    string? Email,
    string? Metadata,
    Guid ActorId) : ICommand;
