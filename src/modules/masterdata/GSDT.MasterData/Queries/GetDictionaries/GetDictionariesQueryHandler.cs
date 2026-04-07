using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.MasterData.Queries.GetDictionaries;

/// <summary>Read-side query via Dapper — follows project read-side patterns.</summary>
public sealed class GetDictionariesQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetDictionariesQuery, Result<PagedResult<DictionaryDto>>>
{
    public async Task<Result<PagedResult<DictionaryDto>>> Handle(
        GetDictionariesQuery query, CancellationToken ct)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var sql = """
            SELECT d.Id, d.Code, d.Name, d.NameVi, d.Description,
                   d.Status, d.CurrentVersion, d.TenantId, d.IsSystemDefined,
                   COUNT(*) OVER() AS TotalCount
            FROM [masterdata].Dictionaries d
            WHERE d.TenantId = @TenantId
              AND d.IsDeleted = 0
              AND (@Status IS NULL OR d.Status = @Status)
              AND (@Search IS NULL OR d.Name LIKE @Search ESCAPE '\'
                                   OR d.Code LIKE @Search ESCAPE '\')
            ORDER BY d.Code ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await db.QueryAsync<DictionaryRow>(sql, new
        {
            query.TenantId,
            Status = (int?)query.Status,
            Search = query.Search is not null ? $"%{query.Search}%" : null,
            Offset = offset,
            query.PageSize
        });

        var list = rows.ToList();
        var total = list.Count > 0 ? list[0].TotalCount : 0;

        var dtos = list.Select(r => new DictionaryDto(
            r.Id, r.Code, r.Name, r.NameVi, r.Description,
            (DictionaryStatus)r.Status, r.CurrentVersion, r.TenantId, r.IsSystemDefined)).ToList();

        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)total / query.PageSize) : 0;
        var meta = new PaginationMeta(query.Page, query.PageSize, totalPages, null, null, query.Page < totalPages);
        return Result.Ok(new PagedResult<DictionaryDto>(dtos, total, meta));
    }
}

internal sealed record DictionaryRow(
    Guid Id, string Code, string Name, string NameVi, string? Description,
    int Status, int CurrentVersion, Guid TenantId, bool IsSystemDefined, int TotalCount);
