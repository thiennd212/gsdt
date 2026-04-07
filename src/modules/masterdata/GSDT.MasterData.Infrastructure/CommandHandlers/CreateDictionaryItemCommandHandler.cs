using FluentResults;
using MediatR;

namespace GSDT.MasterData.Infrastructure.CommandHandlers;

public sealed class CreateDictionaryItemCommandHandler(MasterDataDbContext db)
    : IRequestHandler<CreateDictionaryItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDictionaryItemCommand cmd, CancellationToken ct)
    {
        var dictExists = await db.Dictionaries.AnyAsync(d => d.Id == cmd.DictionaryId, ct);
        if (!dictExists)
            return Result.Fail(new NotFoundError($"Dictionary {cmd.DictionaryId} not found"));

        var codeExists = await db.DictionaryItems.AnyAsync(
            i => i.DictionaryId == cmd.DictionaryId && i.Code == cmd.Code, ct);
        if (codeExists)
            return Result.Fail(new ConflictError(
                $"Item with code '{cmd.Code}' already exists in dictionary {cmd.DictionaryId}"));

        if (cmd.ParentId.HasValue)
        {
            var parentExists = await db.DictionaryItems.AnyAsync(
                i => i.Id == cmd.ParentId.Value && i.DictionaryId == cmd.DictionaryId, ct);
            if (!parentExists)
                return Result.Fail(new ValidationError(
                    $"Parent item {cmd.ParentId} not found in dictionary {cmd.DictionaryId}"));
        }

        var item = DictionaryItem.Create(
            cmd.DictionaryId, cmd.Code, cmd.Name, cmd.NameVi,
            cmd.ParentId, cmd.SortOrder, cmd.EffectiveFrom,
            cmd.EffectiveTo, cmd.Metadata, cmd.TenantId, cmd.ActorId);

        db.DictionaryItems.Add(item);
        await db.SaveChangesAsync(ct);
        return Result.Ok(item.Id);
    }
}
