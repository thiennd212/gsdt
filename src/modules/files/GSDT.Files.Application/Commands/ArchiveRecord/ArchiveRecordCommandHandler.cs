using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.ArchiveRecord;

public sealed class ArchiveRecordCommandHandler(
    IRepository<RecordLifecycle, Guid> lifecycleRepository)
    : IRequestHandler<ArchiveRecordCommand, Result<RecordLifecycleDto>>
{
    public async Task<Result<RecordLifecycleDto>> Handle(
        ArchiveRecordCommand request,
        CancellationToken cancellationToken)
    {
        // Create or update RecordLifecycle — this command always creates a new lifecycle entry
        // (enforcement job handles update of existing lifecycle)
        var lifecycle = RecordLifecycle.Create(
            request.FileRecordId,
            null,
            request.ArchivedBy);
        lifecycle.Archive(request.ArchivedBy);

        await lifecycleRepository.AddAsync(lifecycle, cancellationToken);

        return Result.Ok(MapToDto(lifecycle));
    }

    internal static RecordLifecycleDto MapToDto(RecordLifecycle l) => new(
        l.Id, l.FileRecordId, l.CurrentStatus, l.RetentionPolicyId,
        l.ArchivedAt, l.ScheduledDestroyAt, l.DestroyedAt, l.DestroyedBy);
}
