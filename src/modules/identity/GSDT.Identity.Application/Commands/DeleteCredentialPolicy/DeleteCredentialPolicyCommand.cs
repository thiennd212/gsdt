
namespace GSDT.Identity.Application.Commands.DeleteCredentialPolicy;

public sealed record DeleteCredentialPolicyCommand(Guid Id, Guid ActorId) : ICommand;
