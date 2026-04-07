
namespace GSDT.Audit.Domain.Entities;

/// <summary>
/// Append-only record of a single AI prompt/response cycle — M15 AI Governance.
/// Captures tokens, latency, cost, and classification for GOV compliance audit trail.
/// PromptText is stored encrypted at application level; never exposed in DTOs.
/// </summary>
public sealed class AiPromptTrace : AuditableEntity<Guid>, IAggregateRoot, ITenantScoped
{
    public Guid SessionId { get; private set; }
    public Guid? ModelProfileId { get; private set; }
    public string ModelName { get; private set; } = string.Empty;

    /// <summary>SHA-256 of prompt content — for deduplication without storing raw text.</summary>
    public string PromptHash { get; private set; } = string.Empty;

    /// <summary>Encrypted prompt text — null if encryption is not configured.</summary>
    public string? PromptText { get; private set; }

    public string? ResponseHash { get; private set; }
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public int LatencyMs { get; private set; }
    public decimal Cost { get; private set; }

    /// <summary>Data classification per QĐ742: Public | Internal | Confidential | Secret | TopSecret.</summary>
    public new string ClassificationLevel { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    // IAggregateRoot support
    public void ClearDomainEvents() => _domainEvents.Clear();

    private AiPromptTrace() { }

    public static AiPromptTrace Create(
        Guid sessionId,
        string modelName,
        string promptHash,
        int inputTokens,
        int outputTokens,
        int latencyMs,
        decimal cost,
        string classificationLevel,
        Guid tenantId,
        Guid? modelProfileId = null,
        string? promptText = null,
        string? responseHash = null)
    {
        return new AiPromptTrace
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            ModelName = modelName,
            PromptHash = promptHash,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            LatencyMs = latencyMs,
            Cost = cost,
            ClassificationLevel = classificationLevel,
            TenantId = tenantId,
            ModelProfileId = modelProfileId,
            PromptText = promptText,
            ResponseHash = responseHash
        };
    }
}
