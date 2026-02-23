using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlan2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudyPlans",
                columns: table => new
                {
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeekEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlanJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionPercentage = table.Column<double>(type: "double precision", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlans", x => x.StudyPlanId);
                    table.ForeignKey(
                        name: "FK_StudyPlans_AspNetUsers_UserId",
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
                values: new object[] { "99353484-e5de-4d8a-8beb-fcfd4a1ab8f9", "AQAAAAIAAYagAAAAEB7zwCNgsoU9HKB2YPmB9ND/qrysSAEw56hh5lQO/f5olP+ieeCKqgEDrSkoe4e76g==", "bb613711-c42c-4585-aeb6-7e555a9df773" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 23, 6, 14, 54, 957, DateTimeKind.Utc).AddTicks(752));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 23, 6, 14, 54, 957, DateTimeKind.Utc).AddTicks(760));

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_UserId_IsActive_Status",
                table: "StudyPlans",
                columns: new[] { "UserId", "IsActive", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_UserId_WeekStart",
                table: "StudyPlans",
                columns: new[] { "UserId", "WeekStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyPlans");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4c486035-54c1-4e52-9a4e-779e4f2b8e4f", "AQAAAAIAAYagAAAAEHzVjnkyZZCEKVY+Xww/MV5UVOn7HXIKdY3wJ2s3CfeC2rVGR5BmY7Dz0j87kSTHaA==", "c23c64fa-dc44-40d3-b27e-e05e08a17d23" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 23, 6, 10, 58, 844, DateTimeKind.Utc).AddTicks(6736));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 23, 6, 10, 58, 844, DateTimeKind.Utc).AddTicks(6746));
        }
    }
}
