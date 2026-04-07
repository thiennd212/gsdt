using FluentResults;
using MediatR;

namespace GSDT.Organization.Infrastructure.CommandHandlers;

public sealed class AssignStaffCommandHandler(OrgDbContext db)
    : IRequestHandler<AssignStaffCommand, Result<UserOrgUnitAssignmentDto>>
{
    public async Task<Result<UserOrgUnitAssignmentDto>> Handle(AssignStaffCommand req, CancellationToken ct)
    {
        var unitExists = await db.Query<OrgUnit>()
            .AnyAsync(x => x.Id == req.OrgUnitId && x.TenantId == req.TenantId && x.IsActive, ct);
        if (!unitExists)
            return Result.Fail(new NotFoundError($"Active OrgUnit '{req.OrgUnitId}' not found."));

        var assignment = UserOrgUnitAssignment.Create(req.UserId, req.OrgUnitId, req.RoleInOrg, req.TenantId);
        db.Assignments.Add(assignment);

        var history = StaffPositionHistory.Create(req.UserId, req.OrgUnitId, req.PositionTitle, req.TenantId);
        db.PositionHistories.Add(history);

        await db.SaveChangesAsync(ct);

        return Result.Ok(new UserOrgUnitAssignmentDto(
            assignment.Id, assignment.UserId, assignment.OrgUnitId,
            assignment.RoleInOrg, assignment.ValidFrom, assignment.ValidTo, assignment.IsActive));
    }
}

public sealed class TransferStaffCommandHandler(OrgDbContext db)
    : IRequestHandler<TransferStaffCommand, Result<UserOrgUnitAssignmentDto>>
{
    public async Task<Result<UserOrgUnitAssignmentDto>> Handle(TransferStaffCommand req, CancellationToken ct)
    {
        var targetExists = await db.Query<OrgUnit>()
            .AnyAsync(x => x.Id == req.ToOrgUnitId && x.TenantId == req.TenantId && x.IsActive, ct);
        if (!targetExists)
            return Result.Fail(new NotFoundError($"Target OrgUnit '{req.ToOrgUnitId}' not found."));

        var now = DateTimeOffset.UtcNow;

        var activeAssignments = await db.TrackingQuery<UserOrgUnitAssignment>()
            .Where(x => x.UserId == req.UserId && x.TenantId == req.TenantId && x.IsActive)
            .ToListAsync(ct);
        foreach (var a in activeAssignments) a.Close(now);

        var openHistories = await db.TrackingQuery<StaffPositionHistory>()
            .Where(x => x.UserId == req.UserId && x.TenantId == req.TenantId && x.EndDate == null)
            .ToListAsync(ct);
        foreach (var h in openHistories) h.Close(now);

        var newAssignment = UserOrgUnitAssignment.Create(req.UserId, req.ToOrgUnitId, req.RoleInOrg, req.TenantId, now);
        db.Assignments.Add(newAssignment);

        var newHistory = StaffPositionHistory.Create(req.UserId, req.ToOrgUnitId, req.PositionTitle, req.TenantId, now);
        db.PositionHistories.Add(newHistory);

        await db.SaveChangesAsync(ct);

        return Result.Ok(new UserOrgUnitAssignmentDto(
            newAssignment.Id, newAssignment.UserId, newAssignment.OrgUnitId,
            newAssignment.RoleInOrg, newAssignment.ValidFrom, newAssignment.ValidTo, newAssignment.IsActive));
    }
}

