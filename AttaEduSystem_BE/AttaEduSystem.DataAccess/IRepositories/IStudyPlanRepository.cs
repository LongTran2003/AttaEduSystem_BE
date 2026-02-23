using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IStudyPlanRepository : IRepository<StudyPlan>
    {
        /// <summary>
        /// Lấy plan active hiện tại của user
        /// </summary>
        Task<StudyPlan?> GetActivePlanAsync(string userId);

        /// <summary>
        /// Lấy plan theo tuần cụ thể
        /// </summary>
        Task<StudyPlan?> GetPlanByWeekAsync(string userId, DateTime weekStart);

        /// <summary>
        /// Lấy lịch sử plans (paginated)
        /// </summary>
        Task<(List<StudyPlan> Plans, int TotalCount)> GetPlanHistoryAsync(string userId, int months, int page, int pageSize);

        /// <summary>
        /// Deactivate tất cả plans cũ của user
        /// </summary>
        Task DeactivateAllPlansAsync(string userId);
        void Update(StudyPlan studyPlan);
    }
}
