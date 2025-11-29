using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class QuestionOptionRepository : Repository<QuestionOption>, IQuestionOptionRepository
    {
        private readonly ApplicationDBContext _context;
        public QuestionOptionRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
