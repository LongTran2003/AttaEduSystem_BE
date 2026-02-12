using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class SharedExamRepository : Repository<SharedExam>, ISharedExamRepository
    {
        private readonly ApplicationDBContext _context;

        public SharedExamRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SharedExam?> GetByTokenAsync(string token)
        {
            return await _context.SharedExams
                .FirstOrDefaultAsync(s => s.ShareToken == token);
        }

        public async Task<IEnumerable<SharedExam>> GetByUserIdAsync(string userId)
        {
            return await _context.SharedExams
                .Where(s => s.CreatedBy == userId)
                .OrderByDescending(s => s.CreatedTime)
                .ToListAsync();
        }

        public async Task<bool> TokenExistsAsync(string token)
        {
            return await _context.SharedExams.AnyAsync(s => s.ShareToken == token);
        }

        public void Update(SharedExam sharedExam)
        {
            _context.SharedExams.Update(sharedExam);
        }
    }
}
