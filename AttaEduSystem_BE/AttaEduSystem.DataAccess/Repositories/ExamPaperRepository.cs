using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Utilities.Constants;
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
                .Include(e => e.Creator) // navigation optional nếu bạn giữ
                .Include(e => e.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.Options) // Lấy đáp án A, B, C, D
                .FirstOrDefaultAsync(e => e.ExamPaperId == id);
        }

        public async Task<IEnumerable<ExamPaper>> GetByUserIdAsync(string userId)
        {
            return await _context.ExamPapers
                .Include(e => e.Creator)
                .Where(e => e.CreatedBy == userId)
                .OrderByDescending(e => e.CreatedTime)
                .ToListAsync();
        }

        public async Task<(List<ExamPaper> Papers, int TotalCount)> GetExamPapersAsync(
            int pageNumber,
            int pageSize,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            string? includeProperties = null,
            string status = StaticOperationStatus.ExamPaper.Ready)
        {
            var query = _context.ExamPapers
                .Include(e => e.Creator)
                .Where(e => e.Status == status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                var keyword = filterQuery.Trim().ToLowerInvariant();
                query = filterOn.ToLowerInvariant() switch
                {
                    "title" => query.Where(e => e.Title.ToLower().Contains(keyword)),
                    "subject" => query.Where(e => e.Subject != null && e.Subject.ToLower().Contains(keyword)),
                    "author" => query.Where(e => e.CreatedBy != null &&
                                                  e.CreatedBy.ToLower().Contains(keyword)),
                    _ => query
                };
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property.Trim());
                }
            }

            var totalCount = await query.CountAsync();

            query = (sortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "title_desc" => query.OrderByDescending(e => e.Title),
                "title" => query.OrderBy(e => e.Title),
                "subject_desc" => query.OrderByDescending(e => e.Subject),
                "subject" => query.OrderBy(e => e.Subject),
                "created_desc" => query.OrderByDescending(e => e.CreatedTime),
                _ => query.OrderBy(e => e.CreatedTime)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}