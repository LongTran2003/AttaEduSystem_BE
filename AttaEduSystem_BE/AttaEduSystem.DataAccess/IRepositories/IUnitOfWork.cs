using Microsoft.EntityFrameworkCore.Storage;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IUnitOfWork
    {
        IStudentRepository Student { get; }
        ITeacherRepository Teacher { get; }
        IExamPaperRepository ExamPaper { get; }
        IGeneratedExamPaperRepository GeneratedExamPaper { get; }

        Task<int> SaveAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
