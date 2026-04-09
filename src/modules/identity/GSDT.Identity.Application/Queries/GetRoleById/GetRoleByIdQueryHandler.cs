using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.GetRoleById;

/// <summary>
/// Handles GetRoleByIdQuery — fetches role + permissions via Dapper (read-side).
/// JOINs [identity].AspNetRoles with RolePermissions and Permissions tables.
/// </summary>
public sealed class GetRoleByIdQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetRoleByIdQuery, Result<RoleDetailDto>>
{
    public async Task<Result<RoleDetailDto>> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                r.Id,
                r.Code,
                r.Name,
                r.Description,
                CASE r.RoleType WHEN 1 THEN 'System' ELSE 'Business' END AS RoleType,
                r.IsActive,
                p.Id   AS PermissionId,
                p.Code AS PermissionCode,
                p.Name AS PermissionName
            FROM [identity].AspNetRoles r
            LEFT JOIN [identity].RolePermissions rp ON rp.RoleId = r.Id
            LEFT JOIN [identity].Permissions      p  ON p.Id = rp.PermissionId
            WHERE r.Id = @Id
            """;

        var rows = (await db.QueryAsync<RoleDetailRow>(sql, new { request.Id }, cancellationToken)).ToList();

        if (rows.Count == 0)
            return Result.Fail($"Role '{request.Id}' not found.");

        // First row carries the role metadata; remaining rows are extra permissions
        var first = rows[0];
        var permissions = rows
            .Where(r => r.PermissionId.HasValue)
            .Select(r => new RolePermissionDto(r.PermissionId!.Value, r.PermissionCode!, r.PermissionName!))
            .ToList();

        var dto = new RoleDetailDto(
            first.Id,
            first.Code,
            first.Name,
            first.Description,
            first.RoleType,
            first.IsActive,
            permissions);

        return Result.Ok(dto);
    }
}

/// <summary>
/// Flat projection row from the JOIN query.
/// Exposed as <c>internal</c> so unit tests can build mock return values without Dapper.
/// </summary>
internal sealed class RoleDetailRow
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? PermissionId { get; set; }
    public string? PermissionCode { get; set; }
    public string? PermissionName { get; set; }
}
