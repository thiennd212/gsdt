using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListExternalIdentities;

/// <summary>Read-side query via Dapper — follows ListUsersQueryHandler pattern.</summary>
public sealed class ListExternalIdentitiesQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListExternalIdentitiesQuery, Result<PagedResult<ExternalIdentityDto>>>
{
    public async Task<Result<PagedResult<ExternalIdentityDto>>> Handle(
        ListExternalIdentitiesQuery query, CancellationToken ct)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var sql = """
            SELECT e.Id, e.UserId, e.Provider, e.ExternalId, e.DisplayName, e.Email,
                   e.LinkedAt, e.LastSyncAt, e.IsActive,
                   COUNT(*) OVER() AS TotalCount
            FROM [identity].ExternalIdentities e
            WHERE e.UserId = @UserId
              AND e.IsDeleted = 0
            ORDER BY e.LinkedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await db.QueryAsync<ExternalIdentityRow>(sql, new
        {
            query.UserId,
            Offset = offset,
            query.PageSize
        });

        var list = rows.ToList();
        var total = list.Count > 0 ? list[0].TotalCount : 0;

        var dtos = list.Select(r => new ExternalIdentityDto(
            r.Id, r.UserId, (ExternalIdentityProvider)r.Provider,
            r.ExternalId, r.DisplayName, r.Email,
            r.LinkedAt, r.LastSyncAt, r.IsActive)).ToList();

        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)total / query.PageSize) : 0;
        var meta = new PaginationMeta(query.Page, query.PageSize, totalPages, null, null, query.Page < totalPages);
        return Result.Ok(new PagedResult<ExternalIdentityDto>(dtos, total, meta));
    }
}

internal sealed record ExternalIdentityRow(
    Guid Id, Guid UserId, int Provider, string ExternalId,
    string? DisplayName, string? Email,
    DateTime LinkedAt, DateTime? LastSyncAt, bool IsActive, int TotalCount);
