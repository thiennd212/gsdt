
namespace GSDT.Integration.Infrastructure.Repositories;

public sealed class MessageLogRepository(IntegrationDbContext dbContext) : IMessageLogRepository
{
    public async Task<MessageLog?> GetByIdAsync(Guid id, CancellationToken ct)
        => await dbContext.MessageLogs.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(MessageLog log, CancellationToken ct)
    {
        dbContext.MessageLogs.Add(log);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MessageLog log, CancellationToken ct)
    {
        if (dbContext.Entry(log).State == EntityState.Detached)
            dbContext.MessageLogs.Update(log);
        await dbContext.SaveChangesAsync(ct);
    }
}
