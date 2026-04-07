using FluentResults;
using MediatR;

namespace GSDT.Organization.Queries;

// ── Query Records ────────────────────────────────────────────────────────────

/// <summary>Get full flat list of org units for a tenant (client-side tree rendering).</summary>
public sealed record GetOrgTreeQuery(Guid TenantId) : IQuery<IReadOnlyList<OrgUnitDto>>;

/// <summary>Get a single org unit by Id.</summary>
public sealed record GetOrgUnitQuery(Guid Id, Guid TenantId) : IQuery<OrgUnitDto>;

/// <summary>Get all active members (assignments) of an org unit.</summary>
public sealed record GetOrgUnitMembersQuery(Guid OrgUnitId, Guid TenantId)
    : IQuery<IReadOnlyList<UserOrgUnitAssignmentDto>>;

/// <summary>Get full assignment and position history for a user.</summary>
public sealed record GetStaffHistoryQuery(Guid UserId, Guid TenantId) : IQuery<StaffHistoryDto>;

/// <summary>Combined staff history response.</summary>
public sealed record StaffHistoryDto(
    IReadOnlyList<UserOrgUnitAssignmentDto> Assignments,
    IReadOnlyList<StaffPositionHistoryDto> PositionHistory);

// ── Query Handlers ───────────────────────────────────────────────────────────

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

public sealed class GetOrgUnitMembersQueryHandler(OrgDbContext db)
    : IRequestHandler<GetOrgUnitMembersQuery, Result<IReadOnlyList<UserOrgUnitAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<UserOrgUnitAssignmentDto>>> Handle(
        GetOrgUnitMembersQuery req, CancellationToken ct)
    {
        var members = await db.Query<UserOrgUnitAssignment>()
            .Where(x => x.OrgUnitId == req.OrgUnitId && x.TenantId == req.TenantId && x.IsActive)
            .Select(x => new UserOrgUnitAssignmentDto(
                x.Id, x.UserId, x.OrgUnitId, x.RoleInOrg, x.ValidFrom, x.ValidTo, x.IsActive))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<UserOrgUnitAssignmentDto>>(members);
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
