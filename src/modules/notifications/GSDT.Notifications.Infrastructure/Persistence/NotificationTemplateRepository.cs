using FluentResults;

namespace GSDT.Notifications.Infrastructure.Persistence;

public sealed class NotificationTemplateRepository(NotificationsDbContext dbContext)
    : INotificationTemplateRepository
{
    public async Task<Result<NotificationTemplate>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        return entity is null
            ? Result.Fail(new NotFoundError($"NotificationTemplate {id} not found."))
            : Result.Ok(entity);
    }

    public async Task AddAsync(NotificationTemplate entity, CancellationToken cancellationToken = default)
    {
        await dbContext.NotificationTemplates.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(NotificationTemplate entity, CancellationToken cancellationToken = default)
    {
        dbContext.NotificationTemplates.Update(entity);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null) return;
        entity.Delete();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationTemplate?> FindByKeyAsync(
        string templateKey, string channel, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // EF Core cannot translate Channel.Value or .ToLowerInvariant() inline.
        // Pre-compute the comparison value and use the converted column directly.
        var channelLower = channel.ToLowerInvariant();
        return await dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t =>
                t.TemplateKey == templateKey &&
                t.Channel == Domain.ValueObjects.NotificationChannel.From(channelLower) &&
                t.TenantId == tenantId &&
                !t.IsDeleted,
                cancellationToken);
    }
}
