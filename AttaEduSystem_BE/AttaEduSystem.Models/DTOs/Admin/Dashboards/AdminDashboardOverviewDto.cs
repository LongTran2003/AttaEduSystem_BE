namespace AttaEduSystem.Models.DTOs.Admin.Dashboards
{
    public class AdminDashboardOverviewDto
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }

        // Content Statistics
        public int TotalExamPapers { get; set; }
        public int TotalGeneratedExams { get; set; }
        public int TotalExamRooms { get; set; }

        // Subscription Statistics
        public int TotalActiveSubscriptions { get; set; }
        public int TotalFreeUsers { get; set; }
        public int TotalProUsers { get; set; }

        // Revenue Statistics
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
    }
}
