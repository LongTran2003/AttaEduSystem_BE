using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.Repositories;

public class ExamFolderRepository : Repository<ExamFolder>, IExamFolderRepository
{
    private readonly ApplicationDBContext _context;

    public ExamFolderRepository(ApplicationDBContext context) : base(context)
    {
        _context = context;
    }
}