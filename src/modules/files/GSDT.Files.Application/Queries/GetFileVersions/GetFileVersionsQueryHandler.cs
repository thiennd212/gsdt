using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Queries.GetFileVersions;

public sealed class GetFileVersionsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetFileVersionsQuery, Result<IReadOnlyList<FileVersionDto>>>
{
    public async Task<Result<IReadOnlyList<FileVersionDto>>> Handle(
        GetFileVersionsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, FileRecordId, VersionNumber, StoragePath, FileSize,
                   ContentHash, UploadedBy, UploadedAt, Comment, TenantId
            FROM files.FileVersions
            WHERE FileRecordId = @FileRecordId
              AND TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY VersionNumber
            """;

        var rows = await db.QueryAsync<FileVersionDto>(
            sql, new { request.FileRecordId, request.TenantId }, cancellationToken);

        return Result.Ok<IReadOnlyList<FileVersionDto>>(rows.ToList());
    }
}
