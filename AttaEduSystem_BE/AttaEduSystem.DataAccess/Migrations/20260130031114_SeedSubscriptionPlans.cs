using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedSubscriptionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "69945397-001f-4969-9676-3f221366d3d9", "AQAAAAIAAYagAAAAEHSjmjIo8yEAhd0rUQL8l/3s0Wcznth4OwLSYOgA2RMA/dSAP8/8zEwtIcB4HaVn2Q==", "17b1e92d-6d7e-4840-8bd2-cdc616782d5b" });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "SubscriptionPlanId", "Code", "CreatedBy", "CreatedTime", "Description", "FeaturesJson", "IsActive", "MaxGeneratedExamsPerMonth", "MaxScansPerMonth", "MaxTokensPerMonth", "Name", "PricePerMonth", "Status", "UpdatedBy", "UpdatedTime" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "FREE", "System", new DateTime(2026, 1, 30, 3, 11, 12, 695, DateTimeKind.Utc).AddTicks(2319), "Dành cho người mới bắt đầu, giới hạn tính năng.", null, true, 0, 5, 0, "Gói Cơ Bản (Free)", 0m, null, null, null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "PRO", "System", new DateTime(2026, 1, 30, 3, 11, 12, 695, DateTimeKind.Utc).AddTicks(2331), "Mở khóa toàn bộ tính năng AI & Giải đề.", null, true, 50, 100, 0, "Gói Nâng Cao (Pro)", 2000m, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8587bea5-a2c9-4226-997d-c6dde2f4e016", "AQAAAAIAAYagAAAAEOjx5TSc3ExpT35U47xSooxfOEKtvSaSLiksuXPELw/l2vsNfYAIJa2MWKYQcY8r/Q==", "ce13f78b-b391-438d-8677-47a94cd292ee" });
        }
    }
}
