using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamSolutionRepository : IRepository<ExamSolution>
    {
        Task<(List<ExamSolution> Solutions, int TotalCount)> GetSolutionsAsync(
        int pageNumber,
        int pageSize,
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        string? includeProperties = null);

        Task<ExamSolution?> GetByIdWithExamAsync(Guid solutionId);

        Task<IEnumerable<ExamSolution>> GetByUserIdAsync(string userId);
        void Update(ExamSolution examSolution);
    }
}
