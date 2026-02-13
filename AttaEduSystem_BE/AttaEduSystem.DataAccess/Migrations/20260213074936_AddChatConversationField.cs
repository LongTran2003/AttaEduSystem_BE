using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddChatConversationField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ChatConversations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExamPaperId",
                table: "ChatConversations",
                type: "uuid",
                nullable: true);

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
                column: "CreatedTime",
                value: new DateTime(2026, 2, 13, 7, 49, 35, 51, DateTimeKind.Utc).AddTicks(1197));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 13, 7, 49, 35, 51, DateTimeKind.Utc).AddTicks(1206));

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_ExamPaperId",
                table: "ChatConversations",
                column: "ExamPaperId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConversations_ExamPapers_ExamPaperId",
                table: "ChatConversations",
                column: "ExamPaperId",
                principalTable: "ExamPapers",
                principalColumn: "ExamPaperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatConversations_ExamPapers_ExamPaperId",
                table: "ChatConversations");

            migrationBuilder.DropIndex(
                name: "IX_ChatConversations_ExamPaperId",
                table: "ChatConversations");

            migrationBuilder.DropColumn(
                name: "ExamPaperId",
                table: "ChatConversations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ChatConversations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a31ad376-fef0-405c-a16b-f6475319a3c3", "AQAAAAIAAYagAAAAELhEMvEITS904oUJDY0xdAmDeZ3/7+xXbbLULBKq5oZOEJVJBLY0BtLFYWfHWOna4g==", "ba8cbf54-67a7-4b60-9267-adc01c831c86" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 12, 11, 34, 48, 213, DateTimeKind.Utc).AddTicks(5817));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 12, 11, 34, 48, 213, DateTimeKind.Utc).AddTicks(5828));
        }
    }
}
