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
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserUsage> UserUsages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<ExamAttemptDetail> ExamAttemptDetails { get; set; }
        public DbSet<ExamFolder> ExamFolders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Gọi seed data
            ApplicationDbContextSeed.SeedAdminAccount(modelBuilder); // ADMIN
            ApplicationDbContextSeed.SeedSubscriptionPlans(modelBuilder); // SUBSCRIPTION PLAN


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

            // UserSubscription
            modelBuilder.Entity<UserSubscription>()
                .HasKey(us => us.UserSubscriptionId);

            modelBuilder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany() // hoặc WithMany(x => x.Subscriptions) nếu bạn thêm collection vào ApplicationUser
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSubscription>()
                .HasOne(us => us.Plan)
                .WithMany()
                .HasForeignKey(us => us.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSubscription>() // Chỉ cho phép 1 subscription active tại 1 thời điểm (tùy chọn, enforce bằng logic service)
                .HasIndex(us => new { us.UserId, us.Status });

            // UserUsage
            modelBuilder.Entity<UserUsage>()
                .HasKey(uu => uu.UserUsageId);

            modelBuilder.Entity<UserUsage>()
                .HasOne(uu => uu.User)
                .WithMany()
                .HasForeignKey(uu => uu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserUsage>()
                .HasIndex(uu => new { uu.UserId, uu.PeriodStart, uu.PeriodEnd })
                .IsUnique();

            // Order
            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Payment
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentTransactionId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderNumber)
                .HasPrincipalKey(o => o.OrderNumber)
                .OnDelete(DeleteBehavior.Cascade);
            
            // ExamAttempt
                // 1. ExamAttempt nối với User (1 User nộp nhiều bài)
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa user thì xóa luôn bài làm

                // 2. ExamAttempt nối với ExamPaper (1 Đề có nhiều lần làm bài)
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.ExamPaper)
                .WithMany()
                .HasForeignKey(a => a.ExamPaperId)
                .OnDelete(DeleteBehavior.Restrict); // Xóa đề thi không được xóa lịch sử làm bài (để thống kê)
            
            // ExamAttemptDetail
                // 3. Detail thuộc về 1 Attempt (1 Bài làm có nhiều câu trả lời)
            modelBuilder.Entity<ExamAttemptDetail>()
                .HasOne(d => d.ExamAttempt)
                .WithMany(a => a.Details)
                .HasForeignKey(d => d.ExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa bài làm thì xóa luôn chi tiết

                // 4. Detail nối với ExamQuestion (Để biết trả lời cho câu hỏi nào)
            modelBuilder.Entity<ExamAttemptDetail>()
                .HasOne(d => d.ExamQuestion)
                .WithMany()
                .HasForeignKey(d => d.ExamQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // ExamFolder
            modelBuilder.Entity<ExamFolder>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa luôn Folder

            modelBuilder.Entity<ExamPaper>()
                .HasOne(p => p.Folder)
                .WithMany(f => f.ExamPapers)
                .HasForeignKey(p => p.FolderId)
                .OnDelete(DeleteBehavior.SetNull); // Xóa Folder thì Đề thi văng ra ngoài (Set Null) chứ không bị xóa mất
            
            
        }
    }
}

