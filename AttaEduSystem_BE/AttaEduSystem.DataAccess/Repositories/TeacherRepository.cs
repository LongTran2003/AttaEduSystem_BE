using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class TeacherRepository : Repository<Teacher>, ITeacherRepository
    {
        private readonly ApplicationDBContext _context;

        public TeacherRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Teacher teacher)
        {
            _context.Teachers.Attach(teacher);
            _context.Entry(teacher).State = EntityState.Modified;
        }

        public async Task<string> GetNextTeacherCodeAsync()
        {

            // Lấy 2 số cuối của năm hiện tại
            int currentYear = DateTime.Now.Year % 100;
            string prefix = $"S{currentYear:D2}-";

            // Lấy danh sách các phần số của TeacherCode (sau dấu gạch)
            var codeParts = await _context.Teachers
                .Where(s => s.TeacherCode.StartsWith(prefix))
                .Select(s => s.TeacherCode.Substring(prefix.Length))
                .ToListAsync();

            // Chuyển đổi chuỗi sang số và tìm số lớn nhất
            int maxNumber = codeParts
                .Select(codePart =>
                {
                    int number;
                    return int.TryParse(codePart, out number) ? number : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            int nextNumber = maxNumber + 1;
            return $"{prefix}{nextNumber:D4}";
        }
    }
}
