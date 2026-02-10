using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamRoomParticipantRepository : Repository<ExamRoomParticipant>, IExamRoomParticipantRepository
    {
        private readonly ApplicationDBContext _context;

        public ExamRoomParticipantRepository(ApplicationDBContext context) : base(context)
        {
            _context = context; 
        }

        public async Task<ExamRoomParticipant?> GetByRoomAndUserAsync(Guid examRoomId, string userId)
        {
            return await _context.ExamRoomParticipants
                .Include(p => p.ExamRoom)
                    .ThenInclude(r => r.ExamPaper)
                .Include(p => p.ExamAttempt)
                .FirstOrDefaultAsync(p => p.ExamRoomId == examRoomId && p.UserId == userId);
        }

        public async Task<List<ExamRoomParticipant>> GetByRoomIdAsync(Guid examRoomId)
        {
            return await _context.ExamRoomParticipants
                .Include(p => p.User)
                .Include(p => p.ExamAttempt)
                .Where(p => p.ExamRoomId == examRoomId)
                .OrderBy(p => p.JoinedAt)
                .ToListAsync();
        }

        public async Task<int> GetParticipantCountAsync(Guid examRoomId)
        {
            return await _context.ExamRoomParticipants
                .CountAsync(p => p.ExamRoomId == examRoomId);
        }

        public void Update(ExamRoomParticipant participant)
        {
            _context.ExamRoomParticipants.Update(participant);
        }
    }
}
