using OpenIddict.Abstractions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// === Serilog structured logging ===
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// === EF Core + OpenIddict stores ===
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));
    options.UseOpenIddict();
    // Suppress pending-model-changes warning: OpenIddict manages its own schema
    // via its own migration system; EF warning is a false positive here
    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// === ASP.NET Core Identity ===
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
    {
        opts.Password.RequiredLength = 12;
        opts.Password.RequireNonAlphanumeric = true;
        opts.Password.RequireDigit = true;
        opts.Password.RequireUppercase = true;
        opts.Lockout.MaxFailedAccessAttempts = 5;
        // Dev/Test: short lockout for E2E test resilience; Production: 15min per QĐ742 §5.2
        opts.Lockout.DefaultLockoutTimeSpan = builder.Environment.IsProduction()
            ? TimeSpan.FromMinutes(15)
            : TimeSpan.FromSeconds(30);
        opts.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

// === Cookie security per architecture decision ===
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.Name = builder.Environment.IsDevelopment() ? "aqt-auth" : "__Host-aqt-auth";
    opts.Cookie.HttpOnly = true;
    opts.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    opts.Cookie.SameSite = SameSiteMode.Strict;
    opts.ExpireTimeSpan = TimeSpan.FromHours(8);
    opts.SlidingExpiration = false;
});

// === External identity providers (SSO/OIDC federation) ===
var externalProviders = builder.Configuration
    .GetSection("ExternalProviders")
    .Get<ExternalProviderConfig[]>() ?? [];

foreach (var provider in externalProviders)
{
    builder.Services.AddAuthentication()
        .AddOpenIdConnect(provider.Scheme, provider.DisplayName, opts =>
        {
            opts.Authority = provider.Authority;
            opts.ClientId = provider.ClientId;
            opts.ClientSecret = provider.ClientSecret;
            opts.ResponseType = "code"; // Authorization Code flow
            opts.SaveTokens = false;    // We don't need IdP tokens
            opts.GetClaimsFromUserInfoEndpoint = true;
            opts.CallbackPath = $"/signin-{provider.Scheme.ToLowerInvariant()}";
            opts.SignInScheme = IdentityConstants.ExternalScheme;
            // Map standard claims
            opts.Scope.Clear();
            opts.Scope.Add("openid");
            opts.Scope.Add("profile");
            opts.Scope.Add("email");
        });
}

// === OpenIddict authorization server ===
builder.Services.AddOpenIddict()
    .AddCore(opts => opts.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())
    .AddServer(opts =>
    {
        // Only Authorization Code + PKCE — no implicit flow (OIDC hardening)
        opts.AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange();

        opts.AllowRefreshTokenFlow();

        // ROPC dev flow — for .http file testing only
        if (builder.Environment.IsDevelopment())
            opts.AllowPasswordFlow();

        // Register custom scopes — must match FE oidc-client-ts scope config
        opts.RegisterScopes("openid", "profile", "email", "roles", "api", "offline_access");

        // Disable all other flows
        opts.DisableAccessTokenEncryption(); // reference tokens used instead

        // Endpoints
        opts.SetAuthorizationEndpointUris("/connect/authorize")
            .SetTokenEndpointUris("/connect/token")
            .SetUserInfoEndpointUris("/connect/userinfo")
            .SetIntrospectionEndpointUris("/connect/introspect")
            .SetRevocationEndpointUris("/connect/revoke")
            .SetEndSessionEndpointUris("/connect/logout");

        // Token settings
        opts.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
        opts.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

        // Refresh token rotation — zero reuse leeway (security hardening)
        opts.SetRefreshTokenReuseLeeway(TimeSpan.Zero);

        // Use reference tokens (stored in DB, revocable)
        opts.UseReferenceAccessTokens();
        opts.UseReferenceRefreshTokens();

        // Signing/encryption — dev uses ephemeral keys; production uses Vault-backed certs
        if (builder.Environment.IsDevelopment())
        {
            opts.AddDevelopmentEncryptionCertificate();
            opts.AddDevelopmentSigningCertificate();

            // Allow HTTP in dev/Docker — set issuer explicitly to avoid HTTPS auto-detection
            var issuerUri = builder.Configuration["Auth:IssuerUri"];
            if (!string.IsNullOrEmpty(issuerUri))
                opts.SetIssuer(new Uri(issuerUri));
        }

        var aspNetCore = opts.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableStatusCodePagesIntegration();

        if (builder.Environment.IsDevelopment())
            aspNetCore.DisableTransportSecurityRequirement();
    })
    .AddValidation(opts =>
    {
        opts.UseLocalServer();
        opts.UseAspNetCore();
    });

builder.Services.AddControllersWithViews();

// NullCacheService — AuthServer only handles consent/token flows; no caching needed
builder.Services.AddSingleton<ICacheService, NullCacheService>();

// Audit infrastructure — login attempt logging (shared DB with API)
// AuthServer uses "DefaultConnection" while Audit module expects "Default"
// Register AuditDbContext manually with the correct connection string name
builder.Services.AddDbContext<GSDT.Audit.Infrastructure.Persistence.AuditDbContext>(options =>
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "audit")));
builder.Services.AddScoped<GSDT.Audit.Domain.Repositories.IAuditLogRepository,
    GSDT.Audit.Infrastructure.Persistence.AuditLogRepository>();
builder.Services.AddScoped<GSDT.Audit.Domain.Services.IAuditService,
    GSDT.Audit.Infrastructure.Services.AuditService>();
builder.Services.AddScoped<GSDT.Audit.Application.Services.IBackgroundAuditJobEnqueuer,
    GSDT.Audit.Infrastructure.Services.HangfireAuditJobEnqueuer>();
// ICurrentUser required by AuditService — resolved from HttpContext claims
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// JIT SSO provisioning — auto-create users on first external login
builder.Services.AddScoped<GSDT.Identity.Domain.Repositories.IExternalIdentityRepository,
    GSDT.Identity.Infrastructure.Persistence.ExternalIdentityRepository>();
builder.Services.AddScoped<GSDT.Identity.Domain.Repositories.IJitProviderConfigRepository,
    GSDT.Identity.Infrastructure.Persistence.JitProviderConfigRepository>();
builder.Services.AddScoped<JitProvisioningService>();

// MediatR — only consent handlers needed for authorization consent page
// Disable ValidateOnBuild: other Identity.Application handlers require full
// infrastructure (repositories, Dapper) registered only in GSDT.Api
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(GSDT.Identity.Application.ModuleRegistration).Assembly));
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = builder.Environment.IsDevelopment();
    options.ValidateOnBuild = false; // AuthServer only needs consent handlers at runtime
});

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sqlserver");

// === CORS — allow FE origins for OIDC discovery + token endpoints ===
// Origins configured via Cors:AllowedOrigins in appsettings; fallback to local dev defaults.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://localhost:3000", "http://localhost:5001"];
        policy.WithOrigins(corsOrigins)
            .WithHeaders("Content-Type", "Authorization", "X-Correlation-Id", "X-Tenant-Id", "Accept", "Accept-Language")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
            .AllowCredentials();
    });
});

var app = builder.Build();

// === Run EF migrations on startup (idempotent) ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
}

// === Seed dev client applications ===
await OpenIddictDataSeeder.SeedApplicationsAsync(app.Services, app.Environment);

// === Seed dev users for local/E2E testing ===
if (app.Environment.IsDevelopment())
    await OpenIddictDataSeeder.SeedDevUsersAsync(app.Services);

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(); // Must be after UseRouting, before UseAuthentication
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
