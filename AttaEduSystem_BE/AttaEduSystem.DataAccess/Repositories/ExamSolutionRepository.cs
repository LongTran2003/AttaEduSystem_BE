using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamSolutionRepository : Repository<ExamSolution>, IExamSolutionRepository
    {
        private readonly ApplicationDBContext _context;
        public ExamSolutionRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public void Update(ExamSolution examSolution)
        {
            _context.ExamSolutions.Update(examSolution);
        }
    }
}
