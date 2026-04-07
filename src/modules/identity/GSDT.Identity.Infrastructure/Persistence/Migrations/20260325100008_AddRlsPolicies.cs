
#nullable disable

namespace GSDT.Identity.Infrastructure.Persistence.Migrations;

/// <summary>Deploy SQL Server RLS policies for tenant isolation on the identity schema.</summary>
public partial class AddRlsPolicies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
            "identity",
            "CredentialPolicies"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("identity", "CredentialPolicies"));
    }
}
