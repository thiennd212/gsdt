using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.MasterData.Application.Queries.GetExternalMappings;

/// <summary>Read-side query via Dapper.</summary>
public sealed class GetExternalMappingsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetExternalMappingsQuery, Result<PagedResult<ExternalMappingDto>>>
{
    public async Task<Result<PagedResult<ExternalMappingDto>>> Handle(
        GetExternalMappingsQuery query, CancellationToken ct)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var sql = """
            SELECT m.Id, m.InternalCode, m.ExternalSystem, m.ExternalCode,
                   m.Direction, m.DictionaryId, m.IsActive, m.ValidFrom, m.ValidTo, m.TenantId,
                   COUNT(*) OVER() AS TotalCount
            FROM [masterdata].ExternalMappings m
            WHERE m.TenantId = @TenantId
              AND m.IsDeleted = 0
              AND (@ExternalSystem IS NULL OR m.ExternalSystem = @ExternalSystem)
              AND (@InternalCode IS NULL OR m.InternalCode = @InternalCode)
              AND (@ActiveOnly = 0 OR m.IsActive = 1)
            ORDER BY m.ExternalSystem ASC, m.InternalCode ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await db.QueryAsync<ExternalMappingRow>(sql, new
        {
            query.TenantId,
            query.ExternalSystem,
            query.InternalCode,
            ActiveOnly = query.ActiveOnly ? 1 : 0,
            Offset = offset,
            query.PageSize
        });

        var list = rows.ToList();
        var total = list.Count > 0 ? list[0].TotalCount : 0;

        var dtos = list.Select(r => new ExternalMappingDto(
            r.Id, r.InternalCode, r.ExternalSystem, r.ExternalCode,
            (MappingDirection)r.Direction, r.DictionaryId,
            r.IsActive, r.ValidFrom, r.ValidTo, r.TenantId)).ToList();

        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)total / query.PageSize) : 0;
        var meta = new PaginationMeta(query.Page, query.PageSize, totalPages, null, null, query.Page < totalPages);
        return Result.Ok(new PagedResult<ExternalMappingDto>(dtos, total, meta));
    }
}

internal sealed record ExternalMappingRow(
    Guid Id, string InternalCode, string ExternalSystem, string ExternalCode,
    int Direction, Guid? DictionaryId, bool IsActive,
    DateTime ValidFrom, DateTime? ValidTo, Guid TenantId, int TotalCount);
