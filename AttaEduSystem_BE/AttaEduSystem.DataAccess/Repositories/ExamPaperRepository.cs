using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamPaperRepository : Repository<ExamPaper>, IExamPaperRepository
    {
        private readonly ApplicationDBContext _context;

        public ExamPaperRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ExamPaper?> GetByIdWithUserAsync(Guid id)
        {
            return await _context.ExamPapers
                .Include(e => e.CreatedBy) // navigation optional nếu bạn giữ
                .FirstOrDefaultAsync(e => e.ExamPaperId == id);
        }

        public async Task<IEnumerable<ExamPaper>> GetByUserIdAsync(string userId)
        {
            return await _context.ExamPapers
                .Where(e => e.CreatedBy == userId)
                .OrderByDescending(e => e.CreatedTime)
                .ToListAsync();
        }
    }
}