using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddExamRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ExamRooms",
                columns: table => new
                {
                    ExamRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeLimit = table.Column<int>(type: "integer", nullable: false),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRooms", x => x.ExamRoomId);
                    table.ForeignKey(
                        name: "FK_ExamRooms_ExamPapers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalTable: "ExamPapers",
                        principalColumn: "ExamPaperId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamRoomParticipants",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExamAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRoomParticipants", x => x.ParticipantId);
                    table.ForeignKey(
                        name: "FK_ExamRoomParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRoomParticipants_ExamAttempts_ExamAttemptId",
                        column: x => x.ExamAttemptId,
                        principalTable: "ExamAttempts",
                        principalColumn: "ExamAttemptId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExamRoomParticipants_ExamRooms_ExamRoomId",
                        column: x => x.ExamRoomId,
                        principalTable: "ExamRooms",
                        principalColumn: "ExamRoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "47282f92-b1cf-4697-b63e-4a55405bfd32", "AQAAAAIAAYagAAAAEAOcXh6EHoKy8bOjZV7MauNq70aCJl8JVQYIP7hrfOweKrQIfVM8hQXKEb1IJ42GJg==", "ed3df71e-04c8-4dfc-98b4-6ffc52c20c5a" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 10, 4, 59, 46, 508, DateTimeKind.Utc).AddTicks(4302));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 10, 4, 59, 46, 508, DateTimeKind.Utc).AddTicks(4313));

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomParticipants_ExamAttemptId",
                table: "ExamRoomParticipants",
                column: "ExamAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomParticipants_ExamRoomId_UserId",
                table: "ExamRoomParticipants",
                columns: new[] { "ExamRoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomParticipants_UserId",
                table: "ExamRoomParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRooms_ExamPaperId",
                table: "ExamRooms",
                column: "ExamPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRooms_RoomCode",
                table: "ExamRooms",
                column: "RoomCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamRoomParticipants");

            migrationBuilder.DropTable(
                name: "ExamRooms");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "Students");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "abff5794-3c57-44eb-8974-81e5ab796804", "AQAAAAIAAYagAAAAEOMiZTsB3mrOTeejh24DXEat7Q18RclZZVrtAARPXBssEiSQFL5WvTxbv1F+Nii/Wg==", "011c6007-f548-463b-acc5-88b02ac856aa" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 6, 14, 22, 20, 534, DateTimeKind.Utc).AddTicks(2069));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 2, 6, 14, 22, 20, 534, DateTimeKind.Utc).AddTicks(2079));
        }
    }
}
