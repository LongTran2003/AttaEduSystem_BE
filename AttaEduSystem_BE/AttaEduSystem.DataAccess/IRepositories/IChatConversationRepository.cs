using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IChatConversationRepository : IRepository<ChatConversation>
    {
        /// <summary>
        /// Lấy danh sách conversations với pagination
        /// </summary>
        Task<(List<ChatConversation> Conversations, int TotalCount)> GetConversationsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            string status = "Active");

        /// <summary>
        /// Lấy conversation với messages
        /// </summary>
        Task<ChatConversation?> GetByIdWithMessagesAsync(Guid conversationId, string userId);

        void Update(ChatConversation conversation);
    }
}
