using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamRoomRepository : Repository<ExamRoom>, IExamRoomRepository
    {
        private readonly ApplicationDBContext _context;

        public ExamRoomRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ExamRoom?> GetByCodeAsync(string code)
        {
            return await _context.ExamRooms
                .Include(r => r.ExamPaper)
                .FirstOrDefaultAsync(r => r.RoomCode == code);
        }

        public async Task<ExamRoom?> GetByCodeWithParticipantsAsync(string code)
        {
            return await _context.ExamRooms
                .Include(r => r.ExamPaper)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.RoomCode == code);
        }

        public async Task<ExamRoom?> GetByIdWithParticipantsAsync(Guid examRoomId)
        {
            return await _context.ExamRooms
                .Include(r => r.ExamPaper)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.ExamRoomId == examRoomId);
        }

        public async Task<List<ExamRoom>> GetByCreatorIdAsync(string userId)
        {
            return await _context.ExamRooms
                .Include(r => r.ExamPaper)
                .Include(r => r.Participants)
                .Where(r => r.CreatedBy == userId)
                .OrderByDescending(r => r.CreatedTime)
                .ToListAsync();
        }

        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await _context.ExamRooms.AnyAsync(r => r.RoomCode == code);
        }

        public void Update(ExamRoom examRoom)
        {
            _context.ExamRooms.Update(examRoom);
        }
    }
}
