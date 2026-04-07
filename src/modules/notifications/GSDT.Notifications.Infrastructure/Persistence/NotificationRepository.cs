using FluentResults;

namespace GSDT.Notifications.Infrastructure.Persistence;

public sealed class NotificationRepository(NotificationsDbContext dbContext) : INotificationRepository
{
    public async Task<Result<Notification>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, cancellationToken);

        return entity is null
            ? Result.Fail(new NotFoundError($"Notification {id} not found."))
            : Result.Ok(entity);
    }

    public async Task AddAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Notifications.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        dbContext.Notifications.Update(entity);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (entity is null) return;
        entity.Delete(); // soft delete via Entity base
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Notifications
            .Where(n => n.RecipientUserId == userId
                     && n.TenantId == tenantId
                     && !n.IsRead
                     && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
