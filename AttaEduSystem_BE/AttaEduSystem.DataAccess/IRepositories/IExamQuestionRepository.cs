using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamQuestionRepository : IRepository<ExamQuestion>
    {
        Task<List<ExamQuestion>> GetByExamPaperIdAsync(Guid examPaperId);
        Task<ExamQuestion?> GetByIdWithOptionsAsync(Guid questionId);
        void Update(ExamQuestion examQuestion);
        void UpdateRange(IEnumerable<ExamQuestion> examQuestions);
    }
}
