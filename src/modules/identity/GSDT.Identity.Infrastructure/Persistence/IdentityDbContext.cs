// Explicit file-level using: brings IdentityDbContext<T,T,T> into scope without polluting global usings
// (adding it globally would create CS0104 ambiguity with the project's own IdentityDbContext class).
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GSDT.Identity.Infrastructure.Persistence;

/// <summary>
/// Identity module DbContext — schema "identity", separate from other module contexts.
/// Does NOT extend ModuleDbContext (Identity has its own base via IdentityDbContext&lt;T&gt;).
/// OpenIddict tables are registered here via UseOpenIddict().
/// </summary>
public class IdentityDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserDelegation> UserDelegations => Set<UserDelegation>();
    public DbSet<AccessReviewRecord> AccessReviewRecords => Set<AccessReviewRecord>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();
    public DbSet<AttributeRule> AttributeRules => Set<AttributeRule>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<UserGroupMembership> UserGroupMemberships => Set<UserGroupMembership>();
    public DbSet<GroupRoleAssignment> GroupRoleAssignments => Set<GroupRoleAssignment>();
    public DbSet<DataScopeType> DataScopeTypes => Set<DataScopeType>();
    public DbSet<RoleDataScope> RoleDataScopes => Set<RoleDataScope>();
    public DbSet<UserDataScopeOverride> UserDataScopeOverrides => Set<UserDataScopeOverride>();
    public DbSet<PolicyRule> PolicyRules => Set<PolicyRule>();
    public DbSet<SodConflictRule> SodConflictRules => Set<SodConflictRule>();
    public DbSet<AppMenu> AppMenus => Set<AppMenu>();
    public DbSet<MenuRolePermission> MenuRolePermissions => Set<MenuRolePermission>();
    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
    public DbSet<CredentialPolicy> CredentialPolicies => Set<CredentialPolicy>();
    public DbSet<JitProviderConfig> JitProviderConfigs => Set<JitProviderConfig>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.HasDefaultSchema("identity");
        mb.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        // Register OpenIddict entity model so EF migrations capture the tables
        mb.UseOpenIddict();
    }
}
