using AttaEduSystem.DataAccess.Seed;
using AttaEduSystem.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.DBContext
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        // DbSet các entity, sắp xếp A-Z
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed admin account
            ApplicationDbContextSeed.SeedAdminAccount(modelBuilder);

            // Thêm các cấu hình khác nếu cần
            // Student
            modelBuilder.Entity<Student>()
                .HasKey(s => s.StudentId);
            modelBuilder.Entity<Student>()
                .HasOne(s => s.ApplicationUser)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            // Teacher
            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.TeacherId);
        }
    }
}

