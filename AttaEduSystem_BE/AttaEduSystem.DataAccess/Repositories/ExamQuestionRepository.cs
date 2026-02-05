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
    }
}
