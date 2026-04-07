
namespace GSDT.Files.Application.Queries.DownloadFile;

/// <summary>Download file stream from storage. Returns stream + content metadata for response headers.</summary>
public sealed record DownloadFileQuery(Guid FileId, Guid TenantId, Guid RequestedBy) : IQuery<DownloadFileResult>;

public sealed record DownloadFileResult(
    Stream FileStream,
    string OriginalFileName,
    string ContentType,
    long SizeBytes);
