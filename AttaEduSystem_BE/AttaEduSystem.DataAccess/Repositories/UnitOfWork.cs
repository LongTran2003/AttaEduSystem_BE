using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;
        public IStudentRepository Student { get; private set; }
        public ITeacherRepository Teacher { get; private set; }
        public IExamPaperRepository ExamPaper { get; private set; }
        public IGeneratedExamPaperRepository GeneratedExamPaper { get; private set; }
        public IExamQuestionRepository ExamQuestion { get; private set; }
        public IQuestionOptionRepository QuestionOption { get; private set; }
        public IExamSolutionRepository ExamSolution { get; private set; }
        public ISubscriptionPlanRepository SubscriptionPlan { get; private set; }
        public IUserSubscriptionRepository UserSubscription { get; private set; }
        public IUserUsageRepository UserUsage { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IPaymentRepository Payment { get; private set; }
        public IExamAttemptRepository ExamAttempt { get; private set; }
        public IExamAttemptDetailRepository ExamAttemptDetail { get; private set; }
        

        public UnitOfWork(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            Student = new StudentRepository(_context);
            Teacher = new TeacherRepository(_context);
            ExamPaper = new ExamPaperRepository(_context);
            GeneratedExamPaper = new GeneratedExamPaperRepository(_context);
            ExamQuestion = new ExamQuestionRepository(_context);
            QuestionOption = new QuestionOptionRepository(_context);
            ExamSolution = new ExamSolutionRepository(_context);
            SubscriptionPlan = new SubscriptionPlanRepository(_context);
            UserSubscription = new UserSubscriptionRepository(_context);
            UserUsage = new UserUsageRepository(_context);
            Order = new OrderRepository(_context);
            Payment = new PaymentRepository(_context);
            ExamAttempt = new ExamAttemptRepository(_context);
            ExamAttemptDetail = new ExamAttemptDetailRepository(_context);
        }



        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
