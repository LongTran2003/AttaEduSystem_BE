using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ManageUser;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Identity;

namespace AttaEduSystem.Services.Services
{
    public class ManageUserAccountService : IManageUserAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageUserAccountService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ResponseDto> LockUser(LockUserDto lockUserDto)
        {
            if (string.IsNullOrEmpty(lockUserDto.UserId))
            {
                return new ResponseDto
                {
                    Message = "User ID is required.",
                    IsSuccess = false,
                    StatusCode = 400
                };
            }

            var user = await _userManager.FindByIdAsync(lockUserDto.UserId);
            if (user == null)
            {
                return new ResponseDto
                {
                    Message = $"User with ID '{lockUserDto.UserId}' not found.",
                    IsSuccess = false,
                    StatusCode = 404
                };
            }

            if (lockUserDto.LockoutEndDate.HasValue)
            {
                user.LockoutEnd = lockUserDto.LockoutEndDate.Value;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            IdentityResult result = await _userManager.SetLockoutEndDateAsync(user, user.LockoutEnd);

            if (result.Succeeded)
            {
                return new ResponseDto
                {
                    Message = "Lock user successfully",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new
                    {
                        user.LockoutEnd,
                        lockUserDto.UserId
                    }
                };
            }

            return new ResponseDto
            {
                Message = "Failed to lock user. Please check logs for details.",
                IsSuccess = false,
                StatusCode = 500
            };
        }

        public async Task<ResponseDto> UnlockUser(UnlockUserDto unLockUserDto)
        {
            var user = await _userManager.FindByIdAsync(unLockUserDto.UserId);

            if (user is null)
            {
                return new ResponseDto()
                {
                    Message = "User was not found",
                    IsSuccess = false,
                    StatusCode = 404,
                    Result = null
                };
            }

            user.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ResponseDto()
                {
                    Message = "Unlock user was failed",
                    IsSuccess = false,
                    StatusCode = 400,
                    Result = null
                };
            }

            return new ResponseDto()
            {
                Message = "Unlock user successfully",
                IsSuccess = true,
                StatusCode = 200,
                Result = unLockUserDto.UserId
            };
        }
    }
}
