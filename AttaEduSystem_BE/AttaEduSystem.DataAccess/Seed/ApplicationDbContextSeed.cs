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

                var adminUserId = "Movok-Admin";

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
    }
}
