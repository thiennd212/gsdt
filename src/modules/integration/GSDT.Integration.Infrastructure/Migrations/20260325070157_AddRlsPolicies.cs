
#nullable disable

namespace GSDT.Integration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.GenerateRlsPolicies(
                "integration",
                "Partners",
                "Contracts",
                "MessageLogs"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("integration", "Partners"));
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("integration", "Contracts"));
            migrationBuilder.Sql(RlsMigrationHelper.DropRlsPolicy("integration", "MessageLogs"));
        }
    }
}
