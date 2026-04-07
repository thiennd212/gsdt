
namespace GSDT.Files.Domain.Repositories;

/// <summary>File aggregate repository — extends generic repo with file-specific queries.</summary>
public interface IFileRepository : IRepository<FileRecord, FileId>
{
    Task<FileRecord?> FindByStorageKeyAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileRecord>> GetByStatusAsync(FileStatus status, CancellationToken cancellationToken = default);
}
