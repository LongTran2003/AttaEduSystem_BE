using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddExamFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FolderId",
                table: "ExamPapers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamFolders",
                columns: table => new
                {
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ColorCode = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFolders", x => x.FolderId);
                    table.ForeignKey(
                        name: "FK_ExamFolders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "abff5794-3c57-44eb-8974-81e5ab796804", "AQAAAAIAAYagAAAAEOMiZTsB3mrOTeejh24DXEat7Q18RclZZVrtAARPXBssEiSQFL5WvTxbv1F+Nii/Wg==", "011c6007-f548-463b-acc5-88b02ac856aa" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 6, 14, 22, 20, 534, DateTimeKind.Utc).AddTicks(2069));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 6, 14, 22, 20, 534, DateTimeKind.Utc).AddTicks(2079));

            migrationBuilder.CreateIndex(
                name: "IX_ExamPapers_FolderId",
                table: "ExamPapers",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFolders_UserId",
                table: "ExamFolders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPapers_ExamFolders_FolderId",
                table: "ExamPapers",
                column: "FolderId",
                principalTable: "ExamFolders",
                principalColumn: "FolderId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamPapers_ExamFolders_FolderId",
                table: "ExamPapers");

            migrationBuilder.DropTable(
                name: "ExamFolders");

            migrationBuilder.DropIndex(
                name: "IX_ExamPapers_FolderId",
                table: "ExamPapers");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "ExamPapers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "03625c98-cde1-4ea9-aa96-fab977fe6965", "AQAAAAIAAYagAAAAEGTKSgaOeVLuDbdKasY1tNZnwTf3KpAC/fZszD7YRSU9AWDmiFijG6EiNpLBpA1+Ow==", "76c97153-c209-4a72-b5a7-ba71257ee946" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 5, 6, 38, 12, 256, DateTimeKind.Utc).AddTicks(85));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 5, 6, 38, 12, 256, DateTimeKind.Utc).AddTicks(92));
        }
    }
}
