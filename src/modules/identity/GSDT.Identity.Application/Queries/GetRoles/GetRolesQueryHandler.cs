using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.GetRoles;

/// <summary>
/// Returns all roles from AspNetRoles table with Id + Name.
/// Replaces static catalogue — DB query ensures FE gets real GUIDs for data-scopes.
/// </summary>
public sealed class GetRolesQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDefinitionDto>>>
{
    public async Task<Result<IReadOnlyList<RoleDefinitionDto>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT r.Id, r.Code, r.Name, r.Description, r.RoleType, r.IsActive,
                   COUNT(rp.RoleId) AS PermissionCount
            FROM [identity].AspNetRoles r
            LEFT JOIN [identity].RolePermissions rp ON rp.RoleId = r.Id
            GROUP BY r.Id, r.Code, r.Name, r.Description, r.RoleType, r.IsActive
            ORDER BY r.Name
            """;

        var roles = (await db.QueryAsync<RoleRow>(sql, cancellationToken: cancellationToken))
            .Select(r => new RoleDefinitionDto(r.Id, r.Code, r.Name, r.Description, r.RoleType, r.IsActive, r.PermissionCount))
            .ToList();

        return Result.Ok<IReadOnlyList<RoleDefinitionDto>>(roles);
    }

    private sealed class RoleRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RoleType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PermissionCount { get; set; }
    }
}
