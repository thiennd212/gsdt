using FluentResults;
using MediatR;

namespace GSDT.MasterData.Infrastructure.CommandHandlers;

public sealed class UpdateDictionaryCommandHandler(MasterDataDbContext db)
    : IRequestHandler<UpdateDictionaryCommand, Result>
{
    public async Task<Result> Handle(UpdateDictionaryCommand cmd, CancellationToken ct)
    {
        var dict = await db.Dictionaries.FirstOrDefaultAsync(d => d.Id == cmd.Id, ct);
        if (dict is null)
            return Result.Fail(new NotFoundError($"Dictionary {cmd.Id} not found"));

        dict.Update(cmd.Name, cmd.NameVi, cmd.Description, cmd.ActorId);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
