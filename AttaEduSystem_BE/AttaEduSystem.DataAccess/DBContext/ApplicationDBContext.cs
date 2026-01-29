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
        public DbSet<ExamPaper> ExamPapers { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<ExamSolution> ExamSolutions { get; set; }
        public DbSet<GeneratedExamPaper> GeneratedExamPapers { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }


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

            // ExamPaper
            modelBuilder.Entity<ExamPaper>()
            .HasKey(e => e.ExamPaperId);
            
            modelBuilder.Entity<ExamPaper>() // Cấu hình quan hệ 1-Nhiều: ExamPaper -> ExamQuestions
                .HasMany(e => e.Questions) // Cần thêm property này vào Entity ExamPaper (xem mục 2 bên dưới)
                .WithOne(q => q.ExamPaper)
                .HasForeignKey(q => q.ExamPaperId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa đề là xóa hết câu hỏi

            // ExamQuestion
            modelBuilder.Entity<ExamQuestion>()
                .HasKey(q => q.QuestionId);

            modelBuilder.Entity<ExamQuestion>() // Cấu hình quan hệ 1-Nhiều: ExamQuestion -> Options
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa câu hỏi là xóa hết đáp án

            // 3. ExamSolution (Quan hệ 1-1 hoặc 1-Nhiều tùy logic, ở đây tôi để 1-Nhiều cho an toàn)
            modelBuilder.Entity<ExamSolution>()
                .HasKey(s => s.ExamSolutionId);

            modelBuilder.Entity<ExamSolution>()
                .HasOne(s => s.ExamPaper)
                .WithMany()
                .HasForeignKey(s => s.ExamPaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // GeneratedExamPaper
            modelBuilder.Entity<GeneratedExamPaper>()
                .HasKey(g => g.GeneratedExamPaperId);

            modelBuilder.Entity<GeneratedExamPaper>()
                .HasOne(g => g.OriginalExamPaper)
                .WithMany()
                .HasForeignKey(g => g.OriginalExamPaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>()
            .HasKey(p => p.SubscriptionPlanId);

            modelBuilder.Entity<SubscriptionPlan>()
                .HasIndex(p => p.Code)
                .IsUnique(); // mỗi gói 1 code duy nhất, dễ query
        }
    }
}

