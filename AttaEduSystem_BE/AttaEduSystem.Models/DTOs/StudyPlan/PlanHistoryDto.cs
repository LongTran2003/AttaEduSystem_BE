namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// Lịch sử các plan đã lưu (cho list view)
    /// </summary>
    public class PlanHistoryDto
    {
        public Guid StudyPlanId { get; set; }
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public string Status { get; set; } = null!;
        public double CompletionPercentage { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
    }
}
