
namespace GSDT.Audit.Domain.Entities;

/// <summary>
/// Human review record for an AI-generated output — M15 AI Governance.
/// Linked to AiPromptTrace via PromptTraceId.
/// Starts as Pending; reviewer calls Submit() to set Decision.
/// </summary>
public sealed class AiOutputReview : AuditableEntity<Guid>
{
    public Guid PromptTraceId { get; private set; }
    public Guid? ReviewerId { get; private set; }
    public ReviewDecision Decision { get; private set; } = ReviewDecision.Pending;
    public string? Reason { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    private AiOutputReview() { }

    public static AiOutputReview Create(Guid promptTraceId)
    {
        return new AiOutputReview
        {
            Id = Guid.NewGuid(),
            PromptTraceId = promptTraceId,
            Decision = ReviewDecision.Pending
        };
    }

    /// <summary>Submits a review decision. Transitions from Pending to any other state.</summary>
    public void Submit(Guid reviewerId, ReviewDecision decision, string? reason)
    {
        ReviewerId = reviewerId;
        Decision = decision;
        Reason = reason;
        ReviewedAt = DateTime.UtcNow;
    }
}
