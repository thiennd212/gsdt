
namespace GSDT.Identity.Application.Commands.DeleteExternalIdentity;

public sealed record DeleteExternalIdentityCommand(Guid Id, Guid ActorId) : ICommand;
