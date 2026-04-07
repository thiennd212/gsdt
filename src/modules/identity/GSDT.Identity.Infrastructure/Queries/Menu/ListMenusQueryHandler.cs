using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Queries.Menu;

/// <summary>Handles ListMenusQuery — returns all menus with their permission codes (admin view).</summary>
public sealed class ListMenusQueryHandler(IdentityDbContext db)
    : IRequestHandler<ListMenusQuery, Result<IReadOnlyList<MenuDto>>>
{
    public async Task<Result<IReadOnlyList<MenuDto>>> Handle(
        ListMenusQuery request, CancellationToken ct)
    {
        var menus = await db.AppMenus
            .Include(m => m.RolePermissions)
            .Where(m => request.TenantId == null || m.TenantId == request.TenantId)
            .OrderBy(m => m.SortOrder)
            .Select(m => new MenuDto(
                m.Id,
                m.ParentId,
                m.Code,
                m.Title,
                m.Icon,
                m.Route,
                m.SortOrder,
                m.IsActive,
                m.TenantId,
                m.RolePermissions.Select(rp => rp.PermissionCode).ToList()))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<MenuDto>>(menus);
    }
}
