using Microsoft.EntityFrameworkCore.Storage;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IUnitOfWork
    {
        IStudentRepository Student { get; }
        ITeacherRepository Teacher { get; }
        IExamPaperRepository ExamPaper { get; }
        IGeneratedExamPaperRepository GeneratedExamPaper { get; }
        IExamQuestionRepository ExamQuestion { get; }
        IQuestionOptionRepository QuestionOption { get; }
        IExamSolutionRepository ExamSolution { get; }
        ISubscriptionPlanRepository SubscriptionPlan { get; }
        IUserSubscriptionRepository UserSubscription { get; }
        IUserUsageRepository UserUsage { get; }
        IOrderRepository Order { get; }
        IPaymentRepository Payment { get; }
        IExamAttemptRepository ExamAttempt { get; }
        IExamAttemptDetailRepository ExamAttemptDetail { get; }
        IExamFolderRepository ExamFolder { get; }
        
        
        Task<int> SaveAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
