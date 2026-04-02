using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories;

public class ExamAttemptRepository : Repository<ExamAttempt>, IExamAttemptRepository
{
    private readonly ApplicationDBContext _context;
    
    public ExamAttemptRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }


    public async Task<List<ExamAttempt>> GetHistoryByUserIdAsync(string userId)
    {
        return await _context.ExamAttempts
            .Include(x => x.ExamPaper) // JOIN để lấy Title đề thi
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CompletedAt) // Mới nhất lên đầu
            .ToListAsync();
    }

    public async Task<ExamAttempt?> GetAttemptWithDetailsAsync(Guid attemptId)
    {
        return await _context.ExamAttempts
            .Include(x => x.ExamPaper) // Lấy tên đề
            .Include(x => x.Details)   // Lấy danh sách câu trả lời
            .ThenInclude(d => d.ExamQuestion)
            .ThenInclude(q => q.Options) // Lấy options để resolve đáp án đúng theo IsCorrect khi cần
            .FirstOrDefaultAsync(x => x.ExamAttemptId == attemptId);
    }

    public void Update(ExamAttempt examAttempt)
    {
        _context.ExamAttempts.Update(examAttempt);
    }
}
