using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ChatConversationRepository : Repository<ChatConversation>, IChatConversationRepository
    {
        private readonly ApplicationDBContext _context;

        public ChatConversationRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<ChatConversation> Conversations, int TotalCount)> GetConversationsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            string status = "Active")
        {
            var query = _context.ChatConversations
                .Include(c => c.Messages)
                .Where(c => c.UserId == userId && c.Status == status)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            // Sorting
            query = (sortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "title" => query.OrderBy(c => c.Title),
                "title_desc" => query.OrderByDescending(c => c.Title),
                "created" => query.OrderBy(c => c.CreatedTime),
                "created_desc" => query.OrderByDescending(c => c.CreatedTime),
                _ => query.OrderByDescending(c => c.UpdatedAt) // Default: recent first
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<ChatConversation?> GetByIdWithMessagesAsync(Guid conversationId, string userId)
        {
            return await _context.ChatConversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedTime))
                .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && c.UserId == userId);
        }

        public void Update(ChatConversation conversation)
        {
            _context.ChatConversations.Update(conversation);
        }
    }
}
