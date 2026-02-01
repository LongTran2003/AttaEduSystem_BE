using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSolvesQuotaColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SolvesUsed",
                table: "UserUsages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxSolvesPerMonth",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04115f41-1442-4ef3-94b3-cd5da73f4a9e", "AQAAAAIAAYagAAAAEFBtJ4Quuu23NirqhG6mSk07arKRbe7Xz6YlH6gfeQ8MoFadgnbBwucwIthvYaOcFw==", "f2b70ec7-75bb-4173-b926-4b517c42d180" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedTime", "MaxSolvesPerMonth" },
                values: new object[] { new DateTime(2026, 2, 1, 4, 52, 45, 6, DateTimeKind.Utc).AddTicks(8175), 0 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedTime", "MaxSolvesPerMonth" },
                values: new object[] { new DateTime(2026, 2, 1, 4, 52, 45, 6, DateTimeKind.Utc).AddTicks(8184), 100 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolvesUsed",
                table: "UserUsages");

            migrationBuilder.DropColumn(
                name: "MaxSolvesPerMonth",
                table: "SubscriptionPlans");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "69945397-001f-4969-9676-3f221366d3d9", "AQAAAAIAAYagAAAAEHSjmjIo8yEAhd0rUQL8l/3s0Wcznth4OwLSYOgA2RMA/dSAP8/8zEwtIcB4HaVn2Q==", "17b1e92d-6d7e-4840-8bd2-cdc616782d5b" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 1, 30, 3, 11, 12, 695, DateTimeKind.Utc).AddTicks(2319));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 1, 30, 3, 11, 12, 695, DateTimeKind.Utc).AddTicks(2331));
        }
    }
}
