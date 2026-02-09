using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Admin;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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

        // --- Helper Methods ---
        private async Task EnrichUserCodeAsync(ApplicationUser user, GetUserDto userDto)
        {
            if (userDto.Roles != null && userDto.Roles.Contains(StaticUserRoles.Student))
            {
                var student = await _unitOfWork.Student.GetAsync(s => s.UserId == user.Id);
                userDto.StudentCode = student?.StudentCode;
            }
            else if (userDto.Roles != null && userDto.Roles.Contains(StaticUserRoles.Teacher))
            {
                var teacher = await _unitOfWork.Teacher.GetAsync(t => t.UserId == user.Id);
                userDto.TeacherCode = teacher?.TeacherCode;
            }
        }
    }
}
