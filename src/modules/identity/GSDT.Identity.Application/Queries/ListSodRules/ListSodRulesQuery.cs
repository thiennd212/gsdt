
namespace GSDT.Identity.Application.Queries.ListSodRules;

/// <summary>List SoD conflict rules, optionally filtered by tenant.</summary>
public sealed record ListSodRulesQuery(Guid? TenantId) : IQuery<IReadOnlyList<SodRuleDto>>;

public sealed record SodRuleDto(
    Guid Id,
    string PermissionCodeA,
    string PermissionCodeB,
    string EnforcementLevel,
    string? Description,
    bool IsActive,
    Guid? TenantId);
