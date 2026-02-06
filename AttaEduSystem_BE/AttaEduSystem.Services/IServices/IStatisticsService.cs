using System.Security.Claims;
using AttaEduSystem.Models.DTOs;

namespace AttaEduSystem.Services.IServices;

public interface IStatisticsService
{
    Task<ResponseDto> GetOverviewStats(ClaimsPrincipal user);
    Task<ResponseDto> GetProgressChart(ClaimsPrincipal user, int days = 7); // Mặc định lấy 7 ngày gần nhất
}