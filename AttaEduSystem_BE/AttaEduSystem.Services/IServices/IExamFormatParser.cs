using AttaEduSystem.Models.DTOs.ExamFormat;

namespace AttaEduSystem.Services.IServices
{
    public interface IExamFormatParser
    {
        ExamFormatSchema Parse(string scannedText);
    }
}
