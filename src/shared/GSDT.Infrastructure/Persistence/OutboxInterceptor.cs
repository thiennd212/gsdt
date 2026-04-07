using System.Text.Json;

namespace GSDT.Infrastructure.Persistence;

/// <summary>
/// EF SaveChanges interceptor — collects IExternalDomainEvent from aggregates and persists
/// them as OutboxMessages in the same transaction.
/// External events use Outbox for at-least-once delivery via MassTransit (Phase 02c).
/// Stateless — registered as singleton.
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await CollectOutboxMessagesAsync(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            CollectOutboxMessages(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private static async Task CollectOutboxMessagesAsync(
        Microsoft.EntityFrameworkCore.DbContext context,
        CancellationToken cancellationToken)
    {
        var messages = BuildOutboxMessages(context);
        if (messages.Count > 0)
            await context.Set<OutboxMessage>().AddRangeAsync(messages, cancellationToken);
    }

    private static void CollectOutboxMessages(Microsoft.EntityFrameworkCore.DbContext context)
    {
        var messages = BuildOutboxMessages(context);
        if (messages.Count > 0)
            context.Set<OutboxMessage>().AddRange(messages);
    }

    private static List<OutboxMessage> BuildOutboxMessages(Microsoft.EntityFrameworkCore.DbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var messages = aggregates
            .SelectMany(a => a.DomainEvents.OfType<IExternalDomainEvent>())
            .Select(evt => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = evt.GetType().AssemblyQualifiedName ?? evt.GetType().FullName!,
                Payload = JsonSerializer.Serialize(evt, evt.GetType(), SerializerOptions),
                CreatedAtUtc = DateTimeOffset.UtcNow
            })
            .ToList();

        // Only clear external events (collected into outbox). Preserve internal events
        foreach (var aggregate in aggregates)
        {
            var internalEvents = aggregate.DomainEvents
                .Where(e => e is not IExternalDomainEvent).ToList();
            aggregate.ClearDomainEvents();
            foreach (var ie in internalEvents)
                aggregate.AddDomainEvent(ie);
        }

        return messages;
    }
}
