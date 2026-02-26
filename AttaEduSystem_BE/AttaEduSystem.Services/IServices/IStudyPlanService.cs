using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.StudyPlan;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IStudyPlanService
    {
        /// <summary>
        /// Generate weekly study plan (not saved yet)
        /// </summary>
        Task<ResponseDto> GenerateWeeklyPlanAsync(GeneratePlanDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Save generated plan to DB (minimal signature uses planId)
        /// </summary>
        Task<ResponseDto> SavePlanAsync(Guid planId, SavePlanDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Save generated plan to DB (overload: pass full DTO from FE)
        /// </summary>
        Task<ResponseDto> SavePlanAsync(WeeklyStudyPlanDto planDto, SavePlanDto saveDto, ClaimsPrincipal user);

        /// <summary>
        /// Get saved plan by week
        /// </summary>
        Task<ResponseDto> GetPlanByWeekAsync(DateTime weekStart, ClaimsPrincipal user);

        /// <summary>
        /// Get active plan
        /// </summary>
        Task<ResponseDto> GetActivePlanAsync(ClaimsPrincipal user);

        /// <summary>
        /// Get plan history (last N months)
        /// </summary>
        Task<ResponseDto> GetPlanHistoryAsync(ClaimsPrincipal user, int months = 3, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get plan detail by planId
        /// </summary>
        Task<ResponseDto> GetPlanDetailAsync(Guid planId, ClaimsPrincipal user);

        /// <summary>
        /// Delete saved plan
        /// </summary>
        Task<ResponseDto> DeletePlanAsync(Guid planId, ClaimsPrincipal user);

        /// <summary>
        /// Update session completion status
        /// </summary>
        Task<ResponseDto> UpdateSessionStatusAsync(Guid planId, UpdateSessionDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Regenerate plan for remaining days
        /// </summary>
        Task<ResponseDto> RegeneratePlanAsync(Guid planId, RegeneratePlanDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Get subject performance analysis (for debugging/frontend chart)
        /// </summary>
        Task<ResponseDto> GetSubjectPerformanceAsync(ClaimsPrincipal user, int lookbackDays = 30);
    }
}
