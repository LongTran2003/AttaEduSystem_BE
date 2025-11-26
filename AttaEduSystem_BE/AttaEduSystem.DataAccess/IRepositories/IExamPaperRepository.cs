using AttaEduSystem.Models.Entities;
using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamPaperRepository : IRepository<ExamPaper>
    {
        Task<ExamPaper?> GetByIdWithUserAsync(Guid examPaperId);
        Task<IEnumerable<ExamPaper>> GetByUserIdAsync(string userId);
        Task<(List<ExamPaper> Papers, int TotalCount)> GetExamPapersAsync(
            int pageNumber,
            int pageSize,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            string? includeProperties = null,
            string status = StaticOperationStatus.ExamPaper.Ready);
    }
}