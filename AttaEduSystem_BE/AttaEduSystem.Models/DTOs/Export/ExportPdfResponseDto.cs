namespace AttaEduSystem.Models.DTOs.Export
{
    public class ExportPdfResponseDto
    {
        public string DownloadUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string ExportedBy { get; set; } = null!;
        public DateTime ExportedAt { get; set; }
    }
}
