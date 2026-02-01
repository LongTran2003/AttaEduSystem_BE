using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class GeneratedExamPaperRepository : Repository<GeneratedExamPaper>, IGeneratedExamPaperRepository
    {
        private readonly ApplicationDBContext _context;

        public GeneratedExamPaperRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GeneratedExamPaper>> GetByOriginalExamAsync(Guid examPaperId)
        {
            return await _context.GeneratedExamPapers
                .Where(g => g.OriginalExamPaperId == examPaperId)
                .OrderByDescending(g => g.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<GeneratedExamPaper>> GetByUserAsync(string userId)
        {
            return await _context.GeneratedExamPapers
                .Where(g => g.CreatedBy == userId)
                .OrderByDescending(g => g.CreatedTime)
                .ToListAsync();
        }

        public void Update(GeneratedExamPaper examPaper)
        {
            _context.GeneratedExamPapers.Update(examPaper);
        }
    }   
}
