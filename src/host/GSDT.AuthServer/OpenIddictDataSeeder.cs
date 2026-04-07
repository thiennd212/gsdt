using OpenIddict.Abstractions;

namespace GSDT.AuthServer;

/// <summary>
/// Seeds OpenIddict application registrations and dev users on startup.
/// All methods are idempotent — safe to run on every startup.
/// WARNING: Dev user seeding must never run in production.
/// </summary>
internal static class OpenIddictDataSeeder
{
    // ---------------------------------------------------------------------------
    // Dev seed: registers the API and a test SPA client in OpenIddict
    // ---------------------------------------------------------------------------
    internal static async Task SeedApplicationsAsync(IServiceProvider services, IWebHostEnvironment env)
    {
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Register custom "api" scope — required for token requests with scope=api
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("api") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                DisplayName = "GSDT API access"
            });
        }

        // Resource server descriptor (the API itself) — Confidential client, secret in descriptor
        const string apiClientId = "gsdt-api-resource";
        if (await manager.FindByClientIdAsync(apiClientId) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = apiClientId,
                ClientSecret = "dev-resource-secret-change-in-prod",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Introspection
                }
            });
        }

        // Dev SPA / Swagger client (Authorization Code + PKCE, public)
        const string spaClientId = "gsdt-spa-dev";
        // Delete + recreate to pick up permission changes (dev only)
        var existingSpa = await manager.FindByClientIdAsync(spaClientId);
        if (existingSpa is not null)
            await manager.DeleteAsync(existingSpa);
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = spaClientId,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                RedirectUris =
                {
                    new Uri("http://localhost:3000/callback"),
                    new Uri("http://localhost:4200/callback"),
                    new Uri("http://localhost:5173/callback"),
                    new Uri("https://localhost:5001/swagger/oauth2-redirect.html")
                },
                PostLogoutRedirectUris =
                {
                    // Both with and without trailing slash — oidc-client-ts sends origin without slash
                    new Uri("http://localhost:3000"),
                    new Uri("http://localhost:3000/"),
                    new Uri("http://localhost:4200"),
                    new Uri("http://localhost:4200/"),
                    new Uri("http://localhost:5173"),
                    new Uri("http://localhost:5173/")
                },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    // Scopes — must match FE oidc-client-ts scope config exactly
                    $"{OpenIddictConstants.Permissions.Prefixes.Scope}openid",
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    $"{OpenIddictConstants.Permissions.Prefixes.Scope}api",
                    $"{OpenIddictConstants.Permissions.Prefixes.Scope}offline_access"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }

        // Dev ROPC client — password flow for .http files and local tooling ONLY
        // WARNING: Never seed this in production environments
        if (env.IsDevelopment())
        {
            const string ropcClientId = "gsdt-dev-ropc";
            if (await manager.FindByClientIdAsync(ropcClientId) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = ropcClientId,
                    ClientSecret = "dev-secret-not-for-production",
                    ClientType = OpenIddictConstants.ClientTypes.Confidential,
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.Password,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        $"{OpenIddictConstants.Permissions.Prefixes.Scope}api"
                    }
                });
            }
        }
    }

    // ---------------------------------------------------------------------------
    // Dev seed: creates test users for local development and E2E tests
    // WARNING: Never seed this in production environments
    // ---------------------------------------------------------------------------
    internal static async Task SeedDevUsersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Ensure all required roles exist
        foreach (var role in new[] { "Admin", "SystemAdmin", "GovOfficer", "Viewer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        // Default dev tenant (matches E2E seed TEST_TENANT_ID)
        var devTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Passwords from config — no hardcoded secrets in source
        var adminPassword = config["SeedPasswords:AdminPassword"] ?? "DevAdmin@12345";
        var officerPassword = config["SeedPasswords:OfficerPassword"] ?? "DevOfficer@12345";
        var viewerPassword = config["SeedPasswords:ViewerPassword"] ?? "DevViewer@12345";

        // admin@dev.local — full admin access (Admin + SystemAdmin + GovOfficer)
        await EnsureDevUserAsync(
            userManager, roleManager,
            email: "admin@dev.local",
            password: adminPassword,
            fullName: "Dev Admin",
            roles: ["Admin", "SystemAdmin", "GovOfficer"],
            tenantId: devTenantId);

        // officer@dev.local — GovOfficer only (for role-based access tests)
        await EnsureDevUserAsync(
            userManager, roleManager,
            email: "officer@dev.local",
            password: officerPassword,
            fullName: "Dev Officer",
            roles: ["GovOfficer"],
            tenantId: devTenantId);

        // viewer@dev.local — no privileged roles (for 403 / read-only tests)
        await EnsureDevUserAsync(
            userManager, roleManager,
            email: "viewer@dev.local",
            password: viewerPassword,
            fullName: "Dev Viewer",
            roles: []);
    }

    /// <summary>
    /// Creates user if absent; on existing user ensures all required roles and tenant are assigned.
    /// Idempotent — safe to run on every startup.
    /// </summary>
    internal static async Task EnsureDevUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        string email, string password, string fullName, string[] roles,
        Guid? tenantId = null)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Ensure role membership is correct (handles case where roles changed after initial seed)
            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(existing, role))
                    await userManager.AddToRoleAsync(existing, role);
            }

            // Ensure tenant assignment is correct
            if (tenantId.HasValue && existing.TenantId != tenantId)
            {
                existing.TenantId = tenantId;
                await userManager.UpdateAsync(existing);
            }
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            TenantId = tenantId,
        };

        // Password meets policy: 12+ chars, uppercase, digit, special char
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"[SeedDevUser] Failed to create '{email}': {errors}");
            return;
        }

        foreach (var role in roles)
            await userManager.AddToRoleAsync(user, role);

        Console.WriteLine($"[SeedDevUser] Created '{email}' with roles: [{string.Join(", ", roles)}]");
    }
}
