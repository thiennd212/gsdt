using FluentResults;

namespace GSDT.Identity.Application.Commands.DecideAccessReview;

/// <summary>Approve or reject a pending access review record (QĐ742).</summary>
public sealed record DecideAccessReviewCommand(
    Guid ReviewId,
    AccessReviewDecision Decision,
    Guid ActorId) : ICommand;
