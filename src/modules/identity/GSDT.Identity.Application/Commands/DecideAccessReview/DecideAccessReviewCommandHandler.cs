using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.DecideAccessReview;

public sealed class DecideAccessReviewCommandHandler
    : IRequestHandler<DecideAccessReviewCommand, Result>
{
    private readonly IAccessReviewRepository _repo;

    public DecideAccessReviewCommandHandler(IAccessReviewRepository repo) => _repo = repo;

    public async Task<Result> Handle(DecideAccessReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _repo.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
            return Result.Fail(new NotFoundError($"AccessReview {request.ReviewId} not found."));

        if (review.Decision is not null)
            return Result.Fail(new ConflictError("Access review already decided."));

        review.Decision = request.Decision;
        review.ReviewedBy = request.ActorId;
        review.ReviewedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(review, cancellationToken);
        return Result.Ok();
    }
}
