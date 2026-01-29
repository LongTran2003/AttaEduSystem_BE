namespace AttaEduSystem.Models.DTOs.Billing
{
    public class GetUsageInfoDto
    {
        public int TokensUsed { get; set; }
        public int ScansUsed { get; set; }
        public int GeneratedExamsUsed { get; set; }
        public int MaxTokens { get; set; }
        public int MaxScans { get; set; }
        public int MaxGeneratedExams { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
