using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface ITeacherRepository : IRepository<Teacher>
    {
        void Update(Teacher teacher);
        Task<string> GetNextTeacherCodeAsync();
    }
}
