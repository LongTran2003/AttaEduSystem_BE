namespace AttaEduSystem.Models.DTOs.Admin.Dashboards
{
    public class AdminDashboardChartsDto
    {
        public List<UserGrowthChartDto> UserGrowth { get; set; } = new();
        public List<ExamCreationChartDto> ExamCreationTrend { get; set; } = new();
        public List<RevenueChartDto> RevenueChart { get; set; } = new();
    }
}
