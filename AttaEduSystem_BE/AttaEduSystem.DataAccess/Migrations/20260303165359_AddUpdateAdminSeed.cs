using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "Email", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ec2f2f32-9132-4395-bd8b-236f24a8eb1a", "sysadmin@attaedu.site", "AQAAAAIAAYagAAAAEOa5s6Wmeqq42cCNcLefTFxr/Kr6OgL9/KphQyP+3hEHyEsk0ZmQJ29Bu94Zi19xwQ==", "2afa5c96-db6c-477f-b942-3232148a9371" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 3, 3, 16, 53, 58, 174, DateTimeKind.Utc).AddTicks(8874));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 3, 3, 16, 53, 58, 174, DateTimeKind.Utc).AddTicks(8957));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "Email", "PasswordHash", "SecurityStamp" },
                values: new object[] { "58b3ba04-6ccf-44ca-a525-ed71d198f0b3", "admin@gmail.com", "AQAAAAIAAYagAAAAEAgFtt2VxxwFCVYpPjzI935SHj/qbBVPSrQ+HYRSUTG8gOe6cri0q8DLnVEemWbrRQ==", "e2b3bc99-66f7-47e0-86e7-009096e038c0" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 26, 13, 45, 10, 428, DateTimeKind.Utc).AddTicks(8726));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 26, 13, 45, 10, 428, DateTimeKind.Utc).AddTicks(8747));
        }
    }
}
