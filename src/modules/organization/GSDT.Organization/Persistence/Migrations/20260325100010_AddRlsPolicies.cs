
#nullable disable

namespace GSDT.Organization.Persistence.Migrations;

/// <summary>
/// Deploy SQL Server RLS policies for tenant isolation on the organization schema.
/// StaffPositionHistory maps to table StaffPositionHistories (EF pluralization via ToTable config).
/// </summary>
public partial class AddRlsPolicies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
            "organization",
            "OrgUnits",
            "StaffPositionHistories",
            "UserOrgUnitAssignments"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("organization", "OrgUnits"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("organization", "StaffPositionHistories"));
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("organization", "UserOrgUnitAssignments"));
    }
}
