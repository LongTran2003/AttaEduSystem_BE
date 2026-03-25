using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories;

public class LearningClassRepository : Repository<LearningClass>, ILearningClassRepository
{
    private readonly ApplicationDBContext _context;

    public LearningClassRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<LearningClass?> GetByIdWithMembersAsync(Guid learningClassId)
    {
        return await _context.LearningClasses
            .Include(c => c.OwnerUser)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.LearningClassId == learningClassId);
    }

    public async Task<List<LearningClass>> GetByOwnerAsync(string ownerUserId)
    {
        return await _context.LearningClasses
            .Include(c => c.OwnerUser)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Where(c => c.OwnerUserId == ownerUserId)
            .OrderByDescending(c => c.CreatedTime)
            .ToListAsync();
    }

    public async Task<List<LearningClass>> GetAllWithMembersAsync()
    {
        return await _context.LearningClasses
            .Include(c => c.OwnerUser)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .OrderByDescending(c => c.CreatedTime)
            .ToListAsync();
    }

    public void Update(LearningClass learningClass)
    {
        _context.LearningClasses.Update(learningClass);
    }
}
