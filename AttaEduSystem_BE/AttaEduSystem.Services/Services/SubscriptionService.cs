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
        private readonly IPayOsService _payOsService;

        public SubscriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SubscriptionService> logger,
            IPayOsService payOsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _payOsService = payOsService;
        }

        public async Task<ResponseDto> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto, ClaimsPrincipal adminUser)
        {
            // Basic validation: unique code
            var exists = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.Code == dto.Code);
            if (exists != null)
                return ErrorResponse.Build("Subscription plan code already exists", 400);

            var plan = new SubscriptionPlan
            {
                SubscriptionPlanId = Guid.NewGuid(),
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationInDays = dto.DurationInDays > 0 ? dto.DurationInDays : 30,
                MaxScansPerMonth = dto.MaxScansPerMonth,
                MaxGeneratedExamsPerMonth = dto.MaxGeneratedExamsPerMonth,
                MaxSolvesPerMonth = dto.MaxSolvesPerMonth,
                Status = "Active",
                CreatedBy = adminUser.FindFirstValue("FullName") ?? "Admin",
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };

            await _unitOfWork.SubscriptionPlan.AddAsync(plan);
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<AdminSubscriptionPlanDto>(plan);
            return SuccessResponse.Build("Subscription plan created successfully", 201, resultDto);
        }

        public async Task<ResponseDto> UpdateSubscriptionPlan(Guid planId, UpdateSubscriptionPlanDto dto, ClaimsPrincipal adminUser)
        {
            var plan = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.SubscriptionPlanId == planId);
            if (plan == null) return ErrorResponse.Build("Subscription plan not found", 404);

            if (!string.IsNullOrWhiteSpace(dto.Name)) plan.Name = dto.Name;
            if (dto.Description != null) plan.Description = dto.Description;
            if (dto.Price.HasValue) plan.Price = dto.Price.Value; // Đã đổi
            if (dto.DurationInDays.HasValue) plan.DurationInDays = dto.DurationInDays.Value;
            if (dto.MaxScansPerMonth.HasValue) plan.MaxScansPerMonth = dto.MaxScansPerMonth.Value;
            if (dto.MaxGeneratedExamsPerMonth.HasValue) plan.MaxGeneratedExamsPerMonth = dto.MaxGeneratedExamsPerMonth.Value;
            if (dto.MaxSolvesPerMonth.HasValue) plan.MaxSolvesPerMonth = dto.MaxSolvesPerMonth.Value;
            if (!string.IsNullOrWhiteSpace(dto.Status)) plan.Status = dto.Status;

            plan.UpdatedBy = adminUser.FindFirstValue("FullName") ?? "Admin";
            plan.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.SubscriptionPlan.Update(plan);
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<AdminSubscriptionPlanDto>(plan);
            return SuccessResponse.Build("Subscription plan updated successfully", 200, resultDto);
        }

        public async Task<ResponseDto> UpdateStatusSubscriptionPlan(Guid planId, UpdateStatusSubscriptionPlanDto dto, ClaimsPrincipal adminUser)
        {
            var plan = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.SubscriptionPlanId == planId);
            if (plan == null)
                return ErrorResponse.Build("Subscription plan not found", 404);

            // Avoid no-op
            if (string.Equals(plan.Status ?? string.Empty, dto.Status, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorResponse.Build("Subscription plan already in the requested status", 400);
            }

            plan.Status = dto.Status;
            plan.UpdatedBy = adminUser.FindFirstValue("FullName") ?? "Admin";
            plan.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.SubscriptionPlan.Update(plan);
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<AdminSubscriptionPlanDto>(plan);
            return SuccessResponse.Build("Subscription plan status updated successfully", 200, resultDto);
        }

        public async Task<ResponseDto> DeleteSubscriptionPlan(Guid planId, ClaimsPrincipal adminUser)
        {
            var plan = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.SubscriptionPlanId == planId);
            if (plan == null)
                return ErrorResponse.Build("Subscription plan not found", 404);

            // Không cho xóa nếu plan đang được xóa rồi
            if (string.Equals(plan.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
                return ErrorResponse.Build("Subscription plan has already been deleted", 400);

            // Kiểm tra plan có đang được user subscribe không (bảo vệ dữ liệu)
            var hasActiveSubscribers = await _unitOfWork.UserSubscription
                .GetAsync(s => s.SubscriptionPlanId == planId && s.Status == "Active");
            if (hasActiveSubscribers != null)
                return ErrorResponse.Build(
                    "Cannot delete plan while users are actively subscribed to it. Deactivate it instead.", 409);

            // Soft delete: cập nhật Status → "Deleted"
            plan.Status = "Deleted";
            plan.UpdatedBy = adminUser.FindFirstValue("FullName") ?? "Admin";
            plan.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.SubscriptionPlan.Update(plan);
            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build("Subscription plan deleted successfully", 200);
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

            // Lấy số ngày của gói cước, mặc định 30 nếu có lỗi data
            int durationDays = order.Plan.DurationInDays > 0 ? order.Plan.DurationInDays : 30;
            var now = StaticOperationStatus.Timezone.Vietnam;

            // Kiểm tra xem user đã có subscription chưa
            //var existingSub = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(order.UserId);
            var existingSub = await _unitOfWork.UserSubscription.GetAsync(s => s.UserId == order.UserId);
            if (existingSub != null)
            {
                // 1. Cập nhật sang Plan mới (QUAN TRỌNG NHẤT)
                existingSub.SubscriptionPlanId = order.SubscriptionPlanId;

                // 2. Reset ngày bắt đầu và kết thúc theo gói mới
                existingSub.StartDate = now;
                existingSub.EndDate = now.AddDays(durationDays);
        
                // 3. Cập nhật trạng thái
                existingSub.Status = "Active";
                existingSub.UpdatedTime = now;
        
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
                    StartDate = now,
                    EndDate = now.AddDays(durationDays),
                    Status = "Active",
                    IsAutoRenew = false,
                    CreatedBy = order.UserId,
                    CreatedTime = now
                };
                await _unitOfWork.UserSubscription.AddAsync(newSub);
            }

            // Reset usage cho chu kỳ mới
            //var currentUsage = await _unitOfWork.UserUsage.GetCurrentPeriodAsync(order.UserId, DateTime.UtcNow);
            var currentUsage = await _unitOfWork.UserUsage.GetAsync(u => u.UserId == order.UserId);
            if (currentUsage == null)
            {
                var newUsage = new UserUsage
                {
                    UserUsageId = Guid.NewGuid(),
                    UserId = order.UserId,
                    PeriodStart = now,
                    PeriodEnd = now.AddDays(durationDays),
                    TokensUsed = 0,
                    ScansUsed = 0,
                    GeneratedExamsUsed = 0,
                    SolvesUsed = 0,
                    CreatedBy = order.UserId,
                    CreatedTime = now
                };
                await _unitOfWork.UserUsage.AddAsync(newUsage);
            }
            else
            {
                currentUsage.TokensUsed = 0;
                currentUsage.ScansUsed = 0;
                currentUsage.GeneratedExamsUsed = 0;
                currentUsage.SolvesUsed = 0;
                currentUsage.PeriodStart = now;
                currentUsage.PeriodEnd = now.AddDays(durationDays);
                currentUsage.UpdatedTime = now;
                _unitOfWork.UserUsage.Update(currentUsage);
            }

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build(
                message: "Subscription activated successfully",
                statusCode: 200);
        }

        public async Task<ResponseDto> CreateCheckout(ClaimsPrincipal user, CreateCheckoutRequestDto request)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var fullName = user.FindFirstValue("FullName") ?? "Unknown";

            if (string.IsNullOrEmpty(userId))
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

            // 1. Lấy plan
            var plan = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.SubscriptionPlanId == request.SubscriptionPlanId);
            if (plan == null || !string.Equals(plan.Status, "Active", StringComparison.OrdinalIgnoreCase))
                return ErrorResponse.Build("Subscription plan not found or inactive", 404);


            // 2. Tạo Order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanId = plan.SubscriptionPlanId,
                TotalPrice = plan.Price,
                CreatedBy = fullName,
                CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                Status = "Pending"
            };

            await _unitOfWork.Order.AddAsync(order);
            await _unitOfWork.SaveAsync(); // để DB sinh OrderNumber

            // 3. Gọi PayOS tạo link thanh toán
            var createPaymentLinkDto = new CreatePaymentLinkDto
            {
                OrderNumber = order.OrderNumber,
                CancelUrl = request.CancelUrl,
                ReturnUrl = request.ReturnUrl
            };

            var paymentResult = await _payOsService.CreatePayOsPaymentLink(user, createPaymentLinkDto);

            // 4. Gộp thông tin Order + checkoutUrl trả về FE
            if (!paymentResult.IsSuccess)
                return paymentResult;

            return SuccessResponse.Build(
                message: "Checkout session created successfully",
                statusCode: 201,
                result: new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    planCode = plan.Code,
                    planName = plan.Name,
                    amount = plan.Price,
                    payment = paymentResult.Result // chứa result từ PayOS (checkoutUrl,...)
                });
        }

        public async Task<bool> CanUseAdvancedFeature(string userId, string featureName)
        {
            var subscription = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(userId);
            if (subscription == null) return false;

            // Ví dụ: chỉ Pro plan mới có thể giải đề chi tiết
            return subscription.Plan.Code == "PRO";
        }
    }
}
