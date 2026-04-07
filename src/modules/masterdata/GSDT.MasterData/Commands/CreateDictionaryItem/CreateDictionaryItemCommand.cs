
namespace GSDT.MasterData.Commands.CreateDictionaryItem;

public sealed record CreateDictionaryItemCommand(
    Guid DictionaryId,
    string Code,
    string Name,
    string NameVi,
    Guid? ParentId,
    int SortOrder,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string? Metadata,
    Guid TenantId,
    Guid ActorId) : ICommand<Guid>;
