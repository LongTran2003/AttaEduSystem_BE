using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EnrollKeyClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnrollKey",
                table: "LearningClasses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin-v2",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "821fc963-9f9a-424d-817c-e3aa21175a51", "AQAAAAIAAYagAAAAEOgkAkCp4WHW3VZRWEcD4hMcdFPd2uD0grl3gUObcZFg0ISDuSBKAuWiXhdobGJu4w==", "8da276d7-8a3c-42b3-abba-a7402d610d3d" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 4, 2, 20, 50, 29, 359, DateTimeKind.Utc).AddTicks(9758));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedTime", "MaxTokensPerMonth" },
                values: new object[] { new DateTime(2026, 4, 2, 20, 50, 29, 359, DateTimeKind.Utc).AddTicks(9778), 50000 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollKey",
                table: "LearningClasses");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin-v2",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d052885c-334f-48ed-8346-64afa75b0711", "AQAAAAIAAYagAAAAEA+S8M8VONnzpEcoZSZ/XBIKIM1gl+KBWClRRLe/IRAJ21dur2sVUTQs1sygkg7XJQ==", "67d0a3b3-4315-4411-84af-181a3d7cddd5" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 3, 25, 17, 17, 45, 636, DateTimeKind.Utc).AddTicks(2964));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedTime", "MaxTokensPerMonth" },
                values: new object[] { new DateTime(2026, 3, 25, 17, 17, 45, 636, DateTimeKind.Utc).AddTicks(2979), 0 });
        }
    }
}
