using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SubscriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto> GetAvailablePlans()
        {
            var plans = await _unitOfWork.SubscriptionPlan.GetActivePlansAsync();
            var dtos = _mapper.Map<IEnumerable<GetSubscriptionPlanDto>>(plans);

            return SuccessResponse.Build(
                message: "Available plans retrieved successfully",
                statusCode: 200,
                result: dtos);
        }

        public async Task<ResponseDto> GetCurrentSubscription(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(
                    StaticOperationStatus.User.UserNotFound,
                    401);
            }

            var subscription = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(userId);
            if (subscription == null)
            {
                return ErrorResponse.Build(
                    message: "No active subscription found",
                    statusCode: 200);
            }

            var dto = _mapper.Map<GetUserSubscriptionDto>(subscription);
            return SuccessResponse.Build(
                message: "Current subscription retrieved successfully",
                statusCode: 200,
                result: dto);
        }

        public async Task<ResponseDto> ActivateFromOrder(Guid orderId)
        {
            var order = await _unitOfWork.Order.GetAsync(o => o.OrderId == orderId,
                includeProperties: nameof(Order.Plan));

            if (order == null)
            {
                return ErrorResponse.Build("Order not found", 404);
            }

            // Kiểm tra xem user đã có subscription chưa
            var existingSub = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(order.UserId);
            if (existingSub != null)
            {
                // Gia hạn subscription hiện tại
                existingSub.EndDate = existingSub.EndDate.AddMonths(1);
                existingSub.UpdatedTime = DateTime.UtcNow;
                _unitOfWork.UserSubscription.Update(existingSub);
            }
            else
            {
                // Tạo subscription mới
                var newSub = new UserSubscription
                {
                    UserSubscriptionId = Guid.NewGuid(),
                    UserId = order.UserId,
                    SubscriptionPlanId = order.SubscriptionPlanId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    Status = "Active",
                    IsAutoRenew = false,
                    CreatedBy = order.UserId,
                    CreatedTime = DateTime.UtcNow
                };
                await _unitOfWork.UserSubscription.AddAsync(newSub);
            }

            // Reset usage cho chu kỳ mới
            var currentUsage = await _unitOfWork.UserUsage.GetCurrentPeriodAsync(order.UserId, DateTime.UtcNow);
            if (currentUsage == null)
            {
                var newUsage = new UserUsage
                {
                    UserUsageId = Guid.NewGuid(),
                    UserId = order.UserId,
                    PeriodStart = DateTime.UtcNow,
                    PeriodEnd = DateTime.UtcNow.AddMonths(1),
                    TokensUsed = 0,
                    ScansUsed = 0,
                    GeneratedExamsUsed = 0,
                    CreatedBy = order.UserId,
                    CreatedTime = DateTime.UtcNow
                };
                await _unitOfWork.UserUsage.AddAsync(newUsage);
            }
            else
            {
                currentUsage.TokensUsed = 0;
                currentUsage.ScansUsed = 0;
                currentUsage.GeneratedExamsUsed = 0;
                currentUsage.PeriodStart = DateTime.UtcNow;
                currentUsage.PeriodEnd = DateTime.UtcNow.AddMonths(1);
                currentUsage.UpdatedTime = DateTime.UtcNow;
                _unitOfWork.UserUsage.Update(currentUsage);
            }

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build(
                message: "Subscription activated successfully",
                statusCode: 200);
        }
    }
}
