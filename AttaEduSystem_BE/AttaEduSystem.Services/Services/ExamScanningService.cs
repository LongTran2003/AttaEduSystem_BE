using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class ExamScanningService : IExamScanningService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExamScanningService> _logger;
        private readonly IOcrService _ocrService;
        private readonly IGeminiAiService _geminiAiService;

        public ExamScanningService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            ICloudinaryService cloudinaryService,
            IConfiguration configuration,
            ILogger<ExamScanningService> logger,
            IOcrService ocrService,
            IGeminiAiService geminiAiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
            _logger = logger;
            _ocrService = ocrService;
            _geminiAiService = geminiAiService;
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

        public async Task<ResponseDto> GetAllReadyExamPapers(
            int pageNumber = 1,
            int pageSize = 10,
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null)
        {
            try
            {
                var (papers, totalCount) = await _unitOfWork.ExamPaper.GetExamPapersAsync(
                    pageNumber,
                    pageSize,
                    filterOn,
                    filterQuery,
                    sortBy,
                    includeProperties: null,
                    status: StaticOperationStatus.ExamPaper.Ready);

                if (!papers.Any())
                {
                    var emptyResult = new
                    {
                        Data = Enumerable.Empty<GetExamPaperDto>(),
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0,
                        TotalPages = 0,
                        HasPreviousPage = false,
                        HasNextPage = false
                    };

                    return SuccessResponse.Build(
                        message: StaticResponseMessage.ExamPaper.Found,
                        statusCode: StaticOperationStatus.StatusCode.Ok,
                        result: emptyResult); 
                }

                var dtos = _mapper.Map<IEnumerable<GetExamPaperDto>>(papers);
                var payload = new
                {
                    Data = dtos,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber * pageSize < totalCount
                };

                return SuccessResponse.Build(
                    message: "Ready exam papers retrieved successfully",
                    statusCode: StaticOperationStatus.StatusCode.Ok,
                    result: payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ready exam papers");
                return ErrorResponse.Build(
                    message: "Failed to retrieve ready exam papers",
                    statusCode: StaticOperationStatus.StatusCode.InternalServerError);
            }
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

                // Convert ảnh sang base64
                string base64Image;
                await using (var ms = new MemoryStream())
                {
                    await uploadDto.ExamImage.CopyToAsync(ms);
                    base64Image = Convert.ToBase64String(ms.ToArray());
                }

                // Upload Cloudinary
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

                // OCR  
                string scannedText;
                try
                {
                    scannedText = await _ocrService.ExtractText(uploadDto.ExamImage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing OCR");
                    return ErrorResponse.Build(
                        message: "Failed to scan exam paper",
                        statusCode: StaticOperationStatus.StatusCode.InternalServerError);
                }

                // Gọi Gemini
                string aiResponseJson;
                try
                {
                    aiResponseJson = await _geminiAiService.AnalyzeExamStructure(
                        base64Image,
                        uploadDto.ExamImage.ContentType ?? "image/jpeg");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling Gemini API");
                    return ErrorResponse.Build("AI Service Unreachable: " + ex.Message, 500);
                }

                // Tạm thời bỏ Parse format vì chưa ổn định được format đề thi  

                //// Parse format
                //ExamFormatSchema? schema = null;
                //string? examFormatJson = null;
                //try
                //{
                //    schema = _examFormatParser.Parse(scannedText);
                //    examFormatJson = JsonSerializer.Serialize(schema);
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogWarning(ex, "Failed to parse exam format, continuing with raw OCR text");
                //}

                var examPaper = _mapper.Map<ExamPaper>(uploadDto);
                examPaper.ExamPaperId = Guid.NewGuid();
                examPaper.Title = string.IsNullOrWhiteSpace(examPaper.Title)
                    ? $"Exam Paper - {StaticOperationStatus.Timezone.Vietnam:yyyy-MM-dd HH:mm}"
                    : examPaper.Title;
                examPaper.OriginalImageUrl = imageUrl;
                examPaper.ScannedText = aiResponseJson;
                //examPaper.ExamFormat = examFormatJson;
                examPaper.CreatedBy = userId;
                examPaper.CreatedTime = StaticOperationStatus.Timezone.Vietnam;
                examPaper.Status = StaticOperationStatus.ExamPaper.Ready;

                await _unitOfWork.ExamPaper.AddAsync(examPaper);
                await _unitOfWork.SaveAsync();

                var responseDto = _mapper.Map<ScanExamPaperResponseDto>(examPaper);
                //responseDto.ExamFormatParsed ??= schema; // fallback nếu mapper null

                return SuccessResponse.Build(
                    message: "Exam paper scanned successfully",
                    statusCode: StaticOperationStatus.StatusCode.Created,
                    result: responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ScanExamPaper");
                return ErrorResponse.Build(
                    message: "An unexpected error occurred",
                    statusCode: StaticOperationStatus.StatusCode.InternalServerError);
            }
        }

        public async Task<ResponseDto> GetExamPaperImage(Guid examPaperId)
        {
            var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
            if (examPaper == null)
            {
                return ErrorResponse.Build("Exam paper not found", 404);
            }

            return SuccessResponse.Build("Image URL retrieved successfully", 200,
                new { examPaper.OriginalImageUrl });
        }

        public async Task<ResponseDto> UpdateExamPaperStatus(Guid examPaperId, string status, ClaimsPrincipal user)
        {
            var allowed = new[] { StaticOperationStatus.ExamPaper.Draft, StaticOperationStatus.ExamPaper.Ready, StaticOperationStatus.ExamPaper.Removed };
            if (!allowed.Contains(status))
            {
                return ErrorResponse.Build("Invalid status value", 400);
            }

            var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
            if (examPaper == null)
            {
                return ErrorResponse.Build("Exam paper not found", 404);
            }

            // Optional: check user role or ownership before allowing change
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
            }

            examPaper.Status = status;
            examPaper.UpdatedBy = userId;
            examPaper.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build("Status updated successfully", 200);
        }
    }
}
