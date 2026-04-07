
namespace GSDT.Files.Infrastructure.Persistence;

public sealed class FileRepository(FilesDbContext context)
    : GenericRepository<FileRecord, FileId>(context), IFileRepository
{
    private FilesDbContext FilesContext => (FilesDbContext)Context;

    public async Task<FileRecord?> FindByStorageKeyAsync(
        string storageKey,
        CancellationToken cancellationToken = default) =>
        await FilesContext.FileRecords
            .FirstOrDefaultAsync(f => f.StorageKey == storageKey, cancellationToken);

    public async Task<IReadOnlyList<FileRecord>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default) =>
        await FilesContext.FileRecords
            .Where(f => f.Status == status)
            .ToListAsync(cancellationToken);
}
