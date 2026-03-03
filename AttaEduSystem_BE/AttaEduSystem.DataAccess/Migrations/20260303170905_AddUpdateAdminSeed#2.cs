using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttaEduSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateAdminSeed2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BƯỚC 1: Tạo user mới TRƯỚC
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "BirthDate", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "Gender", "ImageUrl", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "OtpCode", "OtpExpiry", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Status", "TwoFactorEnabled", "UserName" },
                values: new object[] { "AttaEdu-Admin-v2", 0, "System", new DateTime(2001, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "28da35f7-a0b4-4637-808c-ec686c4c9f18", "sysadmin@attaedu.site", true, "System Administrator", null, "https://ui-avatars.com/api/?name=Admin&background=4F46E5&color=fff&size=200", true, null, "SYSADMIN@ATTAEDU.SITE", "SYSADMIN@ATTAEDU.SITE", null, null, "AQAAAAIAAYagAAAAED16Ccq0BwLrPBXRJKn+KMLht1w+/gqLRf84/vKxEgXFXOwBvH5ZZ8sfAoRACbmEMg==", "0987654321", true, "c648283b-4669-4086-9654-c9a5530b58cb", "Active", false, "sysadmin@attaedu.site" });

            // BƯỚC 2: Gán role cho user mới
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "8fa7c7bb-daa5-a660-bf02-82301a5eb32a", "AttaEdu-Admin-v2" });

            // BƯỚC 3: Update tất cả dữ liệu liên quan sang user mới
            migrationBuilder.Sql(@"
                UPDATE ""ExamPapers"" 
                SET ""CreatedBy"" = 'AttaEdu-Admin-v2' 
                WHERE ""CreatedBy"" = 'AttaEdu-Admin';
            ");

            // BƯỚC 4: Xóa role của user cũ
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8fa7c7bb-daa5-a660-bf02-82301a5eb32a", "AttaEdu-Admin" });

            // BƯỚC 5: Xóa user cũ (bây giờ không còn foreign key nào tham chiếu)
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin");

            // BƯỚC 6: Update timestamps
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Tạo lại user cũ TRƯỚC
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "BirthDate", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "Gender", "ImageUrl", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "OtpCode", "OtpExpiry", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Status", "TwoFactorEnabled", "UserName" },
                values: new object[] { "AttaEdu-Admin", 0, "123 Admin St", new DateTime(2001, 6, 5, 0, 0, 0, 0, DateTimeKind.Utc), "ec2f2f32-9132-4395-bd8b-236f24a8eb1a", "sysadmin@attaedu.site", true, "Admin", null, "https://example.com/avatar.png", true, null, "ADMIN@GMAIL.COM", "ADMIN@GMAIL.COM", null, null, "AQAAAAIAAYagAAAAEOa5s6Wmeqq42cCNcLefTFxr/Kr6OgL9/KphQyP+3hEHyEsk0ZmQJ29Bu94Zi19xwQ==", "1234567890", true, "2afa5c96-db6c-477f-b942-3232148a9371", "Active", false, "admin@gmail.com" });

            // Gán role cho user cũ
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "8fa7c7bb-daa5-a660-bf02-82301a5eb32a", "AttaEdu-Admin" });

            // Update dữ liệu về user cũ
            migrationBuilder.Sql(@"
                UPDATE ""ExamPapers"" 
                SET ""CreatedBy"" = 'AttaEdu-Admin' 
                WHERE ""CreatedBy"" = 'AttaEdu-Admin-v2';
            ");

            // Xóa role user mới
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8fa7c7bb-daa5-a660-bf02-82301a5eb32a", "AttaEdu-Admin-v2" });

            // Xóa user mới
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "AttaEdu-Admin-v2");

            // Rollback timestamps
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
    }
}