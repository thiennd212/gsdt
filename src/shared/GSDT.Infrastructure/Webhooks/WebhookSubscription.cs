using System.Text.Json;

namespace GSDT.Infrastructure.Webhooks;

/// <summary>
/// Persisted webhook subscription entity (schema: webhooks).
/// SecretHash: HMAC-SHA256(secret) — plaintext never stored.
/// EventTypes: JSON array of event type strings.
/// </summary>
public sealed class WebhookSubscription
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    /// <summary>Webhook target URL — internal infrastructure reference, not publicly exposed.</summary>
    [DataClassification(DataClassificationLevel.Internal)]
    public string EndpointUrl { get; private set; } = string.Empty;

    /// <summary>HMAC-SHA256 hex hash of the subscription secret. Plaintext never persisted.</summary>
    [DataClassification(DataClassificationLevel.Restricted)]
    public string SecretHash { get; private set; } = string.Empty;

    /// <summary>JSON array: ["case.approved","case.rejected"]. Stored as nvarchar(max).</summary>
    public string EventTypesJson { get; private set; } = "[]";

    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    // EF Core parameterless ctor
    private WebhookSubscription() { }

    public static WebhookSubscription Create(
        Guid tenantId,
        string endpointUrl,
        IReadOnlyList<string> eventTypes,
        string secretHash)
    {
        return new WebhookSubscription
        {
            TenantId = tenantId,
            EndpointUrl = endpointUrl,
            SecretHash = secretHash,
            EventTypesJson = JsonSerializer.Serialize(eventTypes)
        };
    }

    public IReadOnlyList<string> GetEventTypes()
        => JsonSerializer.Deserialize<List<string>>(EventTypesJson) ?? [];

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
