using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Queries.SodRule;

/// <summary>Handles ListSodRulesQuery — reads SoD conflict rules from IdentityDbContext.</summary>
public sealed class ListSodRulesQueryHandler(IdentityDbContext db)
    : IRequestHandler<ListSodRulesQuery, Result<IReadOnlyList<SodRuleDto>>>
{
    public async Task<Result<IReadOnlyList<SodRuleDto>>> Handle(
        ListSodRulesQuery request, CancellationToken ct)
    {
        var rules = await db.SodConflictRules
            .Where(r => request.TenantId == null || r.TenantId == request.TenantId)
            .OrderBy(r => r.PermissionCodeA)
            .ThenBy(r => r.PermissionCodeB)
            .Select(r => new SodRuleDto(
                r.Id,
                r.PermissionCodeA,
                r.PermissionCodeB,
                r.EnforcementLevel.ToString(),
                r.Description,
                r.IsActive,
                r.TenantId))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<SodRuleDto>>(rules);
    }
}
