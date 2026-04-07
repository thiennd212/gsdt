using System;

#nullable disable

namespace GSDT.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRtbfPiiColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitizenNationalId",
                schema: "audit",
                table: "RtbfRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueBy",
                schema: "audit",
                table: "RtbfRequests",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "FailureLog",
                schema: "audit",
                table: "RtbfRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitizenNationalId",
                schema: "audit",
                table: "RtbfRequests");

            migrationBuilder.DropColumn(
                name: "DueBy",
                schema: "audit",
                table: "RtbfRequests");

            migrationBuilder.DropColumn(
                name: "FailureLog",
                schema: "audit",
                table: "RtbfRequests");
        }
    }
}
