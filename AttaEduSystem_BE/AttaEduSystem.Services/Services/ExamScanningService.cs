using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.DTOs.GeminiAi;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;

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
        private readonly IAiAnalysisService _aiAnalysisService;
        private readonly IUsageTrackerService _usageTracker;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamScanningService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            ICloudinaryService cloudinaryService,
            IConfiguration configuration,
            ILogger<ExamScanningService> logger,
            IOcrService ocrService,
            IGeminiAiService geminiAiService,
            IUsageTrackerService usageTracker,
            UserManager<ApplicationUser> userManager,
            IAiAnalysisService aiAnalysisService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
            _logger = logger;
            _ocrService = ocrService;
            _geminiAiService = geminiAiService;
            _usageTracker = usageTracker;
            _userManager = userManager;
            _aiAnalysisService = aiAnalysisService;
        }
        public async Task<ResponseDto> ScanExamPaper(UploadExamPaperDto uploadDto, ClaimsPrincipal user)
        {
            try
            {
                // 1. Check user có đăng nhập/token
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return ErrorResponse.Build(
                        message: StaticOperationStatus.User.UserNotFound,
                        statusCode: StaticOperationStatus.StatusCode.Unauthorized);
                }

                // 2. Check quota hiện tại có đủ không
                if (!await _usageTracker.TryConsumeAsync(user, UsageType.Scan, 1))
                {
                    return ErrorResponse.Build(
                        message: "Your scan quota has been reached. Please upgrade your subscription to continue.",
                        statusCode: 402); // Payment Required
                }
                
                // 3. Check file có tồn tại 
                if (uploadDto.ExamImage == null || uploadDto.ExamImage.Length == 0)
                {
                    return ErrorResponse.Build(
                        message: StaticOperationStatus.File.FileEmpty,
                        statusCode: StaticOperationStatus.StatusCode.BadRequest);
                }
                
                // 4. Check ảnh có bị quá kích thước cho phép 
                var maxFileSize = _configuration.GetValue<long>("ExamPaperSettings:MaxFileSize", 10485760);
                if (uploadDto.ExamImage.Length > maxFileSize)
                {
                    return ErrorResponse.Build(
                        message: "File size exceeds maximum allowed size",
                        statusCode: StaticOperationStatus.StatusCode.BadRequest);
                }

                // 5. Convert ảnh sang base64
                string base64Image;
                await using (var ms = new MemoryStream())
                {
                    await uploadDto.ExamImage.CopyToAsync(ms);
                    base64Image = Convert.ToBase64String(ms.ToArray());
                }

                // 6. Upload Cloudinary với Hard-code Transformation
                var folderPath = StaticCloudinaryFolders.ExamPapers;
                string imageUrl;
                try
                {
                    // Cấu hình: Giữ nguyên tỷ lệ, chỉ thu nhỏ nếu quá to (>2000px), tối ưu dung lượng
                    var examTransform = new Transformation()
                        .Width(2000).Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto");
                    
                    // Truyền transformation vào hàm upload
                    imageUrl = await _cloudinaryService.UploadImageAsync(uploadDto.ExamImage, folderPath, examTransform);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading exam image");
                    return ErrorResponse.Build(
                        message: "Failed to upload exam image",
                        statusCode: StaticOperationStatus.StatusCode.InternalServerError);
                }

                // 7. OCR  
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

                // 8. Gọi AI Analysis với Fallback Strategy (Gemini -> OpenAI)

                string aiResponseJson;
                try
                {
                    _logger.LogInformation("Starting AI analysis for exam paper");
                    aiResponseJson = await _aiAnalysisService.AnalyzeExamStructureWithFallback(
                        base64Image,
                        uploadDto.ExamImage.ContentType ?? "image/jpeg");
                    _logger.LogInformation("AI analysis completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "All AI services failed to analyze exam structure");
                    return ErrorResponse.Build(
                        message: $"AI analysis failed: {ex.Message}. Please try again later or contact support.",
                        statusCode: StaticOperationStatus.StatusCode.InternalServerError);
                }
                //string aiResponseJson;
                //try
                //{
                //    aiResponseJson = await _geminiAiService.AnalyzeExamStructure(
                //        base64Image,
                //        uploadDto.ExamImage.ContentType ?? "image/jpeg");
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Error calling Gemini API");
                //    return ErrorResponse.Build("AI Service Unreachable: " + ex.Message, 500);
                //}

                // 9. Lưu ExamPaper (Header)
                var examPaper = _mapper.Map<ExamPaper>(uploadDto);
                examPaper.ExamPaperId = Guid.NewGuid();
                examPaper.Title = string.IsNullOrWhiteSpace(examPaper.Title)
                    ? $"Exam Paper - {StaticOperationStatus.Timezone.Vietnam:yyyy-MM-dd HH:mm}"
                    : examPaper.Title;
                examPaper.OriginalImageUrl = imageUrl;
                examPaper.ScannedText = aiResponseJson;
                examPaper.CreatedBy = user.FindFirstValue("FullName");
                examPaper.CreatedTime = StaticOperationStatus.Timezone.Vietnam;
                examPaper.Status = StaticOperationStatus.ExamPaper.Ready;
                
                var currentUser = await _userManager.FindByIdAsync(userId);
                examPaper.Creator = currentUser;
                
                await _unitOfWork.ExamPaper.AddAsync(examPaper);

                // 10. Logic Parse JSON và lưu vào bảng EXAM_QUESTION
                try
                {
                    var parsedData = JsonSerializer.Deserialize<ExamStructureResponse>
                        (aiResponseJson, 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (parsedData?.Questions != null)
                    {
                        // Tự động map List<QuestionItem> sang List<ExamQuestion> (AutoMapper)
                        var questionEntities = _mapper.Map<List<ExamQuestion>>(parsedData.Questions);

                        // Gán FK thủ công vì Mapper không biết ExamPaperId vừa tạo
                        foreach (var q in questionEntities)
                        {
                            q.ExamPaperId = examPaper.ExamPaperId;
                            // Gán OrderIndex
                            q.OrderIndex = questionEntities.IndexOf(q) + 1;
                            q.CreatedBy = user.FindFirstValue("FullName");
                            q.CreatedTime = StaticOperationStatus.Timezone.Vietnam;
                        }

                        await _unitOfWork.ExamQuestion.AddRangeAsync(questionEntities);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to parse structure to entities: " + ex.Message);
                }
                
                // 11. Lưu vào DB
                await _unitOfWork.SaveAsync();
                
                // 12. Response lại bằng ScanExamPaperResponseDto
                var responseDto = _mapper.Map<ScanExamPaperResponseDto>(examPaper);

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

        public async Task<ResponseDto> GetExamPaperById(Guid examPaperId)
        {
            var examPaper = await _unitOfWork.ExamPaper.GetByIdWithUserAsync(examPaperId);
            
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

        public async Task<ResponseDto> UpdateExamPaperStatus(Guid examPaperId, UpdateExamPaperStatusDto dto, ClaimsPrincipal user)
        {
            try
            {
                // 1. Get user ID
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);
                }

                // 2. Get exam paper with creator info
                var examPaper = await _unitOfWork.ExamPaper.GetByIdWithUserAsync(examPaperId);
                if (examPaper == null)
                {
                    return ErrorResponse.Build("Exam paper not found", 404);
                }

                // 3. Check ownership: only creator or admin can update
                var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var isOwner = examPaper.Creator?.Id == userId || examPaper.CreatedBy == userId;
                var isAdmin = userRoles.Contains("Admin") || userRoles.Contains("Administrator");

                if (!isOwner && !isAdmin)
                {
                    return ErrorResponse.Build("You do not have permission to update this exam paper", 403);
                }

                // 4. Update Status if provided
                if (!string.IsNullOrWhiteSpace(dto.Status))
                {
                    var allowedStatuses = new[] {
                        StaticOperationStatus.ExamPaper.Draft,
                        StaticOperationStatus.ExamPaper.Ready,
                        StaticOperationStatus.ExamPaper.Removed
                    };

                    if (!allowedStatuses.Contains(dto.Status))
                    {
                        return ErrorResponse.Build("Invalid status value. Must be Draft, Ready, or Removed.", 400);
                    }

                    examPaper.Status = dto.Status;
                }

                // 5. Update Title if provided
                if (!string.IsNullOrWhiteSpace(dto.Title))
                {
                    examPaper.Title = dto.Title;
                }

                // 6. Update Description if provided (allow empty to clear)
                if (!string.IsNullOrWhiteSpace(dto.Description))
                {
                    examPaper.Description = dto.Description;
                }

                // 7. Update Subject if provided (allow empty to clear)
                if (!string.IsNullOrWhiteSpace(dto.Subject))
                {
                    examPaper.Subject = dto.Subject;
                }

                // 8. Set audit fields
                examPaper.UpdatedBy = userId;
                examPaper.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                // 9. Save changes
                _unitOfWork.ExamPaper.Update(examPaper);
                await _unitOfWork.SaveAsync();

                // 10. Return updated DTO
                var responseDto = _mapper.Map<GetExamPaperDto>(examPaper);
                return SuccessResponse.Build("Exam paper updated successfully", 200, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam paper");
                return ErrorResponse.Build($"An error occurred while updating exam paper, {ex.Message}", 500);
            }
        }
    }
}
