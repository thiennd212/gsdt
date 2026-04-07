
#nullable disable

namespace GSDT.MasterData.Infrastructure.Persistence.Migrations
{
    /// <summary>Deploy SQL Server RLS policies for tenant isolation on the masterdata schema.</summary>
    public partial class AddRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
                "masterdata",
                "Dictionaries",
                "DictionaryItems",
                "ExternalMappings"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("masterdata", "Dictionaries"));
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("masterdata", "DictionaryItems"));
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("masterdata", "ExternalMappings"));
        }
    }
}
