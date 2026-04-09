using FluentResults;
using MediatR;
using GSDT.Identity.Application.Queries.GetPermissions;

namespace GSDT.Identity.Application.Queries.GetPermissionsByModule;

/// <summary>
/// Handles GetPermissionsByModuleQuery — fetches all permissions via Dapper then
/// groups by ModuleCode in C# (avoids complex GROUP BY with JSON aggregation).
/// </summary>
public sealed class GetPermissionsByModuleQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetPermissionsByModuleQuery, Result<IReadOnlyList<ModulePermissionsDto>>>
{
    public async Task<Result<IReadOnlyList<ModulePermissionsDto>>> Handle(
        GetPermissionsByModuleQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.Id, p.Code, p.Name, p.Description, p.ModuleCode, p.ResourceCode, p.ActionCode
            FROM [identity].Permissions p
            ORDER BY p.ModuleCode, p.Code
            """;

        var rows = await db.QueryAsync<PermissionRow>(sql, null, cancellationToken);

        var grouped = rows
            .GroupBy(r => r.ModuleCode)
            .Select(g => new ModulePermissionsDto(
                g.Key,
                g.Select(r => new PermissionDto(r.Id, r.Code, r.Name, r.Description, r.ModuleCode, r.ResourceCode, r.ActionCode))
                 .ToList()))
            .ToList();

        return Result.Ok<IReadOnlyList<ModulePermissionsDto>>(grouped);
    }
}
