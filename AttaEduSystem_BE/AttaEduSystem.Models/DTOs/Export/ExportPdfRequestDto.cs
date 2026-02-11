namespace AttaEduSystem.Models.DTOs.Export
{
    public class ExportPdfRequestDto
    {
        /// <summary>
        /// Source type: "ExamPaper" or "GeneratedExam"
        /// </summary>
        public string Source { get; set; } = "ExamPaper";

        /// <summary>
        /// Include answer key at the end of PDF
        /// </summary>
        public bool IncludeAnswers { get; set; } = false;

        /// <summary>
        /// Upload to Cloudinary and return URL instead of file stream
        /// </summary>
        //public bool UploadToCloud { get; set; } = false;
    }
}
