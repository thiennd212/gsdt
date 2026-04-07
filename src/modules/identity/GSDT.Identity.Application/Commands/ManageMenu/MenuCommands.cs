
namespace GSDT.Identity.Application.Commands.ManageMenu;

/// <summary>Create a new menu item.</summary>
public sealed record CreateMenuCommand(
    Guid? ParentId,
    string Code,
    string Title,
    string? Icon,
    string? Route,
    int SortOrder,
    IReadOnlyList<string> PermissionCodes,
    Guid? TenantId) : ICommand<Guid>;

/// <summary>Update an existing menu item.</summary>
public sealed record UpdateMenuCommand(
    Guid Id,
    Guid? ParentId,
    string Title,
    string? Icon,
    string? Route,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<string> PermissionCodes) : ICommand;
