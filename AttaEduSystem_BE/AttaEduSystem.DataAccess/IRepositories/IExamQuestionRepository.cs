using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamQuestionRepository : IRepository<ExamQuestion>
    {
        Task<List<ExamQuestion>> GetByExamPaperIdAsync(Guid examPaperId);
        Task<List<ExamQuestion>> GetByExamPaperIdWithOptionsAsync(Guid examPaperId);
        Task<ExamQuestion?> GetByIdWithOptionsAsync(Guid questionId);
        void Update(ExamQuestion examQuestion);
        void UpdateRange(IEnumerable<ExamQuestion> examQuestions);

        // Question bank search with filters and pagination
        Task<(List<ExamQuestion> Questions, int TotalCount)> SearchQuestionsAsync(
            string? searchTerm,
            string? questionType,
            string? difficultyLevel,
            string? subject,
            string? userId,
            string scope,
            int pageNumber,
            int pageSize);
    }
}
