
namespace GSDT.Identity.Application.Commands.ManageGroup;

/// <summary>Add a user to a group. Idempotent — no error if membership already exists.</summary>
public sealed record AddUserToGroupCommand(
    Guid GroupId,
    Guid UserId,
    Guid AddedBy) : ICommand;
