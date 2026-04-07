using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Queries.GetDocumentTemplates;

public sealed class GetDocumentTemplatesQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetDocumentTemplatesQuery, Result<PagedResult<DocumentTemplateDto>>>
{
    public async Task<Result<PagedResult<DocumentTemplateDto>>> Handle(
        GetDocumentTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var clauses = new List<string>
        {
            "TenantId = @TenantId",
            "IsDeleted = 0"
        };
        var p = new DynamicParameters();
        p.Add("TenantId", request.TenantId);

        if (request.Status.HasValue)
        {
            clauses.Add("Status = @Status");
            p.Add("Status", request.Status.Value);
        }

        // Optional substring search across Name and Description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(Name LIKE @SearchPattern ESCAPE '\\' OR Description LIKE @SearchPattern ESCAPE '\\')");
        }

        var whereClause = string.Join(" AND ", clauses);
        var offset = (Math.Max(request.Page, 1) - 1) * request.PageSize;
        p.Add("Offset", offset);
        p.Add("PageSize", request.PageSize);

        var countSql = $"SELECT COUNT(*) FROM files.DocumentTemplates WHERE {whereClause}";
        var dataSql = $"""
            SELECT Id, Name, Code, Description, OutputFormat, TemplateContent,
                   Status, TenantId, CreatedAt, CreatedBy
            FROM files.DocumentTemplates
            WHERE {whereClause}
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await db.QuerySingleAsync<int>(countSql, p, cancellationToken);
        var rows = await db.QueryAsync<TemplateRow>(dataSql, p, cancellationToken);

        var items = rows
            .Select(r => new DocumentTemplateDto(
                r.Id, r.Name, r.Code, r.Description,
                (DocumentOutputFormat)r.OutputFormat,
                r.TemplateContent,
                (DocumentTemplateStatus)r.Status,
                r.TenantId, r.CreatedAt, r.CreatedBy))
            .ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<DocumentTemplateDto>(items, total, meta));
    }

    private sealed record TemplateRow(
        Guid Id, string Name, string Code, string? Description,
        int OutputFormat, string TemplateContent, int Status,
        Guid TenantId, DateTimeOffset CreatedAt, Guid CreatedBy);
}
