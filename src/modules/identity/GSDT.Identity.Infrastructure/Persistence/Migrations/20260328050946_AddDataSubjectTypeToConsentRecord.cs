
#nullable disable

namespace GSDT.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDataSubjectTypeToConsentRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSubjectType",
                schema: "identity",
                table: "ConsentRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSubjectType",
                schema: "identity",
                table: "ConsentRecords");
        }
    }
}
