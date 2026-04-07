using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.MasterData.Application.Queries.GetDictionaryItems;

/// <summary>
/// Returns flat list of DictionaryItems ordered by SortOrder.
/// Clients build the tree using ParentId — avoids recursive CTE complexity in read queries.
/// </summary>
public sealed class GetDictionaryItemsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetDictionaryItemsQuery, Result<PagedResult<DictionaryItemDto>>>
{
    public async Task<Result<PagedResult<DictionaryItemDto>>> Handle(
        GetDictionaryItemsQuery query, CancellationToken ct)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var sql = """
            SELECT i.Id, i.DictionaryId, i.Code, i.Name, i.NameVi,
                   i.ParentId, i.SortOrder, i.EffectiveFrom, i.EffectiveTo, i.IsActive,
                   COUNT(*) OVER() AS TotalCount
            FROM [masterdata].DictionaryItems i
            WHERE i.DictionaryId = @DictionaryId
              AND i.IsDeleted = 0
              AND (@ActiveOnly = 0 OR i.IsActive = 1)
            ORDER BY i.SortOrder ASC, i.Code ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await db.QueryAsync<DictionaryItemRow>(sql, new
        {
            query.DictionaryId,
            ActiveOnly = query.ActiveOnly ? 1 : 0,
            Offset = offset,
            query.PageSize
        });

        var list = rows.ToList();
        var total = list.Count > 0 ? list[0].TotalCount : 0;

        var dtos = list.Select(r => new DictionaryItemDto(
            r.Id, r.DictionaryId, r.Code, r.Name, r.NameVi,
            r.ParentId, r.SortOrder, r.EffectiveFrom, r.EffectiveTo, r.IsActive)).ToList();

        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)total / query.PageSize) : 0;
        var meta = new PaginationMeta(query.Page, query.PageSize, totalPages, null, null, query.Page < totalPages);
        return Result.Ok(new PagedResult<DictionaryItemDto>(dtos, total, meta));
    }
}

internal sealed record DictionaryItemRow(
    Guid Id, Guid DictionaryId, string Code, string Name, string NameVi,
    Guid? ParentId, int SortOrder, DateTime EffectiveFrom,
    DateTime? EffectiveTo, bool IsActive, int TotalCount);
