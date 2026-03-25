using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories;

public interface ILearningClassMemberRepository : IRepository<LearningClassMember>
{
    Task<LearningClassMember?> GetByClassAndUserAsync(Guid learningClassId, string userId);
    Task<List<LearningClassMember>> GetByClassAsync(Guid learningClassId);
    Task<List<LearningClassMember>> GetByUserAsync(string userId);
    void Update(LearningClassMember member);
}
