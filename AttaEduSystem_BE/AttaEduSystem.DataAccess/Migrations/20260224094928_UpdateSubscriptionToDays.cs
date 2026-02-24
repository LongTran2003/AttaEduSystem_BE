using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionToDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerMonth",
                table: "SubscriptionPlans",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "DurationInDays",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a3795540-0c6c-4e32-b79c-422612ad0108", "AQAAAAIAAYagAAAAEPYlc9MCFod+PdQPUmpr4eGXbKRZ1tVHLhg38iMXI0XSBq5/imrqBNik1ZV/DjnPVw==", "ffb41188-814c-4386-86f5-564a84d75aa0" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedTime", "DurationInDays" },
                values: new object[] { new DateTime(2026, 2, 24, 9, 49, 26, 903, DateTimeKind.Utc).AddTicks(907), 30 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedTime", "DurationInDays" },
                values: new object[] { new DateTime(2026, 2, 24, 9, 49, 26, 903, DateTimeKind.Utc).AddTicks(921), 30 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInDays",
                table: "SubscriptionPlans");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SubscriptionPlans",
                newName: "PricePerMonth");

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
        }
    }
}
