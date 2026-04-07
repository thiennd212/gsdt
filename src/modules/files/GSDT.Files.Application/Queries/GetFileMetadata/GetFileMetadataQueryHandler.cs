using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Queries.GetFileMetadata;

public sealed class GetFileMetadataQueryHandler(
    IFileRepository fileRepository) : IRequestHandler<GetFileMetadataQuery, Result<FileMetadataDto>>
{
    public async Task<Result<FileMetadataDto>> Handle(
        GetFileMetadataQuery request,
        CancellationToken cancellationToken)
    {
        var fileResult = await fileRepository.GetByIdAsync(
            FileId.From(request.FileId), cancellationToken);

        if (fileResult.IsFailed)
            return Result.Fail(new NotFoundError($"File {request.FileId} not found."));

        var file = fileResult.Value;

        if (file.TenantId != request.TenantId)
            return Result.Fail(new ForbiddenError("File does not belong to tenant."));

        var dto = new FileMetadataDto(
            file.Id.Value,
            file.OriginalFileName,
            file.ContentType,
            file.SizeBytes,
            file.ChecksumSha256,
            file.Status,
            file.TenantId,
            file.UploadedBy,
            file.CaseId,
            file.CreatedAt,
            file.UpdatedAt);

        return Result.Ok(dto);
    }
}
