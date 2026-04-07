
#nullable disable

namespace GSDT.Organization.Infrastructure.Persistence.Migrations
{
    /// <summary>Deploy SQL Server RLS policies for tenant isolation on the organization schema.</summary>
    public partial class AddRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
                "organization",
                "OrgUnits",
                "StaffPositionHistories",
                "UserOrgUnitAssignments"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("organization", "OrgUnits"));
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("organization", "StaffPositionHistories"));
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("organization", "UserOrgUnitAssignments"));
        }
    }
}
