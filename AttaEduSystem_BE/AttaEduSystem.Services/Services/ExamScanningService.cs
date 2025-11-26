using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AttaEduSystem.Services.Services
{
    public class ExamScanningService : IExamScanningService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExamScanningService> _logger;

        public ExamScanningService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService, IConfiguration configuration, ILogger<ExamScanningService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResponseDto> GetExamPaperById(Guid examPaperId)
        {
            var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
            if (examPaper == null)
            {
                return ErrorResponse.Build(
                    message: "Exam paper not found",
                    statusCode: StaticOperationStatus.StatusCode.NotFound);
            }

            var dto = _mapper.Map<GetExamPaperDto>(examPaper);

            return SuccessResponse.Build(
                message: "Exam paper retrieved successfully",
                statusCode: StaticOperationStatus.StatusCode.Ok,
                result: dto);
        }

        public async Task<ResponseDto> GetExamPapersByUser(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(
                    message: StaticOperationStatus.User.UserNotFound,
                    statusCode: StaticOperationStatus.StatusCode.Unauthorized);
            }

            var examPapers = await _unitOfWork.ExamPaper.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<GetExamPaperDto>>(examPapers);

            return SuccessResponse.Build(
                message: "Exam papers retrieved successfully",
                statusCode: StaticOperationStatus.StatusCode.Ok,
                result: dtos);
        }

        public async Task<ResponseDto> GetScannedText(Guid examPaperId)
        {
            var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
            if (examPaper == null)
            {
                return ErrorResponse.Build(
                    message: "Exam paper not found",
                    statusCode: StaticOperationStatus.StatusCode.NotFound);
            }

            return SuccessResponse.Build(
                message: "Scanned text retrieved successfully",
                statusCode: StaticOperationStatus.StatusCode.Ok,
                result: new { examPaper.ScannedText });
        }

        public async Task<ResponseDto> ScanExamPaper(UploadExamPaperDto uploadDto, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return ErrorResponse.Build(
                        message: StaticOperationStatus.User.UserNotFound,
                        statusCode: StaticOperationStatus.StatusCode.Unauthorized);
                }

                if (uploadDto.ExamImage == null || uploadDto.ExamImage.Length == 0)
                {
                    return ErrorResponse.Build(
                        message: StaticOperationStatus.File.FileEmpty,
                        statusCode: StaticOperationStatus.StatusCode.BadRequest);
                }

                var maxFileSize = _configuration.GetValue<long>("ExamPaperSettings:MaxFileSize", 10485760);
                if (uploadDto.ExamImage.Length > maxFileSize)
                {
                    return ErrorResponse.Build(
                        message: "File size exceeds maximum allowed size",
                        statusCode: StaticOperationStatus.StatusCode.BadRequest);
                }

                var folderPath = $"exam-papers/{userId}";
                string imageUrl;
                try
                {
                    imageUrl = await _cloudinaryService.UploadImageAsync(uploadDto.ExamImage, folderPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading exam image");
                    return ErrorResponse.Build(
                        message: "Failed to upload exam image",
                        statusCode: StaticOperationStatus.StatusCode.InternalServerError);
                }

                string scannedText;
                try
                {
                    scannedText = await PerformOcr(uploadDto.ExamImage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing OCR");
                    return ErrorResponse.Build(
                        message: "Failed to scan exam paper",
                        statusCode: StaticOperationStatus.StatusCode.InternalServerError);
                }

                var examPaper = _mapper.Map<ExamPaper>(uploadDto);
                examPaper.ExamPaperId = Guid.NewGuid();
                examPaper.OriginalImageUrl = imageUrl;
                examPaper.ScannedText = scannedText;
                examPaper.CreatedBy = userId;
                examPaper.CreatedTime = StaticOperationStatus.Timezone.Vietnam;
                examPaper.Status = StaticOperationStatus.ExamPaper.Draft;

                await _unitOfWork.ExamPaper.AddAsync(examPaper);
                await _unitOfWork.SaveAsync();

                var response = new ScanExamPaperResponseDto
                {
                    ExamPaperId = examPaper.ExamPaperId,
                    Title = examPaper.Title,
                    ScannedText = examPaper.ScannedText ?? string.Empty,
                    ImageUrl = examPaper.OriginalImageUrl,
                    ExamFormat = examPaper.ExamFormat,
                    Subject = examPaper.Subject,
                    ScannedAt = examPaper.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam
                };

                return SuccessResponse.Build(
                    message: "Exam paper scanned successfully",
                    statusCode: StaticOperationStatus.StatusCode.Created,
                    result: response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ScanExamPaper");
                return ErrorResponse.Build(
                    message: "An unexpected error occurred",
                    statusCode: StaticOperationStatus.StatusCode.InternalServerError);
            }
        }

        private async Task<string> PerformOcr(IFormFile imageFile)
        {
            var apiKey = _configuration["GoogleCloudVision:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                return await PerformOcrWithApiKey(imageFile, apiKey);
            }

            var credentialsPath = _configuration["GoogleCloudVision:CredentialsPath"];
            if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            }

            var client = await new ImageAnnotatorClientBuilder().BuildAsync();
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var image = Image.FromBytes(memoryStream.ToArray());

            var response = await client.DetectTextAsync(image);
            return string.Join("\n", response.Select(annotation => annotation.Description));
        }

        private async Task<string> PerformOcrWithApiKey(IFormFile imageFile, string apiKey)
        {
            using var httpClient = new HttpClient();
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            var base64Image = Convert.ToBase64String(memoryStream.ToArray());
            var requestBody = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = base64Image },
                        features = new[]
                        {
                            new { type = "TEXT_DETECTION", maxResults = 1 }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(
                $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}", content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleVisionResponse>(responseJson);

            return result?.responses?.FirstOrDefault()?.textAnnotations?.FirstOrDefault()?.description ?? string.Empty;
        }

        private class GoogleVisionResponse
        {
            public List<ResponseItem>? responses { get; set; }
        }

        private class ResponseItem
        {
            public List<TextAnnotation>? textAnnotations { get; set; }
        }

        private class TextAnnotation
        {
            public string? description { get; set; }
        }
    }
}
