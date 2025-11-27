using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedExamPapers",
                columns: table => new
                {
                    GeneratedExamPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalExamPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedContent = table.Column<string>(type: "text", nullable: false),
                    AiModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PromptSnapshot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedExamPapers", x => x.GeneratedExamPaperId);
                    table.ForeignKey(
                        name: "FK_GeneratedExamPapers_ExamPapers_OriginalExamPaperId",
                        column: x => x.OriginalExamPaperId,
                        principalTable: "ExamPapers",
                        principalColumn: "ExamPaperId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ffbcf7b8-346c-4d33-abc2-b817fb834a4d", "AQAAAAIAAYagAAAAECmP+2insCCfxOk9zsYCwQnyppIsVCnCgxs9MpimN1s/nP/Nl4n0tLoZcdEFoDMP9Q==", "abd5ba86-2a17-4981-a07f-d54a599f24ce" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedExamPapers_OriginalExamPaperId",
                table: "GeneratedExamPapers",
                column: "OriginalExamPaperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedExamPapers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fd17d973-5574-4d65-8052-cca23d5a5e13", "AQAAAAIAAYagAAAAED+UUyl8k1ueJHG8aE1D2tbAc0hDJzGFp/GG750NHRc6w1eNEmPnNkcnq2iEc6kBuQ==", "0f80909d-5696-4530-ae7f-a3061887ba5b" });
        }
    }
}
