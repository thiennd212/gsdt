using GSDT.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace GSDT.Api;

/// <summary>
/// Development-only hosted service that seeds GSDT-specific roles (BTC, CQCQ, CDT)
/// and creates test users for each role. No-op outside Development environment.
///
/// Roles are seeded as system-level (TenantId = null) so they are visible across tenants.
/// Test users are assigned password "Dev@12345!" — NEVER deploy to production.
/// </summary>
internal sealed class GsdtRoleSeeder(
    IServiceProvider sp,
    IWebHostEnvironment env,
    ILogger<GsdtRoleSeeder> logger) : IHostedService
{
    // Role definitions: (Code/Name, DisplayName, Description)
    private static readonly (string Code, string Name, string Description)[] GsdtRoles =
    [
        ("BTC",  "Bo Tai chinh",      "Quan tri he thong — toan quyen doc/ghi toan bo du an"),
        ("CQCQ", "Co quan chu quan",  "Chi doc du an trong pham vi co quan chu quan"),
        ("CDT",  "Chu dau tu",        "Doc va sua du an cua chinh don vi chu dau tu"),
    ];

    // Test users: (UserName, FullName, Role)
    private static readonly (string UserName, string FullName, string Role)[] TestUsers =
    [
        ("test.btc@gsdt.gov.vn",  "Test BTC User",  "BTC"),
        ("test.cqcq@gsdt.gov.vn", "Test CQCQ User", "CQCQ"),
        ("test.cdt@gsdt.gov.vn",  "Test CDT User",  "CDT"),
    ];

    private const string TestPassword = "Dev@12345!";

    public async Task StartAsync(CancellationToken ct)
    {
        if (!env.IsDevelopment()) return;

        using var scope = sp.CreateScope();
        var roleManager  = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager, ct);
        await SeedTestUsersAsync(userManager, ct);
    }

    private async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, CancellationToken ct)
    {
        foreach (var (code, name, description) in GsdtRoles)
        {
            // Skip if role already exists (idempotent)
            if (await roleManager.RoleExistsAsync(name)) continue;

            var role = new ApplicationRole
            {
                Name        = name,
                Code        = code,
                Description = description,
                RoleType    = RoleType.System,
                TenantId    = null, // system-wide role
                IsActive    = true,
            };

            var result = await roleManager.CreateAsync(role);
            if (result.Succeeded)
                logger.LogInformation("[GsdtRoleSeeder] Created role: {Role}", name);
            else
                logger.LogWarning("[GsdtRoleSeeder] Failed to create role {Role}: {Errors}",
                    name, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager, CancellationToken ct)
    {
        foreach (var (userName, fullName, role) in TestUsers)
        {
            var existing = await userManager.FindByEmailAsync(userName);
            if (existing is not null)
            {
                // Ensure the user is in the correct role even if already created
                if (!await userManager.IsInRoleAsync(existing, role))
                    await userManager.AddToRoleAsync(existing, role);
                continue;
            }

            var user = new ApplicationUser
            {
                UserName  = userName,
                Email     = userName,
                FullName  = fullName,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(user, TestPassword);
            if (!createResult.Succeeded)
            {
                logger.LogWarning("[GsdtRoleSeeder] Failed to create user {User}: {Errors}",
                    userName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                continue;
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (roleResult.Succeeded)
                logger.LogInformation("[GsdtRoleSeeder] Created test user {User} with role {Role}", userName, role);
            else
                logger.LogWarning("[GsdtRoleSeeder] Created user {User} but failed to assign role {Role}: {Errors}",
                    userName, role, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
