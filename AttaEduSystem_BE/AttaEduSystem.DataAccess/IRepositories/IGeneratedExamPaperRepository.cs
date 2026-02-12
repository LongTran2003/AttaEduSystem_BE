using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IGeneratedExamPaperRepository : IRepository<GeneratedExamPaper>
    {
        Task<(List<GeneratedExamPaper> Items, int TotalCount)> GetGeneratedExamsAsync(
        int pageNumber,
        int pageSize,
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        string? includeProperties = null);

        Task<GeneratedExamPaper?> GetByIdWithOriginalAsync(Guid generatedExamId);

        Task<IEnumerable<GeneratedExamPaper>> GetByOriginalExamAsync(Guid examPaperId);
        Task<IEnumerable<GeneratedExamPaper>> GetByUserAsync(string userId);
        void Update (GeneratedExamPaper examPaper);
    }
}
