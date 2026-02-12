using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamSolutionRepository : Repository<ExamSolution>, IExamSolutionRepository
    {
        private readonly ApplicationDBContext _context;
        public ExamSolutionRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<ExamSolution> Solutions, int TotalCount)> GetSolutionsAsync(
        int pageNumber,
        int pageSize,
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        string? includeProperties = null)
        {
            var query = _context.ExamSolutions
                .Include(s => s.ExamPaper) // include exam metadata for filtering/mapping
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                var kw = filterQuery.Trim().ToLowerInvariant();
                query = filterOn.ToLowerInvariant() switch
                {
                    "status" => query.Where(s => s.Status != null && s.Status.ToLower().Contains(kw)),
                    "createdby" => query.Where(s => s.CreatedBy != null && s.CreatedBy.ToLower().Contains(kw)),
                    "examtitle" => query.Where(s => s.ExamPaper != null && s.ExamPaper.Title.ToLower().Contains(kw)),
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
                "createdat" => query.OrderBy(s => s.CreatedTime),
                "createdat_desc" => query.OrderByDescending(s => s.CreatedTime),
                _ => query.OrderByDescending(s => s.CreatedTime)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<ExamSolution?> GetByIdWithExamAsync(Guid solutionId)
        {
            return await _context.ExamSolutions
                .Include(s => s.ExamPaper)
                .FirstOrDefaultAsync(s => s.ExamSolutionId == solutionId);
        }

        public async Task<IEnumerable<ExamSolution>> GetByUserIdAsync(string userId)
        {
            return await _context.ExamSolutions
                .Include(s => s.ExamPaper)
                .Where(s => s.CreatedBy == userId)
                .OrderByDescending(s => s.CreatedTime)
                .ToListAsync();
        }

        public void Update(ExamSolution examSolution)
        {
            _context.ExamSolutions.Update(examSolution);
        }
    }
}
