using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.GetPermissions;

/// <summary>
/// Handles GetPermissionsQuery using Dapper (read-side).
/// Queries [identity].Permissions with optional ModuleCode / Search filters.
/// All SQL parameters are parameterized — no string interpolation.
/// </summary>
public sealed class GetPermissionsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        // Build SQL with optional WHERE clauses using parameterized conditions
        const string baseSql = """
            SELECT p.Id, p.Code, p.Name, p.Description, p.ModuleCode, p.ResourceCode, p.ActionCode
            FROM [identity].Permissions p
            WHERE 1 = 1
              AND (@ModuleCode IS NULL OR p.ModuleCode = @ModuleCode)
              AND (@Search     IS NULL OR p.Code LIKE @SearchPattern OR p.Name LIKE @SearchPattern)
            ORDER BY p.ModuleCode, p.Code
            """;

        // Escape LIKE wildcards (%, _, [) before wrapping to prevent unintended matching
        var searchPattern = request.Search is null
            ? null
            : $"%{request.Search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")}%";

        var rows = await db.QueryAsync<PermissionRow>(
            baseSql,
            new
            {
                ModuleCode    = request.ModuleCode,
                Search        = request.Search,
                SearchPattern = searchPattern
            },
            cancellationToken);

        var result = rows
            .Select(r => new PermissionDto(r.Id, r.Code, r.Name, r.Description, r.ModuleCode, r.ResourceCode, r.ActionCode))
            .ToList();

        return Result.Ok<IReadOnlyList<PermissionDto>>(result);
    }
}

/// <summary>
/// Flat projection row from the Permissions query.
/// Exposed as <c>internal</c> so unit tests can build mock return values.
/// </summary>
internal sealed class PermissionRow
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ResourceCode { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
}
