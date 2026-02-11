namespace AttaEduSystem.Models.DTOs.Admin.Dashboards
{
    public class RevenueChartDto
    {
        public string Month { get; set; } = null!;
        public string MonthLabel { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }
}
