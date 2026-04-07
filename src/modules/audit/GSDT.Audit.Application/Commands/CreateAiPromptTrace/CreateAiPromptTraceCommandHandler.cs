using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.CreateAiPromptTrace;

public sealed class CreateAiPromptTraceCommandHandler(IAiPromptTraceRepository repository)
    : IRequestHandler<CreateAiPromptTraceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateAiPromptTraceCommand request,
        CancellationToken cancellationToken)
    {
        var trace = AiPromptTrace.Create(
            request.SessionId,
            request.ModelName,
            request.PromptHash,
            request.InputTokens,
            request.OutputTokens,
            request.LatencyMs,
            request.Cost,
            request.ClassificationLevel,
            request.TenantId,
            request.ModelProfileId,
            request.PromptText,
            request.ResponseHash);

        await repository.AddAsync(trace, cancellationToken);
        return Result.Ok(trace.Id);
    }
}
