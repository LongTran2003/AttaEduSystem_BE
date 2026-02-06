using AttaEduSystem.Models.DTOs;
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
        [SwaggerOperation(Summary = "Create new folder", Description = "Create a new folder to organize exam papers.")]
        public async Task<ActionResult<ResponseDto>> CreateFolder([FromBody] CreateFolderDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _folderService.CreateFolder(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "List my folders", Description = "Get all active folders created by the current user.")]
        public async Task<ActionResult<ResponseDto>> GetMyFolders()
        {
            var result = await _folderService.GetMyFolders(User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{folderId:guid}")]
        [SwaggerOperation(Summary = "Get folder details", Description = "Get folder info and list of exam papers inside it.")]
        public async Task<ActionResult<ResponseDto>> GetFolderDetails(Guid folderId)
        {
            var result = await _folderService.GetFolderDetails(folderId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{folderId:guid}")]
        [SwaggerOperation(Summary = "Update folder", Description = "Rename folder or change color.")]
        public async Task<ActionResult<ResponseDto>> UpdateFolder(Guid folderId, [FromBody] UpdateFolderDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _folderService.UpdateFolder(folderId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{folderId:guid}")]
        [SwaggerOperation(Summary = "Delete folder (Soft Delete)", Description = "Mark folder as deleted. Exams inside will be detached (moved to root).")]
        public async Task<ActionResult<ResponseDto>> DeleteFolder(Guid folderId)
        {
            var result = await _folderService.DeleteFolder(folderId, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{folderId:guid}/exams")]
        [SwaggerOperation(Summary = "Add exam to folder", Description = "Move an existing exam paper into a specific folder.")]
        public async Task<ActionResult<ResponseDto>> AddExamToFolder(Guid folderId, [FromBody] AddExamToFolderDto dto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _folderService.AddExamToFolder(folderId, dto, User);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("exams/{examPaperId:guid}")]
        [SwaggerOperation(Summary = "Remove exam from folder", Description = "Remove an exam from its current folder (exam will be moved to 'Uncategorized').")]
        public async Task<ActionResult<ResponseDto>> RemoveExamFromFolder(Guid examPaperId)
        {
            var result = await _folderService.RemoveExamFromFolder(examPaperId, User);
            return StatusCode(result.StatusCode, result);
        }
}