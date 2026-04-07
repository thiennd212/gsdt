using FluentResults;

namespace GSDT.Identity.Application.Queries.ListTenants;

/// <summary>Returns distinct tenant IDs with user counts — SystemAdmin only.</summary>
public sealed record ListTenantsQuery : IQuery<IReadOnlyList<TenantSummaryDto>>;

public sealed class TenantSummaryDto
{
    public Guid TenantId { get; init; }
    public string? TenantName { get; init; }
    public int UserCount { get; init; }
}
