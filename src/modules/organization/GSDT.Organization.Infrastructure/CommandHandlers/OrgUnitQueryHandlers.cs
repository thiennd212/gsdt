using FluentResults;
using MediatR;

namespace GSDT.Organization.Infrastructure.CommandHandlers;

/// <summary>
/// Query handlers that require OrgDbContext live here in Infrastructure
/// to avoid Application → Infrastructure circular dependency.
/// Pure Dapper queries (IReadDbConnection only) stay in Application.
/// </summary>

public sealed class GetOrgTreeQueryHandler(OrgUnitService orgService)
    : IRequestHandler<GetOrgTreeQuery, Result<IReadOnlyList<OrgUnitDto>>>
{
    public async Task<Result<IReadOnlyList<OrgUnitDto>>> Handle(GetOrgTreeQuery req, CancellationToken ct)
        => Result.Ok(await orgService.GetTreeAsync(req.TenantId, ct));
}

public sealed class GetOrgUnitQueryHandler(OrgUnitService orgService)
    : IRequestHandler<GetOrgUnitQuery, Result<OrgUnitDto>>
{
    public async Task<Result<OrgUnitDto>> Handle(GetOrgUnitQuery req, CancellationToken ct)
    {
        var unit = await orgService.GetByIdAsync(req.Id, req.TenantId, ct);
        return unit is null
            ? Result.Fail(new NotFoundError($"OrgUnit '{req.Id}' not found."))
            : Result.Ok(unit);
    }
}

/// <summary>
/// [RT-03] Refactored: no cross-schema JOIN. Uses IIdentityModuleClient for user enrichment.
/// </summary>
public sealed class GetOrgUnitMembersQueryHandler(
    IReadDbConnection readDb,
    IIdentityModuleClient identityClient)
    : IRequestHandler<GetOrgUnitMembersQuery, Result<IReadOnlyList<UserOrgUnitAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<UserOrgUnitAssignmentDto>>> Handle(
        GetOrgUnitMembersQuery req, CancellationToken ct)
    {
        // Query local organization data only — no cross-schema JOIN
        const string sql = """
            SELECT a.Id, a.UserId, a.OrgUnitId, a.RoleInOrg, a.ValidFrom, a.ValidTo, a.IsActive
            FROM [organization].UserOrgUnitAssignments a
            WHERE a.OrgUnitId = @OrgUnitId AND a.TenantId = @TenantId
              AND a.IsActive = 1 AND a.IsDeleted = 0
            """;

        var rows = (await readDb.QueryAsync<UserOrgUnitAssignmentDto>(
            sql, new { req.OrgUnitId, req.TenantId }, ct)).ToList();

        if (rows.Count == 0)
            return Result.Ok<IReadOnlyList<UserOrgUnitAssignmentDto>>(rows);

        // Enrich with user names/emails via IIdentityModuleClient (cross-module ACL)
        var userIds = rows.Select(r => r.UserId).Distinct();
        var userMap = await identityClient.GetUserInfoByIdsAsync(userIds, req.TenantId, ct);

        var enriched = rows.Select(r =>
        {
            userMap.TryGetValue(r.UserId, out var user);
            return r with { FullName = user?.FullName, Email = user?.Email };
        })
        .OrderBy(r => r.FullName)
        .ToList();

        return Result.Ok<IReadOnlyList<UserOrgUnitAssignmentDto>>(enriched);
    }
}

public sealed class GetStaffHistoryQueryHandler(OrgDbContext db)
    : IRequestHandler<GetStaffHistoryQuery, Result<StaffHistoryDto>>
{
    public async Task<Result<StaffHistoryDto>> Handle(GetStaffHistoryQuery req, CancellationToken ct)
    {
        var assignments = await db.Query<UserOrgUnitAssignment>()
            .Where(x => x.UserId == req.UserId && x.TenantId == req.TenantId)
            .OrderByDescending(x => x.ValidFrom)
            .Select(x => new UserOrgUnitAssignmentDto(
                x.Id, x.UserId, x.OrgUnitId, x.RoleInOrg, x.ValidFrom, x.ValidTo, x.IsActive))
            .ToListAsync(ct);

        var positions = await db.Query<StaffPositionHistory>()
            .Where(x => x.UserId == req.UserId && x.TenantId == req.TenantId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new StaffPositionHistoryDto(
                x.Id, x.UserId, x.OrgUnitId, x.PositionTitle, x.StartDate, x.EndDate))
            .ToListAsync(ct);

        return Result.Ok(new StaffHistoryDto(assignments, positions));
    }
}
