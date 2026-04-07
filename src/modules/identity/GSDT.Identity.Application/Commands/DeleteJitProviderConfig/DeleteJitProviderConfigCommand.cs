
namespace GSDT.Identity.Application.Commands.DeleteJitProviderConfig;

public sealed record DeleteJitProviderConfigCommand(Guid Id, Guid ActorId) : ICommand;
