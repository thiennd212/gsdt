using FluentResults;
using GSDT.SharedKernel.Errors;
using MediatR;
using GSDT.Identity.Application.Queries.GetRoleById;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>
/// Handles CreateRoleCommand — creates a new business role via ASP.NET Identity RoleManager.
/// Enforces Code uniqueness before creation.
/// </summary>
public sealed class CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager)
    : IRequestHandler<CreateRoleCommand, Result<RoleDetailDto>>
{
    public async Task<Result<RoleDetailDto>> Handle(CreateRoleCommand cmd, CancellationToken ct)
    {
        // Enforce Code uniqueness (Code is a custom field, not the Identity Name).
        // Synchronous Any() is used so the check works against any IQueryable provider
        // (including unit-test mocks) without requiring an EF async provider.
        var codeExists = roleManager.Roles.Any(r => r.Code == cmd.Code);

        if (codeExists)
            return Result.Fail(new ConflictError($"Mã vai trò '{cmd.Code}' đã tồn tại."));

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Code = cmd.Code,
            Name = cmd.Name,
            Description = cmd.Description,
            RoleType = RoleType.Business,
            IsActive = true
        };

        var identityResult = await roleManager.CreateAsync(role);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            return Result.Fail($"Không thể tạo vai trò: {errors}");
        }

        var dto = new RoleDetailDto(
            role.Id,
            role.Code,
            role.Name,
            role.Description,
            role.RoleType.ToString(),
            role.IsActive,
            Array.Empty<RolePermissionDto>());

        return Result.Ok(dto);
    }
}
