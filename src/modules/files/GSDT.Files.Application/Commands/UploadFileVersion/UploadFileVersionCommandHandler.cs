using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.UploadFileVersion;

public sealed class UploadFileVersionCommandHandler(
    IRepository<FileVersion, Guid> repository,
    IReadDbConnection db)
    : IRequestHandler<UploadFileVersionCommand, Result<FileVersionDto>>
{
    public async Task<Result<FileVersionDto>> Handle(
        UploadFileVersionCommand request,
        CancellationToken cancellationToken)
    {
        // Get next version number for this file record
        const string sql = """
            SELECT ISNULL(MAX(VersionNumber), 0) + 1
            FROM files.FileVersions
            WHERE FileRecordId = @FileRecordId
            """;

        var nextVersion = await db.QuerySingleAsync<int>(
            sql, new { request.FileRecordId }, cancellationToken);

        var version = FileVersion.Create(
            request.FileRecordId,
            nextVersion,
            request.StoragePath,
            request.FileSize,
            request.ContentHash,
            request.UploadedBy,
            request.TenantId,
            request.Comment);

        await repository.AddAsync(version, cancellationToken);

        return Result.Ok(MapToDto(version));
    }

    internal static FileVersionDto MapToDto(FileVersion v) => new(
        v.Id, v.FileRecordId, v.VersionNumber, v.StoragePath,
        v.FileSize, v.ContentHash, v.UploadedBy, v.UploadedAt,
        v.Comment, v.TenantId);
}
