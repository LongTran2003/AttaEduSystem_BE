using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories;

public class LearningClassMemberRepository : Repository<LearningClassMember>, ILearningClassMemberRepository
{
    private readonly ApplicationDBContext _context;

    public LearningClassMemberRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<LearningClassMember?> GetByClassAndUserAsync(Guid learningClassId, string userId)
    {
        return await _context.LearningClassMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.LearningClassId == learningClassId && m.UserId == userId);
    }

    public async Task<List<LearningClassMember>> GetByClassAsync(Guid learningClassId)
    {
        return await _context.LearningClassMembers
            .Include(m => m.User)
            .Where(m => m.LearningClassId == learningClassId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.CreatedTime)
            .ToListAsync();
    }

    public async Task<List<LearningClassMember>> GetByUserAsync(string userId)
    {
        return await _context.LearningClassMembers
            .Include(m => m.LearningClass)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedTime)
            .ToListAsync();
    }

    public void Update(LearningClassMember member)
    {
        _context.LearningClassMembers.Update(member);
    }
}
