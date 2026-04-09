using FluentResults;
using MediatR;
using GSDT.Identity.Application.Queries.GetPermissions;

namespace GSDT.Identity.Application.Queries.GetRolePermissions;

/// <summary>
/// Handles GetRolePermissionsQuery — joins RolePermissions with Permissions via Dapper.
/// Returns all permissions assigned to the given role (empty list = no assignments, not an error).
/// </summary>
public sealed class GetRolePermissionsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetRolePermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.Id, p.Code, p.Name, p.Description, p.ModuleCode, p.ResourceCode, p.ActionCode
            FROM [identity].RolePermissions rp
            INNER JOIN [identity].Permissions p ON p.Id = rp.PermissionId
            WHERE rp.RoleId = @RoleId
            ORDER BY p.ModuleCode, p.Code
            """;

        var rows = await db.QueryAsync<PermissionRow>(sql, new { RoleId = request.RoleId }, cancellationToken);

        var result = rows
            .Select(r => new PermissionDto(r.Id, r.Code, r.Name, r.Description, r.ModuleCode, r.ResourceCode, r.ActionCode))
            .ToList();

        return Result.Ok<IReadOnlyList<PermissionDto>>(result);
    }
}
