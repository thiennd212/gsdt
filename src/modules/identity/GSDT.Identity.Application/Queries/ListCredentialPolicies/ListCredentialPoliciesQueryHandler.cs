using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListCredentialPolicies;

/// <summary>Read-side query via Dapper — follows ListUsersQueryHandler pattern.</summary>
public sealed class ListCredentialPoliciesQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListCredentialPoliciesQuery, Result<PagedResult<CredentialPolicyDto>>>
{
    public async Task<Result<PagedResult<CredentialPolicyDto>>> Handle(
        ListCredentialPoliciesQuery query, CancellationToken ct)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var sql = """
            SELECT p.Id, p.Name, p.TenantId,
                   p.MinLength, p.MaxLength,
                   p.RequireUppercase, p.RequireLowercase,
                   p.RequireDigit, p.RequireSpecialChar,
                   p.RotationDays, p.MaxFailedAttempts,
                   p.LockoutMinutes, p.PasswordHistoryCount, p.IsDefault,
                   COUNT(*) OVER() AS TotalCount
            FROM [identity].CredentialPolicies p
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
            ORDER BY p.IsDefault DESC, p.Name ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await db.QueryAsync<CredentialPolicyRow>(sql, new
        {
            query.TenantId,
            Offset = offset,
            query.PageSize
        });

        var list = rows.ToList();
        var total = list.Count > 0 ? list[0].TotalCount : 0;

        var dtos = list.Select(r => new CredentialPolicyDto(
            r.Id, r.Name, r.TenantId,
            r.MinLength, r.MaxLength,
            r.RequireUppercase, r.RequireLowercase,
            r.RequireDigit, r.RequireSpecialChar,
            r.RotationDays, r.MaxFailedAttempts,
            r.LockoutMinutes, r.PasswordHistoryCount,
            r.IsDefault)).ToList();

        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)total / query.PageSize) : 0;
        var meta = new PaginationMeta(query.Page, query.PageSize, totalPages, null, null, query.Page < totalPages);
        return Result.Ok(new PagedResult<CredentialPolicyDto>(dtos, total, meta));
    }
}

internal sealed record CredentialPolicyRow(
    Guid Id, string Name, Guid TenantId,
    int MinLength, int MaxLength,
    bool RequireUppercase, bool RequireLowercase,
    bool RequireDigit, bool RequireSpecialChar,
    int RotationDays, int MaxFailedAttempts,
    int LockoutMinutes, int PasswordHistoryCount,
    bool IsDefault, int TotalCount);
