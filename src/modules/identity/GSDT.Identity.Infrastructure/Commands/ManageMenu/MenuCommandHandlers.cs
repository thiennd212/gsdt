using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Commands.ManageMenu;

/// <summary>Creates a new menu item with its permission requirements.</summary>
public sealed class CreateMenuCommandHandler(IdentityDbContext db)
    : IRequestHandler<CreateMenuCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMenuCommand cmd, CancellationToken ct)
    {
        var codeExists = await db.AppMenus.AnyAsync(
            m => m.Code == cmd.Code && m.TenantId == cmd.TenantId, ct);

        if (codeExists)
            return Result.Fail(new ConflictError(
                $"Menu code '{cmd.Code}' already exists for this tenant."));

        var menu = new AppMenu
        {
            Id = Guid.NewGuid(),
            ParentId = cmd.ParentId,
            Code = cmd.Code,
            Title = cmd.Title,
            Icon = cmd.Icon,
            Route = cmd.Route,
            SortOrder = cmd.SortOrder,
            IsActive = true,
            TenantId = cmd.TenantId,
            RolePermissions = cmd.PermissionCodes
                .Select(code => new MenuRolePermission { PermissionCode = code })
                .ToList()
        };

        // Set MenuId on each child before Add so EF wires the FK
        foreach (var rp in menu.RolePermissions)
            rp.MenuId = menu.Id;

        db.AppMenus.Add(menu);
        await db.SaveChangesAsync(ct);
        return Result.Ok(menu.Id);
    }
}

/// <summary>
/// Updates an existing menu item — replaces permission codes in full.
/// </summary>
public sealed class UpdateMenuCommandHandler(IdentityDbContext db)
    : IRequestHandler<UpdateMenuCommand, Result>
{
    public async Task<Result> Handle(UpdateMenuCommand cmd, CancellationToken ct)
    {
        var menu = await db.AppMenus
            .Include(m => m.RolePermissions)
            .FirstOrDefaultAsync(m => m.Id == cmd.Id, ct);

        if (menu is null)
            return Result.Fail(new NotFoundError($"Menu {cmd.Id} not found."));

        menu.ParentId = cmd.ParentId;
        menu.Title = cmd.Title;
        menu.Icon = cmd.Icon;
        menu.Route = cmd.Route;
        menu.SortOrder = cmd.SortOrder;
        menu.IsActive = cmd.IsActive;

        // Replace permission codes — remove old, add new
        db.MenuRolePermissions.RemoveRange(menu.RolePermissions);
        menu.RolePermissions = cmd.PermissionCodes
            .Select(code => new MenuRolePermission { MenuId = menu.Id, PermissionCode = code })
            .ToList();

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
