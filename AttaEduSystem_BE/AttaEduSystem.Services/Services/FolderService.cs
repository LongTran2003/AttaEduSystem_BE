using System.Security.Claims;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
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

    public FolderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

    public async Task<ResponseDto> GetFolderDetails(Guid folderId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            
        // Lấy folder và Include danh sách đề thi
        // Lưu ý: UnitOfWork Generic thường hỗ trợ include string
        var folder = await _unitOfWork.ExamFolder.GetAsync(
            filter: f => f.FolderId == folderId && f.UserId == userId,
            includeProperties: "ExamPapers"
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
}