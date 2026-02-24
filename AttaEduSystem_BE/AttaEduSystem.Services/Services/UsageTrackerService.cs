using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class UsageTrackerService : IUsageTrackerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UsageTrackerService> _logger;

        public UsageTrackerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UsageTrackerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // =========================================================================
        // CONSUME QUOTA
        // =========================================================================
        public async Task<bool> TryConsumeAsync(ClaimsPrincipal user, UsageType type, int amount = 1)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return false;

            // 1. Xử lý / Lấy subscription hiện tại
            //var subscription = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(userId);
            var (subscription, usage) = await CheckAndRenewSubscription(userId);
            var plan = subscription.Plan;

            // Kiểm tra dung lượng
            switch (type)
            {
                case UsageType.Scan:
                    if (usage.ScansUsed + amount > plan.MaxScansPerMonth) return false;
                    usage.ScansUsed += amount;
                    break;

                case UsageType.GenerateExam:
                    if (usage.GeneratedExamsUsed + amount > plan.MaxGeneratedExamsPerMonth) return false;
                    usage.GeneratedExamsUsed += amount;
                    break;

                case UsageType.Solve:
                    if (usage.SolvesUsed + amount > plan.MaxSolvesPerMonth) return false;
                    usage.SolvesUsed += amount;
                    break;

                case UsageType.Token:
                    if (usage.TokensUsed + amount > plan.MaxTokensPerMonth) return false;
                    usage.TokensUsed += amount;
                    break;

                default:
                    return false;
            }

            // Chỉ cần Update Usage (Không cần Add mới vì Helper đã bao tiêu việc tạo mới)
            usage.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
            _unitOfWork.UserUsage.Update(usage);
            await _unitOfWork.SaveAsync();

            return true;

            //if (subscription == null)
            //{
            //    // Nếu không có subscription, dùng plan FREE mặc định
            //    var freePlan = await _unitOfWork.SubscriptionPlan.GetByCodeAsync("FREE");
            //    if (freePlan == null)
            //        return false;

            //    // Tạo subscription FREE tự động
            //    subscription = new UserSubscription
            //    {
            //        UserSubscriptionId = Guid.NewGuid(),
            //        UserId = userId,
            //        SubscriptionPlanId = freePlan.SubscriptionPlanId,
            //        StartDate = DateTime.UtcNow,
            //        EndDate = DateTime.UtcNow.AddYears(100), // Free plan không hết hạn
            //        Status = "Active",
            //        CreatedBy = userId,
            //        CreatedTime = DateTime.UtcNow
            //    };
            //    await _unitOfWork.UserSubscription.AddAsync(subscription);
            //    await _unitOfWork.SaveAsync();
            //}

            //// 2. Lấy hoặc tạo usage record
            //var usage = await _unitOfWork.UserUsage.GetCurrentPeriodAsync(userId, DateTime.UtcNow);
            //bool isNewUsage = false; // <--- Biến cờ đánh dấu

            //if (usage == null)
            //{
            //    isNewUsage = true; // Đánh dấu là mới
            //    usage = new UserUsage
            //    {
            //        UserUsageId = Guid.NewGuid(),
            //        UserId = userId,
            //        PeriodStart = DateTime.UtcNow,
            //        PeriodEnd = DateTime.UtcNow.AddMonths(1),
            //        CreatedBy = userId,
            //        CreatedTime = DateTime.UtcNow,
            //        // Khởi tạo các giá trị bằng 0 để tránh null
            //        TokensUsed = 0,
            //        ScansUsed = 0,
            //        GeneratedExamsUsed = 0,
            //        SolvesUsed = 0
            //    };
            //    //await _unitOfWork.UserUsage.AddAsync(usage); // ko addasync ở đây
            //}

            //// 3. Kiểm tra và trừ quota
            //var plan = subscription.Plan;
            //switch (type)
            //{
            //    case UsageType.Scan:
            //        if (usage.ScansUsed + amount > plan.MaxScansPerMonth)
            //            return false;
            //        usage.ScansUsed += amount;
            //        break;

            //    case UsageType.GenerateExam:
            //        if (usage.GeneratedExamsUsed + amount > plan.MaxGeneratedExamsPerMonth)
            //            return false;
            //        usage.GeneratedExamsUsed += amount;
            //        break;

            //    case UsageType.Solve:
            //        if (usage.SolvesUsed + amount > plan.MaxSolvesPerMonth) return false;
            //        usage.SolvesUsed += amount;
            //        break;

            //    case UsageType.Token:
            //        if (usage.TokensUsed + amount > plan.MaxTokensPerMonth)
            //            return false;
            //        usage.TokensUsed += amount;
            //        break;

            //    default:
            //        return false;
            //}
            //// 4. Lưu thay đổi (QUAN TRỌNG: Tách luồng Add/Update)
            //usage.UpdatedTime = DateTime.UtcNow;

            //if (isNewUsage)
            //{
            //    // Nếu là mới -> Insert
            //    await _unitOfWork.UserUsage.AddAsync(usage);
            //}
            //else
            //{
            //    // Nếu là cũ -> Update
            //    _unitOfWork.UserUsage.Update(usage);
            //}
            //await _unitOfWork.SaveAsync(); // Lúc này sẽ chạy đúng lệnh Insert hoặc Update

            //return true;
        }

        // =========================================================================
        // GET USAGE INFO
        // =========================================================================
        public async Task<ResponseDto> GetUsageInfo(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
            }

            // Kể cả lúc User vào xem thông tin, nếu thấy hết hạn cũng tự động gia hạn trước khi show
            var (subscription, usage) = await CheckAndRenewSubscription(userId);
            var plan = subscription.Plan;

            var dto = new GetUsageInfoDto
            {
                TokensUsed = usage.TokensUsed,
                ScansUsed = usage.ScansUsed,
                GeneratedExamsUsed = usage.GeneratedExamsUsed,
                SolvesUsed = usage.SolvesUsed,
                MaxTokens = plan.MaxTokensPerMonth,
                MaxScans = plan.MaxScansPerMonth,
                MaxGeneratedExams = plan.MaxGeneratedExamsPerMonth,
                MaxSolves = plan.MaxSolvesPerMonth,
                PeriodStart = usage.PeriodStart,
                PeriodEnd = usage.PeriodEnd
            };

            return SuccessResponse.Build(
                message: "Usage info retrieved successfully",
                statusCode: 200,
                result: dto);

            //var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
            //}

            //var subscription = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(userId);
            //if (subscription == null)
            //{
            //    return ErrorResponse.Build("No active subscription found", 404);
            //}

            //var usage = await _unitOfWork.UserUsage.GetCurrentPeriodAsync(userId, DateTime.UtcNow);
            //if (usage == null)
            //{
            //    // Tạo usage record mới nếu chưa có
            //    usage = new UserUsage
            //    {
            //        UserUsageId = Guid.NewGuid(),
            //        UserId = userId,
            //        PeriodStart = DateTime.UtcNow,
            //        PeriodEnd = DateTime.UtcNow.AddMonths(1),
            //        CreatedBy = userId,
            //        CreatedTime = DateTime.UtcNow
            //    };
            //    await _unitOfWork.UserUsage.AddAsync(usage);
            //    await _unitOfWork.SaveAsync();
            //}

            //var plan = subscription.Plan;
            //var dto = new GetUsageInfoDto
            //{
            //    TokensUsed = usage.TokensUsed,
            //    ScansUsed = usage.ScansUsed,
            //    GeneratedExamsUsed = usage.GeneratedExamsUsed,
            //    SolvesUsed = usage.SolvesUsed,
            //    MaxTokens = plan.MaxTokensPerMonth,
            //    MaxScans = plan.MaxScansPerMonth,
            //    MaxGeneratedExams = plan.MaxGeneratedExamsPerMonth,
            //    MaxSolves = plan.MaxSolvesPerMonth,
            //    PeriodStart = usage.PeriodStart,
            //    PeriodEnd = usage.PeriodEnd
            //};

            //return SuccessResponse.Build(
            //    message: "Usage info retrieved successfully",
            //    statusCode: 200,
            //    result: dto);
        }

        // =========================================================================
        // HÀM HELPER: LAZY RENEW & DOWNGRADE (Gia hạn FREE hoặc Hạ cấp từ PRO về FREE)
        // =========================================================================
        private async Task<(UserSubscription Subscription, UserUsage Usage)> CheckAndRenewSubscription(string userId)
        {
            var now = StaticOperationStatus.Timezone.Vietnam;

            // 1. Lấy gói hiện tại của User (Bất kể hết hạn hay chưa)
            var subscription = await _unitOfWork.UserSubscription.GetAsync(
                filter: s => s.UserId == userId,
                includeProperties: "Plan");

            // Lấy thông tin gói FREE làm mặc định
            var freePlan = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.Code == "FREE");
            int freeDuration = freePlan?.DurationInDays > 0 ? freePlan.DurationInDays : 30;

            bool isPlanChangedOrRenewed = false;

            if (subscription == null)
            {
                // TH1: User mới tinh -> Tạo gói FREE
                subscription = new UserSubscription
                {
                    UserSubscriptionId = Guid.NewGuid(),
                    UserId = userId,
                    SubscriptionPlanId = freePlan!.SubscriptionPlanId,
                    Plan = freePlan,
                    StartDate = now,
                    EndDate = now.AddDays(freeDuration),
                    Status = "Active",
                    CreatedBy = userId,
                    CreatedTime = now
                };
                await _unitOfWork.UserSubscription.AddAsync(subscription);
                isPlanChangedOrRenewed = true;
            }
            else if (subscription.EndDate < now)
            {
                // TH2: Gói đã hết hạn (Bất kể trước đó là FREE hay PRO) -> Đưa về FREE chu kỳ mới
                subscription.SubscriptionPlanId = freePlan!.SubscriptionPlanId;
                subscription.Plan = freePlan;
                subscription.StartDate = now;
                subscription.EndDate = now.AddDays(freeDuration);
                subscription.UpdatedTime = now;

                _unitOfWork.UserSubscription.Update(subscription);
                isPlanChangedOrRenewed = true;
            }

            // 2. Lấy & Reset Usage
            var usage = await _unitOfWork.UserUsage.GetAsync(u => u.UserId == userId);

            if (usage == null)
            {
                // Tạo bảng đếm lần đầu
                usage = new UserUsage
                {
                    UserUsageId = Guid.NewGuid(),
                    UserId = userId,
                    PeriodStart = subscription.StartDate,
                    PeriodEnd = subscription.EndDate,
                    TokensUsed = 0,
                    ScansUsed = 0,
                    GeneratedExamsUsed = 0,
                    SolvesUsed = 0,
                    CreatedBy = userId,
                    CreatedTime = now
                };
                await _unitOfWork.UserUsage.AddAsync(usage);
            }
            else if (usage.PeriodEnd < now || isPlanChangedOrRenewed)
            {
                // Reset Usage nếu chu kỳ đã hết, HOẶC gói cước vừa bị gia hạn/hạ cấp
                usage.PeriodStart = subscription.StartDate;
                usage.PeriodEnd = subscription.EndDate;
                usage.TokensUsed = 0;
                usage.ScansUsed = 0;
                usage.GeneratedExamsUsed = 0;
                usage.SolvesUsed = 0;
                usage.UpdatedTime = now;

                _unitOfWork.UserUsage.Update(usage);
            }

            // Lưu 1 lần duy nhất cho mọi thay đổi
            await _unitOfWork.SaveAsync();

            return (subscription, usage);
        }
    }
}

