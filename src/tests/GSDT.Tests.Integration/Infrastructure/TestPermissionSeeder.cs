using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GSDT.Tests.Integration.Infrastructure;

/// <summary>
/// Seeds GSDT-specific roles, test users, permissions, and role-permission
/// assignments in the test database. Bypasses internal GsdtRoleSeeder/GsdtPermissionSeeder
/// by using public PermissionSeedDefinitions + RoleManager/UserManager directly.
///
/// Required because:
///   1. GsdtRoleSeeder is internal sealed + no-ops outside Development env
///   2. GsdtPermissionSeeder is internal sealed + removed as IHostedService by ApiFactory
///   3. PermissionAuthorizationHandler queries DB via EffectivePermissionService — real users needed
///
/// Call from ApiFactory.InitializeDatabaseAsync after migrations.
/// </summary>
public static class TestPermissionSeeder
{
    // Role definitions: Admin + SystemAdmin (both get all permissions) + GSDT-specific roles.
    // SystemAdmin needed because many existing tests use CreateAuthenticatedClient(roles: ["SystemAdmin"]).
    private static readonly (string Code, string Name, string Description)[] Roles =
    [
        ("Admin",       "Admin",       "Administrator — all permissions"),
        ("SystemAdmin", "SystemAdmin", "System administrator — all permissions"),
        ("BTC",         "BTC",         "Bo Tai chinh — toan quyen doc/ghi du an"),
        ("CQCQ",        "CQCQ",       "Co quan chu quan — chi doc du an"),
        ("CDT",         "CDT",         "Chu dau tu — doc va sua du an"),
    ];

    // Test users with deterministic emails for lookup in integration tests.
    // Admin + SystemAdmin users share all 23 permissions (widest access).
    private static readonly (string Email, string FullName, string Role)[] TestUsers =
    [
        ("test.admin@gsdt.gov.vn",       "Test Admin User",       "Admin"),
        ("test.systemadmin@gsdt.gov.vn", "Test SystemAdmin User", "SystemAdmin"),
        ("test.btc@gsdt.gov.vn",         "Test BTC User",         "BTC"),
        ("test.cqcq@gsdt.gov.vn",        "Test CQCQ User",       "CQCQ"),
        ("test.cdt@gsdt.gov.vn",         "Test CDT User",         "CDT"),
    ];

    /// <summary>
    /// Admin test user ID — populated after SeedAsync. Used by IntegrationTestBase
    /// to create the default client with a real DB user.
    /// </summary>
    public static string? AdminUserId { get; private set; }

    /// <summary>
    /// Role name → seeded user ID mapping. Used by ApiFactory.CreateAuthenticatedClient
    /// to auto-resolve real user IDs when only roles are provided.
    /// </summary>
    public static IReadOnlyDictionary<string, string> RoleUserIds { get; private set; }
        = new Dictionary<string, string>();

    private const string TestPassword = "Dev@12345678!";

    /// <summary>
    /// Seeds roles → users → permissions → role-permission mappings (in order).
    /// Idempotent — checks existing records before inserting.
    /// Creates its own DI scope to avoid disposed DbContext from migration loop.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider rootProvider)
    {
        using var scope = rootProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<IdentityDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedTestUsersAsync(userManager);
        await SeedPermissionsAsync(db);
        await SeedRolePermissionsAsync(db, roleManager);

        // Build role → userId mapping for auto-resolution in CreateAuthenticatedClient
        var map = new Dictionary<string, string>();
        foreach (var (email, _, role) in TestUsers)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
                map[role] = user.Id.ToString();
        }
        RoleUserIds = map;
        AdminUserId = map.GetValueOrDefault("Admin");
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var (code, name, description) in Roles)
        {
            if (await roleManager.RoleExistsAsync(name)) continue;
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = name,
                Code = code,
                Description = description,
                RoleType = RoleType.System,
                TenantId = null,
                IsActive = true,
            });
        }
    }

    private static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager)
    {
        foreach (var (email, fullName, role) in TestUsers)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                };
                var result = await userManager.CreateAsync(user, TestPassword);
                if (!result.Succeeded) continue;
            }
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedPermissionsAsync(IdentityDbContext db)
    {
        var existingCodes = await db.Permissions
            .Select(p => p.Code)
            .ToListAsync();
        var existingSet = existingCodes.ToHashSet();

        foreach (var def in PermissionSeedDefinitions.AllPermissions)
        {
            if (existingSet.Contains(def.Code)) continue;
            db.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Code = def.Code,
                Name = def.Name,
                ModuleCode = def.ModuleCode,
                ResourceCode = def.ResourceCode,
                ActionCode = def.ActionCode,
                IsSensitive = def.IsSensitive,
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(
        IdentityDbContext db,
        RoleManager<ApplicationRole> roleManager)
    {
        var permissionMap = await db.Permissions
            .ToDictionaryAsync(p => p.Code, p => p.Id);

        var existingPairs = (await db.RolePermissions
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync())
            .Select(x => (x.RoleId, x.PermissionId))
            .ToHashSet();

        // Combine PermissionSeedDefinitions map with SystemAdmin (all permissions)
        var allPermissionCodes = PermissionSeedDefinitions.AllPermissions.Select(p => p.Code).ToArray();
        var rolePermMap = new Dictionary<string, string[]>(PermissionSeedDefinitions.RolePermissionMap)
        {
            ["SystemAdmin"] = allPermissionCodes,
        };

        foreach (var (roleName, codes) in rolePermMap)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            foreach (var code in codes)
            {
                if (!permissionMap.TryGetValue(code, out var permId)) continue;
                if (existingPairs.Contains((role.Id, permId))) continue;
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permId,
                });
                existingPairs.Add((role.Id, permId));
            }
        }
        await db.SaveChangesAsync();
    }
}
