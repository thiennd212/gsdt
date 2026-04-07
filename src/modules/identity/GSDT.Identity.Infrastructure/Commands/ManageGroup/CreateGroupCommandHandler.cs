using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Commands.ManageGroup;

/// <summary>
/// Creates a new UserGroup. Validates Code uniqueness within the tenant before persisting.
/// </summary>
public sealed class CreateGroupCommandHandler(IdentityDbContext db)
    : IRequestHandler<CreateGroupCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateGroupCommand cmd, CancellationToken ct)
    {
        // Enforce Code uniqueness per tenant
        var exists = await db.UserGroups
            .AnyAsync(g => g.Code == cmd.Code && g.TenantId == cmd.TenantId, ct);

        if (exists)
            return Result.Fail(new ConflictError($"Group code '{cmd.Code}' already exists in this tenant."));

        var group = new UserGroup
        {
            Id = Guid.NewGuid(),
            Code = cmd.Code,
            Name = cmd.Name,
            Description = cmd.Description,
            TenantId = cmd.TenantId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.UserGroups.Add(group);
        await db.SaveChangesAsync(ct);

        return Result.Ok(group.Id);
    }
}
