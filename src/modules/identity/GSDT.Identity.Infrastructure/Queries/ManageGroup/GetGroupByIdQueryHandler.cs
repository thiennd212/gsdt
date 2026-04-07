using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Queries.ManageGroup;

/// <summary>Handles GetGroupByIdQuery — loads group with members and role assignments.</summary>
public sealed class GetGroupByIdQueryHandler(IdentityDbContext db)
    : IRequestHandler<GetGroupByIdQuery, Result<GroupDetailDto>>
{
    public async Task<Result<GroupDetailDto>> Handle(
        GetGroupByIdQuery request, CancellationToken ct)
    {
        var group = await db.UserGroups
            .Include(g => g.Members)
            .Include(g => g.RoleAssignments)
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct);

        if (group is null)
            return Result.Fail(new NotFoundError($"Group {request.GroupId} not found."));

        // Resolve member user details from AspNetUsers
        var memberUserIds = group.Members.Select(m => m.UserId).ToList();
        var users = memberUserIds.Count > 0
            ? await db.Users
                .Where(u => memberUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.Email })
                .ToListAsync(ct)
            : [];
        var userMap = users.ToDictionary(u => u.Id);

        var members = group.Members
            .Select(m => new GroupMemberDto(
                m.UserId,
                userMap.TryGetValue(m.UserId, out var u) ? u.FullName : "—",
                userMap.TryGetValue(m.UserId, out var u2) ? (u2.Email ?? "—") : "—"))
            .ToList();

        var dto = new GroupDetailDto(
            group.Id, group.Code, group.Name, group.Description,
            group.IsActive, group.TenantId, group.CreatedAtUtc,
            group.Members.Count,
            group.RoleAssignments.Select(r => r.RoleId).ToList(),
            members);

        return Result.Ok(dto);
    }
}
