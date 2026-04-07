using FluentResults;

namespace GSDT.Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string? DepartmentCode,
    Guid? TenantId) : ICommand<Guid>;
