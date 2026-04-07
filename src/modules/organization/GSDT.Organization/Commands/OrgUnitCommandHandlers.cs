using FluentResults;
using MediatR;

namespace GSDT.Organization.Commands;

public sealed class CreateOrgUnitCommandHandler(OrgDbContext db, OrgUnitService orgService)
    : IRequestHandler<CreateOrgUnitCommand, Result<OrgUnitDto>>
{
    public async Task<Result<OrgUnitDto>> Handle(CreateOrgUnitCommand req, CancellationToken ct)
    {
        // Uniqueness: code per tenant
        var exists = await db.OrgUnits
            .AnyAsync(x => x.TenantId == req.TenantId && x.Code == req.Code, ct);
        if (exists)
            return Result.Fail(new ConflictError($"OrgUnit code '{req.Code}' already exists in this tenant."));

        // Compute level from parent
        int level = 1;
        if (req.ParentId.HasValue)
        {
            var parent = await db.OrgUnits
                .FirstOrDefaultAsync(x => x.Id == req.ParentId && x.TenantId == req.TenantId, ct);
            if (parent is null)
                return Result.Fail(new NotFoundError($"Parent OrgUnit '{req.ParentId}' not found."));
            level = parent.Level + 1;
        }

        var unit = OrgUnit.Create(req.Name, req.NameEn, req.Code, req.TenantId, req.ParentId, level);
        db.OrgUnits.Add(unit);
        await db.SaveChangesAsync(ct);
        await orgService.InvalidateCacheAsync(req.TenantId, ct);

        return Result.Ok(ToDto(unit));
    }

    private static OrgUnitDto ToDto(OrgUnit u) =>
        new(u.Id, u.ParentId, u.Name, u.NameEn, u.Code, u.Level, u.IsActive, u.TenantId, u.SuccessorId);
}

public sealed class UpdateOrgUnitCommandHandler(OrgDbContext db, OrgUnitService orgService)
    : IRequestHandler<UpdateOrgUnitCommand, Result<OrgUnitDto>>
{
    public async Task<Result<OrgUnitDto>> Handle(UpdateOrgUnitCommand req, CancellationToken ct)
    {
        var unit = await db.TrackingQuery<OrgUnit>()
            .FirstOrDefaultAsync(x => x.Id == req.Id && x.TenantId == req.TenantId, ct);
        if (unit is null)
            return Result.Fail(new NotFoundError($"OrgUnit '{req.Id}' not found."));

        unit.Update(req.Name, req.NameEn);
        await db.SaveChangesAsync(ct);
        await orgService.InvalidateCacheAsync(req.TenantId, ct);

        return Result.Ok(new OrgUnitDto(
            unit.Id, unit.ParentId, unit.Name, unit.NameEn,
            unit.Code, unit.Level, unit.IsActive, unit.TenantId, unit.SuccessorId));
    }
}

public sealed class DeleteOrgUnitCommandHandler(OrgDbContext db, OrgUnitService orgService)
    : IRequestHandler<DeleteOrgUnitCommand, Result>
{
    public async Task<Result> Handle(DeleteOrgUnitCommand req, CancellationToken ct)
    {
        var unit = await db.TrackingQuery<OrgUnit>()
            .FirstOrDefaultAsync(x => x.Id == req.Id && x.TenantId == req.TenantId, ct);
        if (unit is null)
            return Result.Fail(new NotFoundError($"OrgUnit '{req.Id}' not found."));

        if (await orgService.HasActiveChildrenAsync(req.Id, ct))
            return Result.Fail(new ValidationError(
                "Cannot deactivate org unit with active children. Deactivate children first.",
                "Id"));

        if (req.SuccessorId.HasValue)
            unit.SetSuccessor(req.SuccessorId.Value);

        unit.Deactivate();
        await db.SaveChangesAsync(ct);
        await orgService.InvalidateCacheAsync(req.TenantId, ct);

        return Result.Ok();
    }
}
