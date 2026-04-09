using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GSDT.Api;

/// <summary>
/// Hosted service that seeds permission records and role-permission assignments.
/// Runs in ALL environments (permissions are required data, not dev-only).
///
/// Idempotent: checks existing records before inserting. Transaction-safe.
/// Must be registered AFTER GsdtRoleSeeder (roles must exist first).
/// </summary>
internal sealed class GsdtPermissionSeeder(
    IServiceProvider sp,
    ILogger<GsdtPermissionSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var seededPermissions = await SeedPermissionsAsync(db, ct);
            await SeedRolePermissionsAsync(db, roleManager, seededPermissions, ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[PermissionSeeder] Failed — rolling back transaction. Will retry on next restart");
            await tx.RollbackAsync(ct);
            // Do NOT re-throw: unhandled exception in IHostedService.StartAsync kills the host.
            // Permissions will be seeded on next startup when DB is available.
        }
    }

    /// <summary>Seed missing Permission rows. Returns code→id map for all permissions.</summary>
    private async Task<Dictionary<string, Guid>> SeedPermissionsAsync(
        IdentityDbContext db, CancellationToken ct)
    {
        var existing = await db.Permissions
            .Select(p => new { p.Code, p.Id })
            .ToDictionaryAsync(p => p.Code, p => p.Id, ct);

        var newCount = 0;
        foreach (var def in PermissionSeedDefinitions.AllPermissions)
        {
            if (existing.ContainsKey(def.Code)) continue;

            var entity = new Permission
            {
                Id = Guid.NewGuid(),
                Code = def.Code,
                Name = def.Name,
                ModuleCode = def.ModuleCode,
                ResourceCode = def.ResourceCode,
                ActionCode = def.ActionCode,
                IsSensitive = def.IsSensitive,
            };

            db.Permissions.Add(entity);
            existing[def.Code] = entity.Id;
            newCount++;
        }

        if (newCount > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("[PermissionSeeder] Seeded {Count} new permissions", newCount);
        }
        else
        {
            logger.LogDebug("[PermissionSeeder] All {Total} permissions already exist", existing.Count);
        }

        return existing;
    }

    /// <summary>Seed missing RolePermission rows for each role in the mapping.</summary>
    private async Task SeedRolePermissionsAsync(
        IdentityDbContext db,
        RoleManager<ApplicationRole> roleManager,
        Dictionary<string, Guid> permissionMap,
        CancellationToken ct)
    {
        // Load existing role-permission pairs for O(1) duplicate check
        var existingPairs = (await db.RolePermissions
            .Select(rp => new ValueTuple<Guid, Guid>(rp.RoleId, rp.PermissionId))
            .ToListAsync(ct))
            .ToHashSet();

        var totalNew = 0;

        foreach (var (roleName, permissionCodes) in PermissionSeedDefinitions.RolePermissionMap)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                logger.LogWarning("[PermissionSeeder] Role '{Role}' not found — skipping its permissions", roleName);
                continue;
            }

            var roleNew = 0;
            foreach (var code in permissionCodes)
            {
                if (!permissionMap.TryGetValue(code, out var permId))
                {
                    logger.LogWarning("[PermissionSeeder] Permission '{Code}' not in DB — skipping for role '{Role}'",
                        code, roleName);
                    continue;
                }

                if (existingPairs.Contains((role.Id, permId))) continue;

                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permId,
                });
                existingPairs.Add((role.Id, permId));
                roleNew++;
            }

            if (roleNew > 0)
                logger.LogInformation("[PermissionSeeder] Assigned {Count} permissions to role '{Role}'",
                    roleNew, roleName);

            totalNew += roleNew;
        }

        if (totalNew > 0)
            await db.SaveChangesAsync(ct);
        else
            logger.LogDebug("[PermissionSeeder] All role-permission assignments already exist");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
