namespace AttaEduSystem.Models.DTOs.Admin.Dashboards
{
    public class ExamCreationChartDto
    {
        public string Month { get; set; } = null!;
        public string MonthLabel { get; set; } = null!;
        public int ScannedExams { get; set; } // ExamPaper
        public int AiGeneratedExams { get; set; } // GeneratedExamPaper
        public int Total { get; set; }
    }
}
