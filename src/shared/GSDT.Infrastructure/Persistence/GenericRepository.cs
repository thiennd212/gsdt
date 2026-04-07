using FluentResults;

namespace GSDT.Infrastructure.Persistence;

/// <summary>
/// Generic EF Core repository — base implementation for all module aggregate repositories.
/// Modules extend this for aggregate-specific queries:
///   public sealed class NotificationRepository(NotificationsDbContext ctx)
///       : GenericRepository&lt;Notification, Guid&gt;(ctx), INotificationRepository
///
/// SaveChanges is called per-operation here. For unit-of-work patterns spanning multiple
/// aggregates, inject the DbContext directly in the command handler (Phase 02 guidance).
/// </summary>
public class GenericRepository<T, TId>(ModuleDbContext context) : IRepository<T, TId>
    where T : class
{
    protected readonly ModuleDbContext Context = context;

    public virtual async Task<Result<T>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<T>().FindAsync([id], cancellationToken);
        return entity is null
            ? Result.Fail(new NotFoundError($"{typeof(T).Name} with id '{id}' not found."))
            : Result.Ok(entity);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Only explicitly attach when detached — if the entity is already tracked (loaded via
        // a tracking query in the same scope), calling Update() forces ALL related entities to
        // Modified state, which causes DbUpdateConcurrencyException when new child entities
        // (with non-default Guid keys) haven't been persisted yet.
        if (Context.Entry(entity).State == EntityState.Detached)
            Context.Set<T>().Update(entity);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<T>().FindAsync([id], cancellationToken);
        if (entity is null) return;

        // SoftDeleteInterceptor converts EntityState.Deleted → IsDeleted = true automatically
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
