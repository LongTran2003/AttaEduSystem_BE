using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlan3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudyPlans_UserId_IsActive_Status",
                table: "StudyPlans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StudyPlans");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6fd46845-38a2-4365-b53f-c8303f7d0720", "AQAAAAIAAYagAAAAECsCSiSMs1oj4bqmWpB6Wmkd4VwJzt0um4PJY/oSP3Ff0wPiFVgw6LD2ulKrhBuOTw==", "ee464f17-38fc-40ef-8240-7dac610975b4" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 23, 10, 44, 59, 755, DateTimeKind.Utc).AddTicks(8627));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 23, 10, 44, 59, 755, DateTimeKind.Utc).AddTicks(8637));

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_UserId_Status",
                table: "StudyPlans",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudyPlans_UserId_Status",
                table: "StudyPlans");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StudyPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
        }
    }
}
