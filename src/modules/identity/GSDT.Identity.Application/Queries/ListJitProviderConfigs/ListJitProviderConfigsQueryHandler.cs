using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListJitProviderConfigs;

/// <summary>Read-side query via Dapper — follows ListExternalIdentitiesQueryHandler pattern.</summary>
public sealed class ListJitProviderConfigsQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListJitProviderConfigsQuery, Result<PagedResult<JitProviderConfigDto>>>
{
    public async Task<Result<PagedResult<JitProviderConfigDto>>> Handle(
        ListJitProviderConfigsQuery query, CancellationToken ct)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var sql = """
            SELECT j.Id, j.Scheme, j.DisplayName, j.ProviderType, j.JitEnabled,
                   j.DefaultRoleName, j.RequireApproval, j.ClaimMappingJson,
                   j.DefaultTenantId, j.AllowedDomainsJson, j.MaxProvisionsPerHour,
                   j.IsActive,
                   COUNT(*) OVER() AS TotalCount
            FROM [identity].JitProviderConfigs j
            WHERE j.IsDeleted = 0
            ORDER BY j.DisplayName ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await db.QueryAsync<JitProviderConfigRow>(sql, new
        {
            Offset = offset,
            query.PageSize
        });

        var list = rows.ToList();
        var total = list.Count > 0 ? list[0].TotalCount : 0;

        var dtos = list.Select(r => new JitProviderConfigDto
        {
            Id = r.Id,
            Scheme = r.Scheme,
            DisplayName = r.DisplayName,
            ProviderType = r.ProviderType,
            JitEnabled = r.JitEnabled,
            DefaultRoleName = r.DefaultRoleName,
            RequireApproval = r.RequireApproval,
            ClaimMappingJson = r.ClaimMappingJson,
            DefaultTenantId = r.DefaultTenantId,
            AllowedDomainsJson = r.AllowedDomainsJson,
            MaxProvisionsPerHour = r.MaxProvisionsPerHour,
            IsActive = r.IsActive
        }).ToList();

        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)total / query.PageSize) : 0;
        var meta = new PaginationMeta(query.Page, query.PageSize, totalPages, null, null, query.Page < totalPages);
        return Result.Ok(new PagedResult<JitProviderConfigDto>(dtos, total, meta));
    }
}

/// <summary>Dapper row projection — class required (not positional record) for nullable Dapper mapping.</summary>
internal sealed class JitProviderConfigRow
{
    public Guid Id { get; set; }
    public string Scheme { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int ProviderType { get; set; }
    public bool JitEnabled { get; set; }
    public string DefaultRoleName { get; set; } = string.Empty;
    public bool RequireApproval { get; set; }
    public string? ClaimMappingJson { get; set; }
    public Guid? DefaultTenantId { get; set; }
    public string? AllowedDomainsJson { get; set; }
    public int MaxProvisionsPerHour { get; set; }
    public bool IsActive { get; set; }
    public int TotalCount { get; set; }
}
