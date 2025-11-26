using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamPaperRepository : IRepository<ExamPaper>
    {
        Task<ExamPaper?> GetByIdWithUserAsync(Guid examPaperId);
        Task<IEnumerable<ExamPaper>> GetByUserIdAsync(string userId);
    }
}