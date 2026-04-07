
#nullable disable

namespace GSDT.SystemParams.Infrastructure.Persistence.Migrations
{
    /// <summary>Deploy SQL Server RLS policies for tenant isolation on the config schema.</summary>
    public partial class AddRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only SystemParameters has TenantId; SystemAnnouncements is tenant-agnostic
            migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
                "config",
                "SystemParameters"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("config", "SystemParameters"));
        }
    }
}
