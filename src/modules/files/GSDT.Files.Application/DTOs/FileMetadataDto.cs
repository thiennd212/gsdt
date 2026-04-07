
namespace GSDT.Files.Application.DTOs;

/// <summary>File metadata response DTO — safe to return to clients (no storage internals).</summary>
public sealed record FileMetadataDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string ChecksumSha256,
    FileStatus Status,
    Guid TenantId,
    Guid UploadedBy,
    Guid? CaseId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
