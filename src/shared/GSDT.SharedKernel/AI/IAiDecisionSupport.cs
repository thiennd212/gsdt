using FluentResults;

namespace GSDT.SharedKernel.AI;

/// <summary>
/// AI decision support — suggests next action for gov officers based on entity context.
/// Stub default: returns empty recommendation with Confidence=0.
/// </summary>
public interface IAiDecisionSupport
{
    Task<Result<DecisionSupportResponse>> SuggestAsync(
        DecisionSupportRequest request,
        CancellationToken ct = default);
}
