using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ManageUser;

namespace AttaEduSystem.Services.IServices
{
    public interface IManageUserAccountService
    {
        Task<ResponseDto> LockUser(LockUserDto lockUserDto);
        Task<ResponseDto> UnlockUser(UnlockUserDto unLockUserDto);
    }
}
