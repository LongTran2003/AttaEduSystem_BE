using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Admin;
using AttaEduSystem.Models.DTOs.Admin.Dashboards;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            UserManager<ApplicationUser> userManager, 
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto> GetAllUsers(UserFilterDto filterDto)
        {
            try
            {
                // 1. Call Repository
                var (users, totalCount) = await _unitOfWork.User.GetAllUsersAsync(
                    pageNumber: filterDto.PageNumber,
                    pageSize: filterDto.PageSize,
                    filterOn: "email",
                    filterQuery: filterDto.SearchTerm,
                    sortBy: filterDto.SortBy,
                    status: filterDto.Status
                );

                if (!users.Any())
                {
                    return SuccessResponse.Build(
                        message: "No users found",
                        statusCode: StaticOperationStatus.StatusCode.Ok,
                        result: new { TotalCount = 0 }); // Return gọn
                }

                // 2. Mapping & Enrich Data
                var userDtos = _mapper.Map<List<GetUserDto>>(users);

                // Enrich Roles/Codes manually
                for (int i = 0; i < users.Count; i++)
                {
                    await EnrichUserCodeAsync(users[i], userDtos[i]);
                }

                // 3. Build Payload
                var payload = new
                {
                    Data = userDtos,
                    CurrentPage = filterDto.PageNumber,
                    PageSize = filterDto.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filterDto.PageSize),
                };

                return SuccessResponse.Build("Users retrieved successfully", 200, payload);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build(
                    message: $"Failed to retrieve users: {ex.Message}",
                    statusCode: StaticOperationStatus.StatusCode.InternalServerError);
            }
        }

        public async Task<ResponseDto> GetUserById(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);

                var userDto = _mapper.Map<GetUserDto>(user);

                await EnrichUserCodeAsync(user, userDto);

                return SuccessResponse.Build("User details retrieved successfully", 200, userDto);
            }
            catch (Exception ex)
            {

                return ErrorResponse.Build($"Error retrieving user: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> UpdateUser(string userId, UpdateUserDto updateDto, ClaimsPrincipal admin)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);

                // AutoMapper handles manual property assignments (Clean code)
                _mapper.Map(updateDto, user);

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ErrorResponse.Build($"Update failed: {errorMsg}", 400);
                }

                return SuccessResponse.Build("User updated successfully", 200, new { UserId = user.Id });
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Error updating user: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> DeleteUser(string userId, ClaimsPrincipal admin)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);

                // 1. Kiểm tra nếu đã xóa rồi thì thôi
                if (user.Status == "Deleted")
                {
                    return ErrorResponse.Build("User is already deleted.", 400);
                }

                // 2. Prevent admin self-deletion
                var currentAdminId = admin.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentAdminId == user.Id)
                    return ErrorResponse.Build("You cannot delete your own account.", 400);

                // 3. Soft Delete
                user.Status = "Deleted";

                // 4. Anonymize critical data
                user.FullName = $"{user.FullName}_deleted_{Guid.NewGuid().ToString()[..8]}";
                user.Email = $"{user.Email}_deleted_{Guid.NewGuid().ToString()[..8]}";

                // 5. Invalidate tokens
                await _userManager.UpdateSecurityStampAsync(user);

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return ErrorResponse.Build("Failed to delete user", 500);

                return SuccessResponse.Build("User has been soft deleted", 200);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Error deleting user: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> LockUser(string userId, int lockDurationDays, ClaimsPrincipal admin)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);

                var currentAdminId = admin.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentAdminId == user.Id) return ErrorResponse.Build("You cannot lock your own account.", 400);

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(lockDurationDays));

                user.Status = "Locked";
                await _userManager.UpdateAsync(user);

                return SuccessResponse.Build($"User locked for {lockDurationDays} days", 200);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Error locking user: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> UnlockUser(string userId, ClaimsPrincipal admin)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);

                await _userManager.SetLockoutEndDateAsync(user, null);

                user.Status = "Active";
                await _userManager.UpdateAsync(user);

                return SuccessResponse.Build("User unlocked successfully", 200);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Error unlocking user: {ex.Message}", 500);
            }
        }

        // =========================================================
        // DASHBOARD - Overview Statistics
        // =========================================================
        public async Task<ResponseDto> GetDashboardOverview()
        {
            try
            {
                // 1. User Statistics (Dùng UserManager để đếm theo role)
                var allUsers = await _userManager.Users.ToListAsync();
                var students = await _userManager.GetUsersInRoleAsync(StaticUserRoles.Student);
                var teachers = await _userManager.GetUsersInRoleAsync(StaticUserRoles.Teacher);
                var admins = await _userManager.GetUsersInRoleAsync(StaticUserRoles.Admin);

                // 2. Content Statistics
                var examPapers = await _unitOfWork.ExamPaper.GetAllAsync();
                var generatedExams = await _unitOfWork.GeneratedExamPaper.GetAllAsync();
                var examRooms = await _unitOfWork.ExamRoom.GetAllAsync();

                // 3. Subscription Statistics
                var now = DateTime.UtcNow;
                var allSubscriptions = await _unitOfWork.UserSubscription.GetAllAsync();

                // Active = Status "Active" AND EndDate > Now (Option 3B)
                var activeSubscriptions = allSubscriptions
                    .Where(s => s.Status == "Active" && s.EndDate > now)
                    .ToList();

                // Đếm Free vs Pro users (cần Include Plan hoặc query riêng)
                var freeUserCount = 0;
                var proUserCount = 0;

                foreach (var sub in activeSubscriptions)
                {
                    var plan = await _unitOfWork.SubscriptionPlan.GetAsync(p => p.SubscriptionPlanId == sub.SubscriptionPlanId);
                    if (plan?.Code == "FREE") freeUserCount++;
                    else if (plan?.Code == "PRO") proUserCount++;
                }

                // 4. Revenue Statistics (Option 2A - từ Payment với Status = Paid)
                var payments = await _unitOfWork.Payment.GetAllAsync();
                var paidPayments = payments.Where(p => p.Status == PaymentStatus.Paid).ToList();

                var totalRevenue = paidPayments.Sum(p => p.Amount);

                var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var revenueThisMonth = paidPayments
                    .Where(p => p.CreatedAt >= startOfMonth)
                    .Sum(p => p.Amount);

                // 5. Build Response
                var overview = new AdminDashboardOverviewDto
                {
                    // Users
                    TotalUsers = allUsers.Count,
                    TotalStudents = students.Count,
                    TotalTeachers = teachers.Count,
                    TotalAdmins = admins.Count,

                    // Content
                    TotalExamPapers = examPapers.Count(),
                    TotalGeneratedExams = generatedExams.Count(),
                    TotalExamRooms = examRooms.Count(),

                    // Subscriptions
                    TotalActiveSubscriptions = activeSubscriptions.Count,
                    TotalFreeUsers = freeUserCount,
                    TotalProUsers = proUserCount,

                    // Revenue
                    TotalRevenue = totalRevenue,
                    RevenueThisMonth = revenueThisMonth
                };

                return SuccessResponse.Build("Dashboard overview retrieved successfully", 200, overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard overview");
                return ErrorResponse.Build($"Error retrieving dashboard: {ex.Message}", 500);
            }
        }

        // =========================================================
        // DASHBOARD - Charts Data
        // =========================================================
        public async Task<ResponseDto> GetDashboardCharts(int months = 6)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-months + 1);

                // 1. User Growth Chart (Option 4C - dùng UserSubscription.CreatedTime)
                var subscriptions = await _unitOfWork.UserSubscription.GetAllAsync(
                    s => s.CreatedTime.HasValue && s.CreatedTime >= startDate);

                var userGrowthData = subscriptions
                    .Where(s => s.CreatedTime.HasValue)
                    .GroupBy(s => new { s.CreatedTime!.Value.Year, s.CreatedTime!.Value.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                var userGrowthChart = new List<UserGrowthChartDto>();
                int cumulativeUsers = 0;

                // Lấy tổng users trước startDate để tính cumulative
                var priorSubscriptions = await _unitOfWork.UserSubscription.GetAllAsync(
                    s => s.CreatedTime.HasValue && s.CreatedTime < startDate);
                cumulativeUsers = priorSubscriptions.Count();

                for (int i = 0; i < months; i++)
                {
                    var targetDate = startDate.AddMonths(i);
                    var monthData = userGrowthData.FirstOrDefault(x => x.Year == targetDate.Year && x.Month == targetDate.Month);
                    var newUsers = monthData?.Count ?? 0;
                    cumulativeUsers += newUsers;

                    userGrowthChart.Add(new UserGrowthChartDto
                    {
                        Month = targetDate.ToString("yyyy-MM"),
                        MonthLabel = targetDate.ToString("MMM yyyy"),
                        NewUsers = newUsers,
                        CumulativeUsers = cumulativeUsers
                    });
                }

                // 2. Exam Creation Trend (Option 6: cả ExamPaper + GeneratedExamPaper)
                var examPapers = await _unitOfWork.ExamPaper.GetAllAsync(
                    e => e.CreatedTime.HasValue && e.CreatedTime >= startDate);

                var generatedExams = await _unitOfWork.GeneratedExamPaper.GetAllAsync(
                    g => g.CreatedTime.HasValue && g.CreatedTime >= startDate);

                var examCreationChart = new List<ExamCreationChartDto>();

                for (int i = 0; i < months; i++)
                {
                    var targetDate = startDate.AddMonths(i);
                    var scanned = examPapers.Count(e =>
                        e.CreatedTime!.Value.Year == targetDate.Year &&
                        e.CreatedTime!.Value.Month == targetDate.Month);

                    var aiGenerated = generatedExams.Count(g =>
                        g.CreatedTime!.Value.Year == targetDate.Year &&
                        g.CreatedTime!.Value.Month == targetDate.Month);

                    examCreationChart.Add(new ExamCreationChartDto
                    {
                        Month = targetDate.ToString("yyyy-MM"),
                        MonthLabel = targetDate.ToString("MMM yyyy"),
                        ScannedExams = scanned,
                        AiGeneratedExams = aiGenerated,
                        Total = scanned + aiGenerated
                    });
                }

                // 3. Revenue Chart
                var payments = await _unitOfWork.Payment.GetAllAsync(
                    p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startDate);

                var revenueChart = new List<RevenueChartDto>();

                for (int i = 0; i < months; i++)
                {
                    var targetDate = startDate.AddMonths(i);
                    var monthPayments = payments.Where(p =>
                        p.CreatedAt.Year == targetDate.Year &&
                        p.CreatedAt.Month == targetDate.Month).ToList();

                    revenueChart.Add(new RevenueChartDto
                    {
                        Month = targetDate.ToString("yyyy-MM"),
                        MonthLabel = targetDate.ToString("MMM yyyy"),
                        Revenue = monthPayments.Sum(p => p.Amount),
                        TransactionCount = monthPayments.Count
                    });
                }

                // 4. Build Response
                var charts = new AdminDashboardChartsDto
                {
                    UserGrowth = userGrowthChart,
                    ExamCreationTrend = examCreationChart,
                    RevenueChart = revenueChart
                };

                return SuccessResponse.Build("Dashboard charts retrieved successfully", 200, charts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard charts");
                return ErrorResponse.Build($"Error retrieving charts: {ex.Message}", 500);
            }
        }

        // --- Helper Methods ---
        private async Task EnrichUserCodeAsync(ApplicationUser user, GetUserDto userDto)
        {
            // 1. Lấy Roles từ UserManager (QUAN TRỌNG!)
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();

            // 2. Enrich StudentCode hoặc TeacherCode dựa trên Role
            if (userDto.Roles.Contains(StaticUserRoles.Student))
            {
                var student = await _unitOfWork.Student.GetAsync(s => s.UserId == user.Id);
                userDto.StudentCode = student?.StudentCode;
            }
            else if (userDto.Roles.Contains(StaticUserRoles.Teacher))
            {
                var teacher = await _unitOfWork.Teacher.GetAsync(t => t.UserId == user.Id);
                userDto.TeacherCode = teacher?.TeacherCode;
            }
        }
    }
}
