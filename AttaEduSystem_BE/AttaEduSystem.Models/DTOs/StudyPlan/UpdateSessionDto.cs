namespace AttaEduSystem.Models.DTOs.StudyPlan
{
    /// <summary>
    /// DTO để update trạng thái hoàn thành của buổi học
    /// </summary>
    public class UpdateSessionDto
    {
        public string DayOfWeek { get; set; } = null!;
        public int SessionOrder { get; set; }
        public bool IsCompleted { get; set; }
        public string? CompletionNotes { get; set; }
    }
}
