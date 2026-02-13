using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixChatConversationField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SubscriptionPlans");

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
                columns: new[] { "CreatedTime", "Status" },
                values: new object[] { new DateTime(2026, 2, 13, 9, 14, 46, 958, DateTimeKind.Utc).AddTicks(9881), "Active" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedTime", "Status" },
                values: new object[] { new DateTime(2026, 2, 13, 9, 14, 46, 958, DateTimeKind.Utc).AddTicks(9890), "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "53435381-1de4-43b6-8ed8-d354f3095fa8", "AQAAAAIAAYagAAAAEDdOPUkzYk9C3y3xxdb8goV97t2ff096jfMptAGkZm9pZ5HtgUGwMXV0KM4zEfiLhw==", "f594bc01-7182-40f0-8ab9-462569481e6b" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedTime", "IsActive", "Status" },
                values: new object[] { new DateTime(2026, 2, 13, 7, 49, 35, 51, DateTimeKind.Utc).AddTicks(1197), true, null });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedTime", "IsActive", "Status" },
                values: new object[] { new DateTime(2026, 2, 13, 7, 49, 35, 51, DateTimeKind.Utc).AddTicks(1206), true, null });
        }
    }
}
