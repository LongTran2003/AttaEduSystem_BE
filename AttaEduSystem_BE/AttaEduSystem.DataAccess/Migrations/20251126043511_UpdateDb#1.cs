using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamPapers",
                columns: table => new
                {
                    ExamPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalImageUrl = table.Column<string>(type: "text", nullable: false),
                    ScannedText = table.Column<string>(type: "text", nullable: true),
                    ExamFormat = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPapers", x => x.ExamPaperId);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "Movok-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c0582e48-dcd9-4cc0-a7de-dc34409d0c68", "AQAAAAIAAYagAAAAEICDDTnIXSjMLtsZptDFSUAn5XWQlIbBMv/dYC7FgXE6kuHNOxD5Sjd4G+up0gdLoA==", "ebb0f572-3d74-4475-aea4-21b3cf75a67e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamPapers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "Movok-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f938e57b-789e-4dbd-8915-893c0a635183", "AQAAAAIAAYagAAAAEJ6k7N0+Wa9ltZSm6wD4wmQHvDG/ZrUBcS2kP+3stJdSYkqMMLeg/uxfukAnPom8gw==", "f0e79e40-e4b3-4696-8a33-a758fabc4655" });
        }
    }
}
