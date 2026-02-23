namespace AttaEduSystem.Services.IServices
{
    public interface IGeminiAiService
    {
        Task<string> AnalyzeExamStructure(string base64Image, string mimeType);
        Task<string> SolveExam(string examContentJson);
        Task<string> GenerateSimilarExam(string examStructureJson);
        Task<string> GenerateStudyPlan(string subjectPerformanceJson, string preferencesJson);
    }
}
