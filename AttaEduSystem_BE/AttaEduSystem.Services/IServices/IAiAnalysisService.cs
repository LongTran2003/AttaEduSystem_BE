namespace AttaEduSystem.Services.IServices
{
    /// <summary>
    /// Service interface for AI-powered exam analysis with fallback strategy
    /// </summary>
    public interface IAiAnalysisService
    {
        /// <summary>
        /// Analyze exam structure from image with automatic fallback between AI providers
        /// </summary>
        /// <param name="base64Image">Base64 encoded image</param>
        /// <param name="mimeType">Image MIME type (e.g., image/jpeg)</param>
        /// <returns>JSON string containing exam structure</returns>
        Task<string> AnalyzeExamStructureWithFallback(string base64Image, string mimeType);
    }
}
