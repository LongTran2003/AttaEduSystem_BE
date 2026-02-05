using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.Repositories;

public class ExamAttemptDetailRepository : Repository<ExamAttemptDetail>, IExamAttemptDetailRepository
{
    private readonly ApplicationDBContext _context;
    
    public ExamAttemptDetailRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }
}