using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ChatBox;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IChatService
    {
        /// <summary>
        /// Tạo conversation mới
        /// </summary>
        Task<ResponseDto> CreateConversationAsync(CreateConversationDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Lấy danh sách conversations của user
        /// </summary>
        Task<ResponseDto> GetConversationsAsync(ClaimsPrincipal user, int page = 1, int pageSize = 20);

        /// <summary>
        /// Lấy chi tiết conversation + messages
        /// </summary>
        Task<ResponseDto> GetConversationByIdAsync(Guid conversationId, ClaimsPrincipal user);

        /// <summary>
        /// Gửi message và nhận AI reply
        /// </summary>
        Task<ResponseDto> SendMessageAsync(Guid conversationId, SendMessageDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Xóa conversation
        /// </summary>
        Task<ResponseDto> DeleteConversationAsync(Guid conversationId, ClaimsPrincipal user);
    }
}
