using FluentResults;

namespace GSDT.Identity.Application.Commands.AssignRole;

public sealed record AssignRoleCommand(
    Guid UserId,
    string RoleName,
    Guid AssignedBy) : ICommand;
