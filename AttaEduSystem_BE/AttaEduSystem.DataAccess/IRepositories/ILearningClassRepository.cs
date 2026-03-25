using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories;

public interface ILearningClassRepository : IRepository<LearningClass>
{
    Task<LearningClass?> GetByIdWithMembersAsync(Guid learningClassId);
    Task<List<LearningClass>> GetByOwnerAsync(string ownerUserId);
    Task<List<LearningClass>> GetAllWithMembersAsync();
    void Update(LearningClass learningClass);
}
