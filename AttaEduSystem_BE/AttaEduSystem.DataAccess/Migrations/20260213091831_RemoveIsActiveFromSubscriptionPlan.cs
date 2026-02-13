using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsActiveFromSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fab66f3a-49a3-4591-be68-270844be55a2", "AQAAAAIAAYagAAAAEACXv5I0hx3Q6MftRkfFmj4CizYXZitMLuObpQFLKIYVDp0RbS6d+xOHy541HL2xaw==", "524fef2e-fe93-4a6f-90fd-3f31d4e11335" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 13, 9, 18, 30, 803, DateTimeKind.Utc).AddTicks(537));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 13, 9, 18, 30, 803, DateTimeKind.Utc).AddTicks(546));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6f6a9e3e-d774-42df-9ce6-ac63ddaadf63", "AQAAAAIAAYagAAAAEArMDeXZdZXeHvkcMQk9GcfJLqsGMrDiWssszUgKm95Ss0070gPES2qR74JNQ2dgeA==", "9f093f5f-677d-4e04-804f-e91ec9bca5a7" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 13, 9, 14, 46, 958, DateTimeKind.Utc).AddTicks(9881));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 13, 9, 14, 46, 958, DateTimeKind.Utc).AddTicks(9890));
        }
    }
}
