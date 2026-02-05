using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddExamTaking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "ExamQuestions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyLevel",
                table: "ExamQuestions",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ef2a786f-898c-4d88-9b02-8c69b1039056", "AQAAAAIAAYagAAAAEBmjiV6Q+FiW9R+Y7rAj1kJwvCOz6U0rig5MP/egsbxFc4seodYHOpMdiCJ6yUekqQ==", "257c8789-5b5a-4165-9b13-3d57912b35fb" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 5, 4, 22, 16, 160, DateTimeKind.Utc).AddTicks(917));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 5, 4, 22, 16, 160, DateTimeKind.Utc).AddTicks(933));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "ExamQuestions");

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
                column: "CreatedTime",
                value: new DateTime(2026, 2, 1, 4, 52, 45, 6, DateTimeKind.Utc).AddTicks(8175));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 1, 4, 52, 45, 6, DateTimeKind.Utc).AddTicks(8184));
        }
    }
}
