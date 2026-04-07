
namespace GSDT.Identity.Application.Commands.DeleteUser;

/// <summary>Soft-delete (deactivate) a user. Sets IsActive=false + locks the account.</summary>
public sealed record DeleteUserCommand(Guid UserId, Guid ActorId) : ICommand;
