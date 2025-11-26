using AttaEduSystem.Services.IServices;
using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class GoogleVisionOcrService : IOcrService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleVisionOcrService> _logger;

        public GoogleVisionOcrService(IConfiguration configuration, ILogger<GoogleVisionOcrService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ExtractText(IFormFile imageFile)
        {
            var apiKey = _configuration["GoogleCloudVision:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                return await ExtractWithApiKey(imageFile, apiKey);
            }

            var credentialsPath = _configuration["GoogleCloudVision:CredentialsPath"];
            if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            }

            var client = await new ImageAnnotatorClientBuilder().BuildAsync();
            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var image = Image.FromBytes(ms.ToArray());

            var response = await client.DetectTextAsync(image);
            return string.Join("\n", response.Select(annotation => annotation.Description));
        }

        private async Task<string> ExtractWithApiKey(IFormFile imageFile, string apiKey)
        {
            using var httpClient = new HttpClient();
            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());

            var request = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = base64 },
                        features = new[] { new { type = "TEXT_DETECTION", maxResults = 1 } }
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}",
                content);

            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<GoogleVisionResponse>(
                await response.Content.ReadAsStringAsync());

            return result?.responses?.FirstOrDefault()?.textAnnotations?.FirstOrDefault()?.description ?? string.Empty;
        }

        private sealed class GoogleVisionResponse
        {
            public List<ResponseItem>? responses { get; set; }
        }

        private sealed class ResponseItem
        {
            public List<TextAnnotation>? textAnnotations { get; set; }
        }

        private sealed class TextAnnotation
        {
            public string? description { get; set; }
        }
    }
}
