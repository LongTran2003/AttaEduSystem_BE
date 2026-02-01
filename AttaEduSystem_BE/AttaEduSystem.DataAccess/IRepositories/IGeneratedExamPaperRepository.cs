using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IGeneratedExamPaperRepository : IRepository<GeneratedExamPaper>
    {
        Task<IEnumerable<GeneratedExamPaper>> GetByOriginalExamAsync(Guid examPaperId);
        Task<IEnumerable<GeneratedExamPaper>> GetByUserAsync(string userId);
        void Update (GeneratedExamPaper examPaper);
    }
}
