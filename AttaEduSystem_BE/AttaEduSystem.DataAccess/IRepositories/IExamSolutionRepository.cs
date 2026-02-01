using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories
{
    public interface IExamSolutionRepository : IRepository<ExamSolution>
    {
        void Update(ExamSolution examSolution);
    }
}
