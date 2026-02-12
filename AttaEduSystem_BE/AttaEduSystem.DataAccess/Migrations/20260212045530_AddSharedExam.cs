using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5588eb8b-94fc-4ff8-a3c9-03a74b1270aa", "AQAAAAIAAYagAAAAENf5jraDiPIWU5Rfs9BXJbDLm22TRnlNr+UGwSm08IUfBcEt8+ruQWGavevUWKZx8g==", "7169e015-f55a-4b5f-8659-4e5b9ce03d34" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 12, 4, 55, 28, 913, DateTimeKind.Utc).AddTicks(9122));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 12, 4, 55, 28, 913, DateTimeKind.Utc).AddTicks(9132));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "cafdbf7c-aca1-45aa-8e97-e2299ac6f771", "AQAAAAIAAYagAAAAELa7yDspd6NE75hln35njJDifU939uLxjJDrypOZcXMPQNZGiwzHZzRTe7t0TGENiQ==", "4842f30b-613a-455a-8bc9-b70729d94394" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 10, 6, 22, 18, 553, DateTimeKind.Utc).AddTicks(8819));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 10, 6, 22, 18, 553, DateTimeKind.Utc).AddTicks(8827));
        }
    }
}
