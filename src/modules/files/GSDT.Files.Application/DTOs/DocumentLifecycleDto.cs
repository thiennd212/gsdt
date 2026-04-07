
namespace GSDT.Files.Application.DTOs;

/// <summary>DTO for FileVersion read operations.</summary>
public sealed record FileVersionDto(
    Guid Id,
    Guid FileRecordId,
    int VersionNumber,
    string StoragePath,
    long FileSize,
    string ContentHash,
    Guid UploadedBy,
    DateTime UploadedAt,
    string? Comment,
    Guid TenantId);

/// <summary>DTO for DocumentTemplate read operations.</summary>
public sealed record DocumentTemplateDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    DocumentOutputFormat OutputFormat,
    string TemplateContent,
    DocumentTemplateStatus Status,
    Guid TenantId,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

/// <summary>DTO for RetentionPolicy read operations.</summary>
public sealed record RetentionPolicyDto(
    Guid Id,
    string Name,
    string DocumentType,
    int RetainDays,
    int? ArchiveAfterDays,
    int? DestroyAfterDays,
    bool IsActive,
    Guid TenantId);

/// <summary>DTO for RecordLifecycle read operations.</summary>
public sealed record RecordLifecycleDto(
    Guid Id,
    Guid FileRecordId,
    RecordLifecycleStatus CurrentStatus,
    Guid? RetentionPolicyId,
    DateTime? ArchivedAt,
    DateTime? ScheduledDestroyAt,
    DateTime? DestroyedAt,
    Guid? DestroyedBy);
