
#nullable disable

namespace GSDT.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectEmailToRtbfRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubjectEmail",
                schema: "audit",
                table: "RtbfRequests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            // [RT-07/VAL-04] Backfill existing rows — LEFT JOIN + sentinel for deleted users + IF EXISTS guard
            migrationBuilder.Sql("""
                IF OBJECT_ID('[identity].AspNetUsers') IS NOT NULL
                BEGIN
                    UPDATE r
                    SET r.SubjectEmail = COALESCE(u.Email, '[deleted-user]')
                    FROM audit.RtbfRequests r
                    LEFT JOIN [identity].AspNetUsers u ON u.Id = r.DataSubjectId
                    WHERE r.SubjectEmail IS NULL
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectEmail",
                schema: "audit",
                table: "RtbfRequests");
        }
    }
}
