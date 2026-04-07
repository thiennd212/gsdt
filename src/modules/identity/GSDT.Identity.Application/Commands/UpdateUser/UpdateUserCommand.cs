
namespace GSDT.Identity.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    string? DepartmentCode,
    Guid? OrgUnitId,
    Guid ActorId) : ICommand;
