using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.ReviewAiOutput;

public sealed class ReviewAiOutputCommandHandler(IAiOutputReviewRepository repository)
    : IRequestHandler<ReviewAiOutputCommand, Result>
{
    public async Task<Result> Handle(
        ReviewAiOutputCommand request,
        CancellationToken cancellationToken)
    {
        var review = await repository.GetByPromptTraceIdAsync(request.PromptTraceId, cancellationToken);

        if (review is null)
        {
            // Create a new review record if one does not exist yet
            review = AiOutputReview.Create(request.PromptTraceId);
            await repository.AddAsync(review, cancellationToken);
        }

        review.Submit(request.ReviewerId, request.Decision, request.Reason);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
