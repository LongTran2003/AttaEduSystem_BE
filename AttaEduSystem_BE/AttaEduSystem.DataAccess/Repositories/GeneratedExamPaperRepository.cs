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

        public async Task<(List<GeneratedExamPaper> Items, int TotalCount)> GetGeneratedExamsAsync(
        int pageNumber,
        int pageSize,
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        string? includeProperties = null)
        {
            var query = _context.GeneratedExamPapers
                .Include(g => g.OriginalExamPaper)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                var kw = filterQuery.Trim().ToLowerInvariant();
                query = filterOn.ToLowerInvariant() switch
                {
                    "status" => query.Where(g => g.Status != null && g.Status.ToLower().Contains(kw)),
                    "createdby" => query.Where(g => g.CreatedBy != null && g.CreatedBy.ToLower().Contains(kw)),
                    "aimodel" => query.Where(g => g.AiModelUsed != null && g.AiModelUsed.ToLower().Contains(kw)),
                    "originaltitle" => query.Where(g => g.OriginalExamPaper != null && g.OriginalExamPaper.Title.ToLower().Contains(kw)),
                    _ => query
                };
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop.Trim());
                }
            }

            var totalCount = await query.CountAsync();

            query = (sortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "createdat" => query.OrderBy(g => g.CreatedTime),
                "createdat_desc" => query.OrderByDescending(g => g.CreatedTime),
                _ => query.OrderByDescending(g => g.CreatedTime)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<GeneratedExamPaper?> GetByIdWithOriginalAsync(Guid generatedExamId)
        {
            return await _context.GeneratedExamPapers
                .Include(g => g.OriginalExamPaper)
                .FirstOrDefaultAsync(g => g.GeneratedExamPaperId == generatedExamId);
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
                .Include(g => g.OriginalExamPaper)
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
