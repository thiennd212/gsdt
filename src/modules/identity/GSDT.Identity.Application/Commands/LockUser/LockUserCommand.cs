using FluentResults;

namespace GSDT.Identity.Application.Commands.LockUser;

public sealed record LockUserCommand(Guid UserId, bool Lock, Guid ActorId) : ICommand;
