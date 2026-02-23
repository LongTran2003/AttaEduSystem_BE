using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
