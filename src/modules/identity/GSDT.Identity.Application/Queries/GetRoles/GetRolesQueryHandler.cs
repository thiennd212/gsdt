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
            SELECT Id, Name, NULL AS Description
            FROM [identity].AspNetRoles
            ORDER BY Name
            """;

        var roles = (await db.QueryAsync<RoleRow>(sql, cancellationToken: cancellationToken))
            .Select(r => new RoleDefinitionDto(r.Id, r.Name, r.Description))
            .ToList();

        return Result.Ok<IReadOnlyList<RoleDefinitionDto>>(roles);
    }

    private sealed class RoleRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
