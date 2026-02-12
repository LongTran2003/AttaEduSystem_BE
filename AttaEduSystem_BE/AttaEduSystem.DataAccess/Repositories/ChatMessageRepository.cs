using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        private readonly ApplicationDBContext _context;

        public ChatMessageRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
