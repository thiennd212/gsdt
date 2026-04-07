
namespace GSDT.Files.Infrastructure.Persistence;

/// <summary>
/// Thin generic repository adapter for Files module entities that don't need custom queries.
/// Passes FilesDbContext (a ModuleDbContext) to GenericRepository base class.
/// Used for FileVersion, DocumentTemplate, DocumentTemplateVersion, RetentionPolicy,
/// RecordLifecycle, DocumentArchive.
/// </summary>
public sealed class GenericFilesRepository<T>(FilesDbContext context)
    : GenericRepository<T, Guid>(context)
    where T : class;
