using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AttaEduSystem.Services.Services
{
    public class AzureVisionOcrService : IOcrService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureVisionOcrService> _logger;
        private readonly ComputerVisionClient _client;

        public AzureVisionOcrService(IConfiguration configuration, ILogger<AzureVisionOcrService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Lấy thông tin từ appsettings.json (sẽ cấu hình ở bước 3)
            var endpoint = _configuration["AzureVision:Endpoint"];
            var apiKey = _configuration["AzureVision:ApiKey"];

            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string> ExtractText(IFormFile imageFile)
        {
            try
            {
                using var stream = imageFile.OpenReadStream();
                // Sử dụng ReadAsync để đọc OCR
                var textHeaders = await _client.ReadInStreamAsync(stream);

                // Lấy ID hoạt động từ headers để polling kết quả
                string operationLocation = textHeaders.OperationLocation;
                string operationId = operationLocation.Substring(operationLocation.Length - 36);

                ReadOperationResult results;
                do
                {
                    await Task.Delay(1000); // Đợi 1 giây trước khi check lại
                    results = await _client.GetReadResultAsync(Guid.Parse(operationId));
                } while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

                // Tổng hợp kết quả
                var textBuilder = new StringBuilder();
                foreach (var page in results.AnalyzeResult.ReadResults)
                {
                    foreach (var line in page.Lines)
                    {
                        textBuilder.AppendLine(line.Text);
                    }
                }
                return textBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Azure OCR");
                throw;
            }
        }
    }
}
