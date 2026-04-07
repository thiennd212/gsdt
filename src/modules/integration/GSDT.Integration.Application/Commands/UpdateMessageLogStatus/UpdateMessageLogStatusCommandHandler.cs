using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.UpdateMessageLogStatus;

public sealed class UpdateMessageLogStatusCommandHandler(IMessageLogRepository repository)
    : IRequestHandler<UpdateMessageLogStatusCommand, Result<MessageLogDto>>
{
    public async Task<Result<MessageLogDto>> Handle(
        UpdateMessageLogStatusCommand request, CancellationToken cancellationToken)
    {
        var log = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (log is null)
            return Result.Fail(new NotFoundError($"MessageLog {request.Id} not found."));

        switch (request.NewStatus)
        {
            case MessageLogStatus.Delivered:   log.MarkDelivered(); break;
            case MessageLogStatus.Failed:      log.MarkFailed();    break;
            case MessageLogStatus.Acknowledged: log.Acknowledge();  break;
            default:
                return Result.Fail(new ValidationError($"Cannot transition to {request.NewStatus}."));
        }

        await repository.UpdateAsync(log, cancellationToken);

        return Result.Ok(new MessageLogDto(
            log.Id, log.TenantId, log.PartnerId, log.ContractId,
            log.Direction.ToString(), log.MessageType, log.Payload,
            log.Status.ToString(), log.CorrelationId,
            log.SentAt, log.AcknowledgedAt, log.CreatedAt, log.UpdatedAt));
    }
}
