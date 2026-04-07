
namespace GSDT.Audit.Application.DTOs;

/// <summary>Read model for AiOutputReview.</summary>
public sealed record AiOutputReviewDto(
    Guid Id,
    Guid PromptTraceId,
    Guid? ReviewerId,
    ReviewDecision Decision,
    string? Reason,
    DateTime? ReviewedAt,
    DateTimeOffset CreatedAt);
