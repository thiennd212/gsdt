namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>Soft-deletes a business role by setting IsActive = false. System roles are rejected.</summary>
public sealed record DeleteRoleCommand(Guid Id) : ICommand;
