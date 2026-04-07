
namespace GSDT.Files.Application.Commands.DeleteFile;

/// <summary>Soft-delete a file record. Storage object retained for audit; logical access removed.</summary>
public sealed record DeleteFileCommand(Guid FileId, Guid TenantId, Guid DeletedBy) : ICommand;
