using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamQuestionRepository : Repository<ExamQuestion>, IExamQuestionRepository
    {
        private readonly ApplicationDBContext _context;
        public ExamQuestionRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ExamQuestion>> GetByExamPaperIdAsync(Guid examPaperId)
        {
            // Lấy tất cả câu hỏi của đề thi đó
            // AsNoTracking giúp tăng tốc độ đọc vì chỉ lấy ra để chấm điểm
            return await _context.ExamQuestions
                .Where(q => q.ExamPaperId == examPaperId)
                .ToListAsync();
        }

        public async Task<List<ExamQuestion>> GetByExamPaperIdWithOptionsAsync(Guid examPaperId)
        {
            return await _context.ExamQuestions
                .Include(q => q.Options)
                .Where(q => q.ExamPaperId == examPaperId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();
        }

        public async Task<ExamQuestion?> GetByIdWithOptionsAsync(Guid questionId)
        {
            return await _context.ExamQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }

        public void Update(ExamQuestion examQuestion)
        {
            _context.ExamQuestions.Update(examQuestion);
        }

        public void UpdateRange(IEnumerable<ExamQuestion> examQuestions)
        {
            _context.ExamQuestions.UpdateRange(examQuestions);
        }

        public async Task<(List<ExamQuestion> Questions, int TotalCount)> SearchQuestionsAsync(
            string? searchTerm,
            string? questionType,
            string? difficultyLevel,
            string? subject,
            string? userId,
            string scope,
            int pageNumber,
            int pageSize)
        {
            var query = _context.ExamQuestions
                .Include(q => q.Options)
                .Include(q => q.ExamPaper)
                .AsQueryable();

            // Filter by scope
            if (scope == "My" && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(q => q.ExamPaper.CreatedBy == userId || q.ExamPaper.Creator!.Id == userId);
            }
            else if (scope == "Public")
            {
                query = query.Where(q => q.ExamPaper.Status == "Ready");
            }
            else // "All"
            {
                query = query.Where(q =>
                    q.ExamPaper.Status == "Ready" ||
                    q.ExamPaper.CreatedBy == userId ||
                    q.ExamPaper.Creator!.Id == userId);
            }

            // Filter by search term (content)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(q => q.Content.Contains(searchTerm));
            }

            // Filter by question type
            if (!string.IsNullOrEmpty(questionType))
            {
                query = query.Where(q => q.QuestionType == questionType);
            }

            // Filter by difficulty level
            if (!string.IsNullOrEmpty(difficultyLevel))
            {
                query = query.Where(q => q.DifficultyLevel == difficultyLevel);
            }

            // Filter by subject
            if (!string.IsNullOrEmpty(subject))
            {
                query = query.Where(q => q.ExamPaper.Subject == subject);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Pagination
            var questions = await query
                .OrderByDescending(q => q.CreatedTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (questions, totalCount);
        }
    }
}
