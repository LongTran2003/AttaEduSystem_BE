using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.StudyPlan;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IStudyPlanService
    {
        /// <summary>Generate AI weekly plan (with optional SaveImmediately)</summary>
        Task<ResponseDto> GenerateWeeklyPlanAsync(GeneratePlanDto dto, ClaimsPrincipal user);

        /// <summary>Save a generated preview plan (only needed when SaveImmediately=false)</summary>
        Task<ResponseDto> SavePlanAsync(Guid planId, SavePlanDto dto, ClaimsPrincipal user);

        /// <summary>Save generated plan (overload: pass full DTO from FE)</summary>
        Task<ResponseDto> SavePlanAsync(WeeklyStudyPlanDto planDto, SavePlanDto saveDto, ClaimsPrincipal user);

        /// <summary>Get saved plan by week start date</summary>
        Task<ResponseDto> GetPlanByWeekAsync(DateTime weekStart, ClaimsPrincipal user);

        /// <summary>Get active plan</summary>
        Task<ResponseDto> GetActivePlanAsync(ClaimsPrincipal user);

        /// <summary>Get today's sessions from active plan (VN timezone)</summary>
        Task<ResponseDto> GetTodayPlanAsync(ClaimsPrincipal user);

        /// <summary>Get all plans in a date range (max 31 days)</summary>
        Task<ResponseDto> GetPlansByRangeAsync(ClaimsPrincipal user, DateTime from, DateTime to);

        /// <summary>Get plan history (last N months), paginated</summary>
        Task<ResponseDto> GetPlanHistoryAsync(ClaimsPrincipal user, int months = 3, int page = 1, int pageSize = 10);

        /// <summary>Get plan detail by planId</summary>
        Task<ResponseDto> GetPlanDetailAsync(Guid planId, ClaimsPrincipal user);

        /// <summary>Soft-delete a saved plan</summary>
        Task<ResponseDto> DeletePlanAsync(Guid planId, ClaimsPrincipal user);

        /// <summary>Update session completion status</summary>
        Task<ResponseDto> UpdateSessionStatusAsync(Guid planId, UpdateSessionDto dto, ClaimsPrincipal user);

        /// <summary>Regenerate remaining days of an existing plan</summary>
        Task<ResponseDto> RegeneratePlanAsync(Guid planId, RegeneratePlanDto dto, ClaimsPrincipal user);
    }
}
