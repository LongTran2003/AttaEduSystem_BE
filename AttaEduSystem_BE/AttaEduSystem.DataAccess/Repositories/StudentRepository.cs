using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttaEduSystem.DataAccess.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        private readonly ApplicationDBContext _context;
        public StudentRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string> GetNextStudentCodeAsync()
        {
            // Format: ST{Year}-{Sequence} (Ví dụ: ST24-0001)

            int currentYear = DateTime.UtcNow.Year % 100;
            string prefix = $"ST{currentYear:D2}-";

            // Lấy danh sách các mã student hiện tại bắt đầu bằng prefix
            var codeParts = await _context.Students
                .Where(s => s.StudentCode.StartsWith(prefix))
                .Select(s => s.StudentCode.Substring(prefix.Length))
                .ToListAsync();

            // Tìm số lớn nhất
            int maxNumber = 0;
            if (codeParts.Any())
            {
                maxNumber = codeParts
                    .Select(part => int.TryParse(part, out int n) ? n : 0)
                    .Max();
            }

            int nextNumber = maxNumber + 1;

            return $"{prefix}{nextNumber:D4}";
        }
    }
}

