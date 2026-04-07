using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Commands.ManageSodRule;

/// <summary>Creates a new SoD conflict rule. Pair (A, B, TenantId) must be unique.</summary>
public sealed class CreateSodRuleCommandHandler(IdentityDbContext db)
    : IRequestHandler<CreateSodRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSodRuleCommand cmd, CancellationToken ct)
    {
        if (!Enum.TryParse<SodEnforcementLevel>(cmd.EnforcementLevel, ignoreCase: true, out var level))
            return Result.Fail(new ValidationError(
                $"Invalid EnforcementLevel '{cmd.EnforcementLevel}'. Use Warn, Block, or RequireApproval."));

        var duplicate = await db.SodConflictRules.AnyAsync(r =>
            r.TenantId == cmd.TenantId
            && ((r.PermissionCodeA == cmd.PermissionCodeA && r.PermissionCodeB == cmd.PermissionCodeB)
             || (r.PermissionCodeA == cmd.PermissionCodeB && r.PermissionCodeB == cmd.PermissionCodeA)), ct);

        if (duplicate)
            return Result.Fail(new ConflictError(
                $"A SoD rule for ({cmd.PermissionCodeA}, {cmd.PermissionCodeB}) already exists."));

        var rule = new SodConflictRule
        {
            Id = Guid.NewGuid(),
            PermissionCodeA = cmd.PermissionCodeA,
            PermissionCodeB = cmd.PermissionCodeB,
            EnforcementLevel = level,
            Description = cmd.Description,
            TenantId = cmd.TenantId,
            IsActive = true
        };

        db.SodConflictRules.Add(rule);
        await db.SaveChangesAsync(ct);
        return Result.Ok(rule.Id);
    }
}

/// <summary>Updates an existing SoD conflict rule.</summary>
public sealed class UpdateSodRuleCommandHandler(IdentityDbContext db)
    : IRequestHandler<UpdateSodRuleCommand, Result>
{
    public async Task<Result> Handle(UpdateSodRuleCommand cmd, CancellationToken ct)
    {
        var rule = await db.SodConflictRules.FindAsync([cmd.Id], ct);
        if (rule is null)
            return Result.Fail(new NotFoundError($"SodConflictRule {cmd.Id} not found."));

        if (!Enum.TryParse<SodEnforcementLevel>(cmd.EnforcementLevel, ignoreCase: true, out var level))
            return Result.Fail(new ValidationError(
                $"Invalid EnforcementLevel '{cmd.EnforcementLevel}'. Use Warn, Block, or RequireApproval."));

        rule.PermissionCodeA = cmd.PermissionCodeA;
        rule.PermissionCodeB = cmd.PermissionCodeB;
        rule.EnforcementLevel = level;
        rule.Description = cmd.Description;
        rule.IsActive = cmd.IsActive;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

/// <summary>Deletes a SoD conflict rule by ID.</summary>
public sealed class DeleteSodRuleCommandHandler(IdentityDbContext db)
    : IRequestHandler<DeleteSodRuleCommand, Result>
{
    public async Task<Result> Handle(DeleteSodRuleCommand cmd, CancellationToken ct)
    {
        var rule = await db.SodConflictRules.FindAsync([cmd.Id], ct);
        if (rule is null)
            return Result.Fail(new NotFoundError($"SodConflictRule {cmd.Id} not found."));

        db.SodConflictRules.Remove(rule);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
