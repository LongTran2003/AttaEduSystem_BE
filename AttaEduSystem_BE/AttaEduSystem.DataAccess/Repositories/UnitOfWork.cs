using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;
        public IStudentRepository Student { get; private set; }
        public ITeacherRepository Teacher { get; private set; }

        public UnitOfWork(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            Student = new StudentRepository(_context);
            Teacher = new TeacherRepository(_context);
        }



        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
