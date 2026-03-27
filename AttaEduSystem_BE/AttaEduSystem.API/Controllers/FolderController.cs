using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.DTOs.ExamQuestion;
using AttaEduSystem.Models.DTOs.ExamRoom.Room;
using AttaEduSystem.Models.DTOs.Folder;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers;

[ApiController]
[Route("api/folders")]
[Authorize]
[SwaggerTag("Library & Folder Management APIs")]

public class FolderController : ControllerBase
{
    private readonly IFolderService _folderService;

        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        // Helper validate input
        private ActionResult<ResponseDto> ReturnInvalidInputResponse()
        {
            return StatusCode(400, new ResponseDto
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Invalid input data.",
                Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        [HttpPost]
        [SwaggerOperation(Summary = "📂 Create new folder", Description = "Create a new folder to organize exam papers.")]
        public async Task<ActionResult<ResponseDto>> CreateFolder([FromBody] CreateFolderDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _folderService.CreateFolder(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "📂 List my folders", 
            Description = "Get all active folders created by the current user.")]
        public async Task<ActionResult<ResponseDto>> GetMyFolders()
        {
            var result = await _folderService.GetMyFolders(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{folderId:guid}")]
        [SwaggerOperation(Summary = "📁 Get folder details with exams",
            Description = "Retrieves folder metadata and list of exam papers. " +
        "Use includeQuestions=true to get full question details.")]
        public async Task<ActionResult<ResponseDto>> GetFolderDetails(
            Guid folderId,
            [FromQuery] bool includeQuestions = false)
        {
            var result = await _folderService.GetFolderDetails(folderId, User, includeQuestions);
            return StatusCode(result.StatusCode, result);
        }

    [HttpPut("{folderId:guid}")]
        [SwaggerOperation(Summary = "📂 Update folder", Description = "Rename folder or change color.")]
        public async Task<ActionResult<ResponseDto>> UpdateFolder(Guid folderId, [FromBody] UpdateFolderDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _folderService.UpdateFolder(folderId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{folderId:guid}")]
        [SwaggerOperation(Summary = "📂 Delete folder (Soft Delete)", Description = "Mark folder as deleted. Exams inside will be detached (moved to root).")]
        public async Task<ActionResult<ResponseDto>> DeleteFolder(Guid folderId)
        {
            var result = await _folderService.DeleteFolder(folderId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{folderId:guid}/exams")]
        [SwaggerOperation(Summary = "📂 Add exam to folder", Description = "Move an existing exam paper into a specific folder.")]
        public async Task<ActionResult<ResponseDto>> AddExamToFolder(Guid folderId, [FromBody] AddExamToFolderDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _folderService.AddExamToFolder(folderId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("exams/{examPaperId:guid}")]
        [SwaggerOperation(Summary = "📂 Remove exam from folder", Description = "Remove an exam from its current folder (exam will be moved to 'Uncategorized').")]
        public async Task<ActionResult<ResponseDto>> RemoveExamFromFolder(Guid examPaperId)
        {
            var result = await _folderService.RemoveExamFromFolder(examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{folderId:guid}/exams/{examPaperId:guid}")]
        [SwaggerOperation(Summary = "📄 Get exam details in folder", Description = "Get full exam details (including questions/options) for editing in folder view.")]
        public async Task<ActionResult<ResponseDto>> GetFolderExamDetails(Guid folderId, Guid examPaperId)
        {
            var result = await _folderService.GetFolderExamDetails(folderId, examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{folderId:guid}/exams/{examPaperId:guid}/questions")]
        [SwaggerOperation(Summary = "✏️ Batch update folder exam questions", Description = "Batch edit questions and options for an exam inside folder.")]
        public async Task<ActionResult<ResponseDto>> UpdateQuestionsInFolderExam(
            Guid folderId,
            Guid examPaperId,
            [FromBody] BatchUpdateQuestionsDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var result = await _folderService.UpdateQuestionsInFolderExam(folderId, examPaperId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{folderId:guid}/exams/{examPaperId:guid}/questions")]
        [SwaggerOperation(Summary = "➕ Add question to folder exam", Description = "Add a new question (with options) to an exam inside folder.")]
        public async Task<ActionResult<ResponseDto>> AddQuestionToFolderExam(
            Guid folderId,
            Guid examPaperId,
            [FromBody] AddExamQuestionDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var result = await _folderService.AddQuestionToFolderExam(folderId, examPaperId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{folderId:guid}/exams/{examPaperId:guid}/questions/{questionId:guid}")]
        [SwaggerOperation(Summary = "✏️ Update one question in folder exam", Description = "Update one question and its options in an exam inside folder.")]
        public async Task<ActionResult<ResponseDto>> UpdateQuestionInFolderExam(
            Guid folderId,
            Guid examPaperId,
            Guid questionId,
            [FromBody] UpdateExamQuestionDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var result = await _folderService.UpdateQuestionInFolderExam(folderId, examPaperId, questionId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{folderId:guid}/exams/{examPaperId:guid}/questions/{questionId:guid}")]
        [SwaggerOperation(Summary = "🗑️ Delete question in folder exam", Description = "Delete a question from an exam inside folder.")]
        public async Task<ActionResult<ResponseDto>> DeleteQuestionInFolderExam(Guid folderId, Guid examPaperId, Guid questionId)
        {
            var result = await _folderService.DeleteQuestionInFolderExam(folderId, examPaperId, questionId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{folderId:guid}/exams/{examPaperId:guid}/solve")]
        [Authorize]
        [SwaggerOperation(Summary = "💡 Solve exam in folder", Description = "Solve a folder exam with AI and save solution.")]
        public async Task<ActionResult<ResponseDto>> SolveExamInFolder(Guid folderId, Guid examPaperId)
        {
            var result = await _folderService.SolveExamInFolder(folderId, examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{folderId:guid}/exams/{examPaperId:guid}/solution")]
        [SwaggerOperation(Summary = "💡 Get solution of folder exam", Description = "Get saved AI solution for an exam inside folder.")]
        public async Task<ActionResult<ResponseDto>> GetExamSolutionInFolder(Guid folderId, Guid examPaperId)
        {
            var result = await _folderService.GetExamSolutionInFolder(folderId, examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{folderId:guid}/exams/{examPaperId:guid}/quiz-room")]
        [SwaggerOperation(Summary = "🎯 Create quiz room from folder exam", Description = "Create an exam room directly from an exam inside folder.")]
        public async Task<ActionResult<ResponseDto>> CreateQuizRoomFromFolderExam(
            Guid folderId,
            Guid examPaperId,
            [FromBody] CreateExamRoomDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();
            var result = await _folderService.CreateQuizRoomFromFolderExam(folderId, examPaperId, dto, User);
            return StatusCode(result.StatusCode, result);
        }
}
