
#nullable disable

namespace GSDT.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceJsonToConsentRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenceJson",
                schema: "identity",
                table: "ConsentRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceJson",
                schema: "identity",
                table: "ConsentRecords");
        }
    }
}
