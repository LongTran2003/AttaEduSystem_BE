using AttaEduSystem.Models.Entities;
using AttaEduSystem.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Seed
{
    public class ApplicationDbContextSeed
    {
        public static void SeedAdminAccount(ModelBuilder modelBuilder)
        {
            {
                var adminRoleId = "8fa7c7bb-daa5-a660-bf02-82301a5eb32a";

                var adminUserId = "AttaEdu-Admin";

                var roles = new List<IdentityRole>
        {

            new()
            {
                Id = adminRoleId,
                ConcurrencyStamp = StaticUserRoles.Admin,
                Name = StaticUserRoles.Admin,
                NormalizedName = StaticUserRoles.Admin.ToUpper()
            }
        };

                modelBuilder.Entity<IdentityRole>().HasData(roles);

                var hasher = new PasswordHasher<ApplicationUser>();

                var adminUser = new ApplicationUser
                {
                    Id = adminUserId,
                    FullName = "Admin",
                    BirthDate = DateTime.SpecifyKind(new DateTime(2001, 6, 5), DateTimeKind.Utc),
                    ImageUrl = "https://example.com/avatar.png",
                    Address = "123 Admin St",
                    UserName = "admin@gmail.com",
                    NormalizedUserName = "ADMIN@GMAIL.COM",
                    Email = "admin@gmail.com",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, "Admin123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnd = null,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };



                modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

                // Assigning the admin role to the admin user (ĐÚNG CÁCH)
                modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                });
            }
        }

        public static void SeedSubscriptionPlans(ModelBuilder modelBuilder)
        {
            // 1. Gói FREE (Mặc định)
            var freePlan = new SubscriptionPlan
            {
                SubscriptionPlanId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // ID cố định để tránh duplicate khi chạy lại
                Code = "FREE",
                Name = "Gói Cơ Bản (Free)",
                Description = "Dành cho người mới bắt đầu, giới hạn tính năng.",
                Price = 0, // Miễn phí
                DurationInDays = 30, // Mặc định 30 ngày
                MaxScansPerMonth = 5, // Cho scan thử 5 lần
                MaxGeneratedExamsPerMonth = 0, // Không cho tạo đề
                MaxSolvesPerMonth = 0, // Free không được giải đề
                // MaxTokensPerMonth = 1000, // (Nếu entity bạn có field này)
                Status = "Active",
                CreatedBy = "System",
                CreatedTime = DateTime.UtcNow
            };

            // 2. Gói PRO (Giá sinh viên nghèo vượt khó: 2k VNĐ)
            var proPlan = new SubscriptionPlan
            {
                SubscriptionPlanId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "PRO",
                Name = "Gói Nâng Cao (Pro)",
                Description = "Mở khóa toàn bộ tính năng AI & Giải đề.",
                Price = 2000, // 2,000 VND (Rẻ hơn ly trà đá để test PayOS)
                DurationInDays = 30, // Gói này có thời hạn 30 ngày
                MaxScansPerMonth = 100, // Scan thoải mái
                MaxGeneratedExamsPerMonth = 50, // Tạo đề thoải mái
                MaxSolvesPerMonth = 100, // Solve tùm lum
                // MaxTokensPerMonth = 100000, 
                Status = "Active",
                CreatedBy = "System",
                CreatedTime = DateTime.UtcNow
            };

            modelBuilder.Entity<SubscriptionPlan>().HasData(freePlan, proPlan);
        }
    }
}
