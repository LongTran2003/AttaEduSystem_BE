namespace AttaEduSystem.Models.DTOs.Admin.Dashboards
{
    public class UserGrowthChartDto
    {
        public string Month { get; set; } = null!; // "2026-01"
        public string MonthLabel { get; set; } = null!; // "Jan 2026"
        public int NewUsers { get; set; }
        public int CumulativeUsers { get; set; } // Tổng tích lũy
    }
}
