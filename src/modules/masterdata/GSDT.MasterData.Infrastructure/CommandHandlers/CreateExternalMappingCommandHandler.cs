using FluentResults;
using MediatR;

namespace GSDT.MasterData.Infrastructure.CommandHandlers;

public sealed class CreateExternalMappingCommandHandler(MasterDataDbContext db)
    : IRequestHandler<CreateExternalMappingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExternalMappingCommand cmd, CancellationToken ct)
    {
        if (cmd.DictionaryId.HasValue)
        {
            var dictExists = await db.Dictionaries.AnyAsync(d => d.Id == cmd.DictionaryId.Value, ct);
            if (!dictExists)
                return Result.Fail(new NotFoundError($"Dictionary {cmd.DictionaryId} not found"));
        }

        var mapping = ExternalMapping.Create(
            cmd.InternalCode, cmd.ExternalSystem, cmd.ExternalCode,
            cmd.Direction, cmd.DictionaryId,
            cmd.ValidFrom, cmd.ValidTo,
            cmd.TenantId, cmd.ActorId);

        db.ExternalMappings.Add(mapping);
        await db.SaveChangesAsync(ct);
        return Result.Ok(mapping.Id);
    }
}
