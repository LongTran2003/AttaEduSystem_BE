using System.Security.Claims;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Statistics;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.Services.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatisticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseDto> GetOverviewStats(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        // 1. Lấy toàn bộ lịch sử làm bài của User
        // Lưu ý: Nếu data quá lớn sau này cần tối ưu bằng SQL Query trực tiếp
        var attempts = await _unitOfWork.ExamAttempt.GetAllAsync(x => x.UserId == userId);

        if (attempts == null || !attempts.Any())
        {
            return SuccessResponse.Build("No data", 200, new OverviewStatsDto());
        }

        // 2. Tính toán
        var stats = new OverviewStatsDto
        {
            TotalExamsTaken = attempts.Count(),
            AverageScore = Math.Round(attempts.Average(x => x.Score), 2),
            HighestScore = attempts.Max(x => x.Score),
            TotalQuestionsAnswered = attempts.Sum(x => x.TotalQuestions)
        };

        return SuccessResponse.Build("Overview stats retrieved", 200, stats);
    }

    public async Task<ResponseDto> GetProgressChart(ClaimsPrincipal user, int days = 7)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        // 1. Lấy dữ liệu trong khoảng ngày yêu cầu
        var fromDate = DateTime.UtcNow.AddDays(-days);
            
        // Lấy các bài thi đã hoàn thành trong N ngày qua
        var attempts = await _unitOfWork.ExamAttempt.GetAllAsync(x => 
            x.UserId == userId && 
            x.CompletedAt.HasValue && 
            x.CompletedAt >= fromDate);

        // 2. Group by Date (Gom nhóm theo ngày)
        // Logic: Những ngày nào không làm bài thì không hiện, hoặc Frontend tự điền 0
        var chartData = attempts
            .GroupBy(x => x.CompletedAt.Value.Date)
            .Select(g => new ProgressChartDto
            {
                Date = g.Key,
                DateLabel = g.Key.ToString("dd/MM"), // Format ngày/tháng
                AverageScore = Math.Round(g.Average(x => x.Score), 2),
                ExamCount = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        return SuccessResponse.Build("Chart data retrieved", 200, chartData);
    }
}