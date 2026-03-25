using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningClassManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningClasses",
                columns: table => new
                {
                    LearningClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchoolYear = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Schedule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OwnerUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningClasses", x => x.LearningClassId);
                    table.ForeignKey(
                        name: "FK_LearningClasses_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningClassMembers",
                columns: table => new
                {
                    LearningClassMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningClassMembers", x => x.LearningClassMemberId);
                    table.ForeignKey(
                        name: "FK_LearningClassMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningClassMembers_LearningClasses_LearningClassId",
                        column: x => x.LearningClassId,
                        principalTable: "LearningClasses",
                        principalColumn: "LearningClassId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                column: "CreatedTime",
                value: new DateTime(2026, 3, 25, 17, 17, 45, 636, DateTimeKind.Utc).AddTicks(2979));

            migrationBuilder.CreateIndex(
                name: "IX_LearningClasses_OwnerUserId",
                table: "LearningClasses",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningClassMembers_LearningClassId_UserId",
                table: "LearningClassMembers",
                columns: new[] { "LearningClassId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningClassMembers_UserId",
                table: "LearningClassMembers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningClassMembers");

            migrationBuilder.DropTable(
                name: "LearningClasses");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin-v2",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "28da35f7-a0b4-4637-808c-ec686c4c9f18", "AQAAAAIAAYagAAAAED16Ccq0BwLrPBXRJKn+KMLht1w+/gqLRf84/vKxEgXFXOwBvH5ZZ8sfAoRACbmEMg==", "c648283b-4669-4086-9654-c9a5530b58cb" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedTime",
                value: new DateTime(2026, 3, 4, 0, 9, 4, 857, DateTimeKind.Utc).AddTicks(9554));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "SubscriptionPlanId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedTime",
                value: new DateTime(2026, 3, 4, 0, 9, 4, 857, DateTimeKind.Utc).AddTicks(9554));
        }
    }
}
