using AttaEduSystem.Models.Entities;

namespace AttaEduSystem.DataAccess.IRepositories;

public interface IExamAttemptRepository : IRepository<ExamAttempt>
{
    // Lấy lịch sử thi của user (kèm thông tin đề thi)
    Task<List<ExamAttempt>> GetHistoryByUserIdAsync(string userId);

    // Lấy chi tiết 1 lần thi (kèm câu hỏi và câu trả lời)
    Task<ExamAttempt?> GetAttemptWithDetailsAsync(Guid attemptId);
}