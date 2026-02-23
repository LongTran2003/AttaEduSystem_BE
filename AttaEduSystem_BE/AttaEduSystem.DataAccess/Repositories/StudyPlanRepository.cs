using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Utilities.Constants;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class StudyPlanRepository : Repository<StudyPlan>, IStudyPlanRepository
    {
        private readonly ApplicationDBContext _context;

        public StudyPlanRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<StudyPlan?> GetActivePlanAsync(string userId)
        {
            return await _context.StudyPlans
                .Where(sp => sp.UserId == userId && 
                        sp.Status == StaticOperationStatus.BaseEntity.Active)
                .OrderByDescending(sp => sp.CreatedTime)
                .FirstOrDefaultAsync();
        }

        public async Task<StudyPlan?> GetPlanByWeekAsync(string userId, DateTime weekStart)
        {
            // Normalize to Monday 00:00:00
            var normalizedWeekStart = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek + (int)DayOfWeek.Monday);

            return await _context.StudyPlans
                .Where(sp => sp.UserId == userId && sp.WeekStart == normalizedWeekStart)
                .FirstOrDefaultAsync();
        }

        public async Task<(List<StudyPlan> Plans, int TotalCount)> GetPlanHistoryAsync(
            string userId, 
            int months, 
            int page, 
            int pageSize)
        {
            var cutoffDate = DateTime.UtcNow.AddMonths(-months);

            var query = _context.StudyPlans
                .Where(sp => sp.UserId == userId && sp.CreatedTime >= cutoffDate)
                .OrderByDescending(sp => sp.WeekStart);

            var totalCount = await query.CountAsync();
            var plans = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (plans, totalCount);
        }

        public async Task DeactivateAllPlansAsync(string userId)
        {
            var activePlans = await _context.StudyPlans
                .Where(sp => sp.UserId == userId && 
                        sp.Status == StaticOperationStatus.BaseEntity.Active)
                .ToListAsync();

            foreach (var plan in activePlans)
            {
                plan.Status = StaticOperationStatus.BaseEntity.Inactive;
                plan.UpdatedTime = DateTime.UtcNow;
            }

            if (activePlans.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public void Update (StudyPlan studyPlan)
        {
            _context.StudyPlans.Update(studyPlan);
        }

    }
}
