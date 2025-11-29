using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class ExamQuestionRepository : Repository<ExamQuestion>, IExamQuestionRepository
    {
        private readonly ApplicationDBContext _context;
        public ExamQuestionRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
