using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Models.DTOs.Payment;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayOS _payOs;
        private readonly IMapper _mapper;
        private readonly ILogger<PayOsService> _logger;

        public PayOsService(
            IUnitOfWork unitOfWork,
            PayOS payOs,
            IMapper mapper,
            ILogger<PayOsService> logger)
        {
            _unitOfWork = unitOfWork;
            _payOs = payOs;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto> CreatePayOsPaymentLink(ClaimsPrincipal user, CreatePaymentLinkDto createPaymentLinkDto)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDto
                    {
                        Message = StaticOperationStatus.User.UserNotFound,
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                // 1. Lấy Order theo OrderNumber + UserId (đảm bảo user chỉ thanh toán đơn của mình)
                var order = await _unitOfWork.Order.GetAsync(
                    o => o.UserId == userId && o.OrderNumber == createPaymentLinkDto.OrderNumber,
                    includeProperties: nameof(Order.Plan));

                if (order == null)
                {
                    return new ResponseDto
                    {
                        Message = "Order not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                // 2. Chuẩn bị item cho PayOS (ở đây 1 item = 1 gói)
                var items = new List<ItemData>
                {
                    new ItemData(
                        order.Plan.Name,
                        1,
                        Convert.ToInt32(order.TotalPrice)
                    )
                };

                var totalPrice = items.Sum(i => i.price * i.quantity);

                // 3. Tạo PaymentData cho PayOS

                var description = $"Thanh toan {order.Plan.Code}";

                // Cắt chuỗi nếu vẫn lỡ tay quá dài (Safety check)
                if (description.Length > 25)
                {
                    description = description.Substring(0, 25);
                }

                var paymentData = new PaymentData(
                    createPaymentLinkDto.OrderNumber, // orderCode
                    totalPrice,
                    description,
                    items,
                    createPaymentLinkDto.CancelUrl,
                    createPaymentLinkDto.ReturnUrl
                );

                // 4. Gọi PayOS tạo link
                var result = await _payOs.createPaymentLink(paymentData);

                // 5. Lưu Payment nội bộ (Pending)
                var payment = new Payment
                {
                    PaymentTransactionId = Guid.NewGuid(),
                    OrderNumber = createPaymentLinkDto.OrderNumber,
                    Amount = result.amount,
                    Description = result.description.Trim(),
                    CancelUrl = paymentData.cancelUrl,
                    ReturnUrl = paymentData.returnUrl,
                    CreatedAt = StaticOperationStatus.Timezone.Vietnam,
                    Status = PaymentStatus.Pending,
                    CreatedBy = userId,
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };

                await _unitOfWork.Payment.AddAsync(payment);
                await _unitOfWork.SaveAsync();

                return new ResponseDto
                {
                    Message = "Create PayOS payment link successfully",
                    IsSuccess = true,
                    StatusCode = 201,
                    Result = new
                    {
                        result // chứa checkoutUrl, etc. FE sẽ dùng để redirect
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating PayOS payment link");
                return new ResponseDto
                {
                    Message = ex.Message,
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDto> ConfirmPayOsTransaction(ConfirmPaymentDto confirmPaymentDto)
        {
            try
            {
                // 1. Lấy Order
                var order = await _unitOfWork.Order.GetByOrderNumberAsync(confirmPaymentDto.OrderNumber);
                if (order == null)
                    return new ResponseDto { Message = "Order not found", IsSuccess = false, StatusCode = 404 };

                // 2. Lấy Payment nội bộ theo OrderNumber
                var payment = await _unitOfWork.Payment.GetPaymentByOrderNumberAsync(confirmPaymentDto.OrderNumber);
                if (payment == null)
                    return new ResponseDto { Message = "Payment record not found", IsSuccess = false, StatusCode = 404 };

                // 🔴 FIX: CHẶN XỬ LÝ LẠI NẾU ĐƠN HÀNG ĐÃ ĐƯỢC THANH TOÁN TRƯỚC ĐÓ
                // (Phòng trường hợp User F5 lại trang callback nhiều lần để lách luật gia hạn gói)
                if (payment.Status == PaymentStatus.Paid || order.Status == "Paid")
                {
                    return new ResponseDto
                    {
                        Message = "Payment already processed",
                        IsSuccess = true,
                        StatusCode = 200,
                        Result = new Dictionary<string, object>
                        {
                            { "orderNumber", payment.OrderNumber },
                            { "orderId", order.OrderId },
                            { "paymentStatus", payment.Status.ToString() },
                            { "payOsStatus", "ALREADY_PAID" } // Trả về chữ này để Controller KHÔNG gọi ActivateFromOrder nữa
                        }
                    };
                }

                // 3. Lấy thông tin giao dịch từ PayOS
                var transactionInfo = await _payOs.getPaymentLinkInformation(confirmPaymentDto.OrderNumber);
                if (transactionInfo == null)
                    return new ResponseDto { Message = "Transaction not found", IsSuccess = false, StatusCode = 400 };

                // 4. Cập nhật trạng thái dựa vào PayOS
                if (transactionInfo.status == "PAID")
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                    order.Status = "Paid";
                    order.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                }
                else if (transactionInfo.status == "CANCELLED")
                {
                    payment.Status = PaymentStatus.Cancelled;
                    payment.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                }
                else
                {
                    return new ResponseDto { Message = $"Payment status from gateway: {transactionInfo.status}", IsSuccess = false, StatusCode = 400 };
                }

                _unitOfWork.Payment.Update(payment);
                await _unitOfWork.SaveAsync();

                return new ResponseDto
                {
                    Message = "Payment status updated successfully",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new Dictionary<string, object>
                    {
                        { "orderNumber", payment.OrderNumber },
                        { "orderId", order.OrderId },
                        { "paymentStatus", payment.Status.ToString() },
                        { "payOsStatus", transactionInfo.status } // Nếu trả về PAID thì Controller sẽ kích hoạt gói
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while confirming PayOS transaction");
                return new ResponseDto { Message = ex.Message, IsSuccess = false, StatusCode = 500 };
            }

            /*try
            {
                // 1. Lấy Order
                var order = await _unitOfWork.Order.GetByOrderNumberAsync(confirmPaymentDto.OrderNumber);
                if (order == null)
                {
                    return new ResponseDto
                    {
                        Message = "Order not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                // 2. Lấy thông tin giao dịch từ PayOS
                var transactionInfo = await _payOs.getPaymentLinkInformation(confirmPaymentDto.OrderNumber);
                if (transactionInfo == null)
                {
                    return new ResponseDto
                    {
                        Message = "Transaction not found",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                // 3. Lấy Payment nội bộ theo OrderNumber
                var payment = await _unitOfWork.Payment.GetPaymentByOrderNumberAsync(confirmPaymentDto.OrderNumber);
                if (payment == null)
                {
                    return new ResponseDto
                    {
                        Message = "Payment record not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                // 4. Cập nhật trạng thái dựa vào PayOS
                if (transactionInfo.status == "PAID")
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                    order.Status = "Paid";
                    order.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                    // TODO: tại đây bạn gọi SubscriptionService để activate/gia hạn gói
                    //await _subscriptionService.ActivateFromOrder(order.OrderId);

                }
                else if (transactionInfo.status == "CANCELLED")
                {
                    payment.Status = PaymentStatus.Cancelled;
                    payment.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                }
                else
                {
                    return new ResponseDto
                    {
                        Message = $"Payment status from gateway: {transactionInfo.status}",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                _unitOfWork.Payment.Update(payment);
                await _unitOfWork.SaveAsync();

                return new ResponseDto
                {
                    Message = "Payment status updated successfully",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new Dictionary<string, object>
                    {
                        { "orderNumber", payment.OrderNumber },
                        { "orderId", order.OrderId },
                        { "paymentStatus", payment.Status.ToString() },
                        { "payOsStatus", transactionInfo.status } // Key quan trọng
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while confirming PayOS transaction");
                return new ResponseDto
                {
                    Message = ex.Message,
                    IsSuccess = false,
                    StatusCode = 500
                };
            }*/
        }

        public async Task<ResponseDto> GetAllPayments(
    ClaimsPrincipal user,
    int pageNumber = 1,
    int pageSize = 10,
    string? filterOn = null,
    string? filterQuery = null,
    string? sortBy = null)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return new ResponseDto
                {
                    Message = StaticOperationStatus.User.UserNotFound,
                    IsSuccess = false,
                    StatusCode = 404
                };
            }

            // Nếu bạn có role Admin/Manager thì có thể cho xem tất cả
            var isAdmin = user.IsInRole(StaticUserRoles.Admin);
            string? filterUserId = isAdmin ? null : userId;

            var (payments, totalPayments) = await _unitOfWork.Payment.GetPaymentsAsync(
                pageNumber,
                pageSize,
                filterOn,
                filterQuery,
                sortBy,
                filterUserId);

            if (!payments.Any())
            {
                return new ResponseDto
                {
                    Message = "No payments found",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new
                    {
                        TotalPayments = 0,
                        TotalPages = 0,
                        PageSize = pageSize,
                        CurrentPage = pageNumber,
                        Payments = Array.Empty<GetAllPaymentDto>()
                    }
                };
            }

            var totalPages = (int)Math.Ceiling((double)totalPayments / pageSize);

            var paymentDtos = _mapper.Map<IEnumerable<GetAllPaymentDto>>(payments); 

            return SuccessResponse.Build(
                message: "Get all payments successfully",
                statusCode: 200,
                result: new
                {
                    TotalPayments = totalPayments,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    Payments = paymentDtos
                });
        }

        public async Task<ResponseDto> GetPaymentById(ClaimsPrincipal user, Guid paymentTransactionId)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return new ResponseDto
                {
                    Message = StaticOperationStatus.User.UserNotFound,
                    IsSuccess = false,
                    StatusCode = 404
                };
            }

            var payment = await _unitOfWork.Payment.GetAsync(p => p.PaymentTransactionId == paymentTransactionId);
            if (payment == null)
            {
                return new ResponseDto
                {
                    Message = "Payment not found",
                    IsSuccess = false,
                    StatusCode = 404
                };
            }

            // Nếu không phải admin, chắc chắn payment phải thuộc về user
            var isAdmin = user.IsInRole(StaticUserRoles.Admin);
            if (!isAdmin)
            {
                var order = await _unitOfWork.Order.GetByOrderNumberAsync(payment.OrderNumber);
                if (order == null || order.UserId != userId)
                {
                    return new ResponseDto
                    {
                        Message = "You are not allowed to view this payment",
                        IsSuccess = false,
                        StatusCode = 403
                    };
                }
            }

            var paymentDto = _mapper.Map<Payment, GetAllPaymentDto>(payment);

            return SuccessResponse.Build(
                message: "Get payment by Id successfully",
                statusCode: 200,
                result: paymentDto);
        }
    }
}