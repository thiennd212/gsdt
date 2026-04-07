using FluentResults;
using MediatR;

namespace GSDT.MasterData.Infrastructure.CommandHandlers;

public sealed class CreateDictionaryCommandHandler(MasterDataDbContext db)
    : IRequestHandler<CreateDictionaryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDictionaryCommand cmd, CancellationToken ct)
    {
        var exists = await db.Dictionaries
            .AnyAsync(d => d.Code == cmd.Code && d.TenantId == cmd.TenantId, ct);
        if (exists)
            return Result.Fail(new ConflictError(
                $"Dictionary with code '{cmd.Code}' already exists for this tenant"));

        var dict = Dictionary.Create(
            cmd.Code, cmd.Name, cmd.NameVi,
            cmd.Description, cmd.TenantId,
            cmd.IsSystemDefined, cmd.ActorId);

        db.Dictionaries.Add(dict);
        await db.SaveChangesAsync(ct);
        return Result.Ok(dict.Id);
    }
}
