using AttaEduSystem.Models.DTOs.Export;
using System.Security.Claims;

namespace AttaEduSystem.Services.IServices
{
    public interface IExportService
    {
        /// <summary>
        /// Export exam to PDF
        /// </summary>
        /// <param name="examId">ID of ExamPaper or GeneratedExamPaper</param>
        /// <param name="request">Export options</param>
        /// <returns>PDF bytes or Cloudinary URL</returns>
        Task<(byte[]? PdfBytes, string? ErrorMessage)> ExportToPdfAsync(
            Guid examId,
            ExportPdfRequestDto request,
            ClaimsPrincipal user);
    }
}
