using System.Security.Claims;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Models.DTOs.ExamRoom.Room;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.DTOs.Folder;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;

namespace AttaEduSystem.Services.Services;

public class FolderService : IFolderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IExamQuestionService _examQuestionService;
    private readonly IExamSolvingService _examSolvingService;
    private readonly IExamRoomService _examRoomService;

    public FolderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IExamQuestionService examQuestionService,
        IExamSolvingService examSolvingService,
        IExamRoomService examRoomService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _examQuestionService = examQuestionService;
        _examSolvingService = examSolvingService;
        _examRoomService = examRoomService;
    }

    public async Task<ResponseDto> CreateFolder(CreateFolderDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        var folder = _mapper.Map<ExamFolder>(dto);
        folder.FolderId = Guid.NewGuid();
        folder.UserId = userId;
        folder.CreatedBy = user.FindFirstValue("FullName");
        folder.CreatedTime = StaticOperationStatus.Timezone.Vietnam;

        // Mặc định Entity chưa có DbSet riêng trong UnitOfWork mẫu, 
        // nhưng ta có thể dùng Generic Repository hoặc thêm vào UnitOfWork.
        // Ở đây giả sử bạn ĐÃ THÊM ExamFolder vào UnitOfWork (Xem lưu ý bên dưới)
        await _unitOfWork.ExamFolder.AddAsync(folder);
        await _unitOfWork.SaveAsync();

        return SuccessResponse.Build("Folder created", 201, _mapper.Map<FolderDto>(folder));
    }

    public async Task<ResponseDto> UpdateFolder(Guid folderId, UpdateFolderDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var folder = await _unitOfWork.ExamFolder.GetAsync(f => f.FolderId == folderId && f.UserId == userId);
            
        if (folder == null) return ErrorResponse.Build("Folder not found", 404);

        if (!string.IsNullOrEmpty(dto.Name)) folder.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.ColorCode)) folder.ColorCode = dto.ColorCode;

        await _unitOfWork.SaveAsync();
        return SuccessResponse.Build("Folder updated", 200, _mapper.Map<FolderDto>(folder));
    }

    public async Task<ResponseDto> DeleteFolder(Guid folderId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var folder = await _unitOfWork.ExamFolder.GetAsync(f => f.FolderId == folderId && f.UserId == userId);
            
        if (folder == null) return ErrorResponse.Build("Folder not found", 404);

        _unitOfWork.ExamFolder.Remove(folder);
        await _unitOfWork.SaveAsync();
            
        return SuccessResponse.Build("Folder deleted successfully", 200);
    }

    public async Task<ResponseDto> GetMyFolders(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        // Cần Include ExamPapers để đếm số lượng
        var folders = await _unitOfWork.ExamFolder.GetAllAsync(
            filter: f => f.UserId == userId && f.Status != "Deleted",
            includeProperties: "ExamPapers"); 

        return SuccessResponse.Build("Folders retrieved", 200, _mapper.Map<List<FolderDto>>(folders));
    }

    public async Task<ResponseDto> GetFolderDetails(Guid folderId, ClaimsPrincipal user, bool includeQuestions = false)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lấy folder và Include danh sách đề thi
        // Lưu ý: UnitOfWork Generic thường hỗ trợ include string
        var includeProps = includeQuestions
        ? "ExamPapers.Questions.Options"
        : "ExamPapers";

        var folder = await _unitOfWork.ExamFolder.GetAsync(
            filter: f => f.FolderId == folderId && f.UserId == userId,
            includeProperties: includeProps
        );

        if (folder == null) return ErrorResponse.Build("Folder not found", 404);

        // Map list ExamPaper sang DTO
        // Lưu ý: ExamPapers trong folder chỉ là list basic, nếu muốn full info (Questions) thì query sâu hơn
        var examDtos = _mapper.Map<List<GetExamPaperDto>>(folder.ExamPapers);

        return SuccessResponse.Build("Folder details retrieved", 200, new 
        {
            Folder = _mapper.Map<FolderDto>(folder),
            Exams = examDtos
        });
    }

    public async Task<ResponseDto> AddExamToFolder(Guid folderId, AddExamToFolderDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            
        // 1. Check folder có tồn tại và thuộc về user không
        var folder = await _unitOfWork.ExamFolder.GetAsync(f => f.FolderId == folderId && f.UserId == userId);
        if (folder == null) return ErrorResponse.Build("Folder not found", 404);

        // 2. Check đề thi có tồn tại không
        var exam = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == dto.ExamPaperId);
        if (exam == null) return ErrorResponse.Build("Exam paper not found", 404);

        // 3. Gán folder
        exam.FolderId = folderId;
        // exam.Folder = folder; // Không cần gán object, gán ID là đủ
            
        await _unitOfWork.SaveAsync();

        return SuccessResponse.Build("Exam added to folder", 200);
    }

    public async Task<ResponseDto> RemoveExamFromFolder(Guid examPaperId, ClaimsPrincipal user)
    {
        // Logic: Set FolderId = null
        var exam = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == examPaperId);
        if (exam == null) return ErrorResponse.Build("Exam not found", 404);
             
        // Check quyền sở hữu đề thi (Optional)
        // if (exam.CreatedBy != userId) return 403...

        exam.FolderId = null;
        await _unitOfWork.SaveAsync();

        return SuccessResponse.Build("Exam removed from folder", 200);
    }

    public async Task<ResponseDto> GetFolderExamDetails(Guid folderId, Guid examPaperId, ClaimsPrincipal user)
    {
        var (folder, exam, error) = await ValidateFolderExamAccess(folderId, examPaperId, user);
        if (error != null) return error;

        var dto = _mapper.Map<GetExamPaperDto>(exam);
        var solution = await _unitOfWork.ExamSolution.GetAsync(
            s => s.ExamPaperId == examPaperId && s.Status != "Deleted");
        dto.HasSolution = solution != null;
        dto.SolutionId = solution?.ExamSolutionId;

        return SuccessResponse.Build("Folder exam details retrieved", 200, new
        {
            Folder = _mapper.Map<FolderDto>(folder),
            Exam = dto
        });
    }

    public async Task<ResponseDto> AddQuestionToFolderExam(Guid folderId, Guid examPaperId, AddExamQuestionDto dto, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;
        return await _examQuestionService.AddQuestionToExamPaper(examPaperId, dto, user);
    }

    public async Task<ResponseDto> UpdateQuestionInFolderExam(Guid folderId, Guid examPaperId, Guid questionId, UpdateExamQuestionDto dto, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;

        var question = await _unitOfWork.ExamQuestion.GetAsync(q => q.QuestionId == questionId);
        if (question == null || question.ExamPaperId != examPaperId)
            return ErrorResponse.Build("Question not found in this folder exam", 404);

        return await _examQuestionService.UpdateQuestion(questionId, dto, user);
    }

    public async Task<ResponseDto> DeleteQuestionInFolderExam(Guid folderId, Guid examPaperId, Guid questionId, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;

        var question = await _unitOfWork.ExamQuestion.GetAsync(q => q.QuestionId == questionId);
        if (question == null || question.ExamPaperId != examPaperId)
            return ErrorResponse.Build("Question not found in this folder exam", 404);

        return await _examQuestionService.DeleteQuestion(questionId, user);
    }

    public async Task<ResponseDto> UpdateQuestionsInFolderExam(Guid folderId, Guid examPaperId, BatchUpdateQuestionsDto dto, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;

        var questionIds = dto.Questions.Select(q => q.QuestionId).ToHashSet();
        var countInExam = (await _unitOfWork.ExamQuestion.GetAllAsync(
            q => q.ExamPaperId == examPaperId && questionIds.Contains(q.QuestionId))).Count();

        if (countInExam != questionIds.Count)
            return ErrorResponse.Build("One or more questions do not belong to this folder exam", 400);

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

        var existingOptions = await _unitOfWork.QuestionOption.GetAllAsync(o => questionIds.Contains(o.QuestionId));
        var optionMap = existingOptions
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var questions = await _unitOfWork.ExamQuestion.GetAllAsync(
            q => q.ExamPaperId == examPaperId && questionIds.Contains(q.QuestionId));
        var questionMap = questions.ToDictionary(q => q.QuestionId);
        var updated = 0;

        foreach (var item in dto.Questions)
        {
            var question = questionMap[item.QuestionId];
            if (item.Content != null) question.Content = item.Content;
            if (item.QuestionIdLabel != null) question.QuestionIdLabel = item.QuestionIdLabel;
            if (item.Points.HasValue) question.Points = item.Points;
            if (item.CorrectAnswer != null) question.CorrectAnswer = item.CorrectAnswer;
            if (item.QuestionType != null) question.QuestionType = item.QuestionType;

            if (item.Options != null)
            {
                if (optionMap.TryGetValue(question.QuestionId, out var optionsToRemove) && optionsToRemove.Any())
                {
                    _unitOfWork.QuestionOption.RemoveRange(optionsToRemove);
                }

                if (item.Options.Any())
                {
                    var newOptions = item.Options.Select(o => new QuestionOption
                    {
                        OptionId = Guid.NewGuid(),
                        QuestionId = question.QuestionId,
                        Label = o.OptionLabel,
                        Content = o.OptionContent
                    }).ToList();
                    await _unitOfWork.QuestionOption.AddRangeAsync(newOptions);
                }
            }

            question.UpdatedBy = user.FindFirstValue("FullName") ?? userId;
            question.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
            _unitOfWork.ExamQuestion.Update(question);
            updated++;
        }

        await _unitOfWork.SaveAsync();
        return SuccessResponse.Build("Folder exam questions updated successfully", 200, new { UpdatedCount = updated });
    }

    public async Task<ResponseDto> SolveExamInFolder(Guid folderId, Guid examPaperId, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;

        return await _examSolvingService.SolveExamPaper(examPaperId, user);
    }

    public async Task<ResponseDto> GetExamSolutionInFolder(Guid folderId, Guid examPaperId, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;

        return await _examSolvingService.GetSolutionByExamId(examPaperId);
    }

    public async Task<ResponseDto> CreateQuizRoomFromFolderExam(Guid folderId, Guid examPaperId, CreateExamRoomDto dto, ClaimsPrincipal user)
    {
        var (_, _, error) = await ValidateFolderExamAccess(folderId, examPaperId, user, false);
        if (error != null) return error;

        dto.ExamPaperId = examPaperId;
        return await _examRoomService.CreateRoom(dto, user);
    }

    private async Task<(ExamFolder? Folder, ExamPaper? ExamPaper, ResponseDto? Error)> ValidateFolderExamAccess(
        Guid folderId,
        Guid examPaperId,
        ClaimsPrincipal user,
        bool includeQuestionData = true)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return (null, null, ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401));

        var folder = await _unitOfWork.ExamFolder.GetAsync(f => f.FolderId == folderId && f.UserId == userId);
        if (folder == null)
            return (null, null, ErrorResponse.Build("Folder not found", 404));

        var includeProps = includeQuestionData ? "Questions.Options,Creator" : "Creator";
        var exam = await _unitOfWork.ExamPaper.GetAsync(
            e => e.ExamPaperId == examPaperId && e.FolderId == folderId,
            includeProperties: includeProps);

        if (exam == null)
            return (folder, null, ErrorResponse.Build("Exam paper not found in this folder", 404));

        return (folder, exam, null);
    }
}
