using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.LearningClass;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers;

[ApiController]
[Route("api/learning-classes")]
[Authorize]
[SwaggerTag("Learning Class Management APIs")]
public class LearningClassesController : ControllerBase
{
    private readonly ILearningClassService _learningClassService;

    public LearningClassesController(ILearningClassService learningClassService)
    {
        _learningClassService = learningClassService;
    }

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
    [Authorize(Roles = "TEACHER, ADMIN")]
    [SwaggerOperation(Summary = "🏫 Create learning class")]
    public async Task<ActionResult<ResponseDto>> CreateClass([FromBody] CreateLearningClassDto dto)
    {
        if (!ModelState.IsValid) return ReturnInvalidInputResponse();
        var result = await _learningClassService.CreateClass(dto, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my-classes")]
    [Authorize(Roles = "TEACHER, ADMIN, STUDENT")]
    [SwaggerOperation(Summary = "🏫 Get my classes")]
    public async Task<ActionResult<ResponseDto>> GetMyClasses([FromQuery] string? keyword, [FromQuery] string? status)
    {
        var result = await _learningClassService.GetMyClasses(User, keyword, status);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{classId:guid}")]
    [Authorize(Roles = "TEACHER, ADMIN, STUDENT")]
    [SwaggerOperation(Summary = "🏫 Get class detail")]
    public async Task<ActionResult<ResponseDto>> GetClassDetail(Guid classId)
    {
        var result = await _learningClassService.GetClassDetail(classId, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{classId:guid}")]
    [Authorize(Roles = "TEACHER, ADMIN")]
    [SwaggerOperation(Summary = "🏫 Update class info")]
    public async Task<ActionResult<ResponseDto>> UpdateClass(Guid classId, [FromBody] UpdateLearningClassDto dto)
    {
        if (!ModelState.IsValid) return ReturnInvalidInputResponse();
        var result = await _learningClassService.UpdateClass(classId, dto, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{classId:guid}")]
    [Authorize(Roles = "TEACHER, ADMIN")]
    [SwaggerOperation(Summary = "🏫 Delete class")]
    public async Task<ActionResult<ResponseDto>> DeleteClass(Guid classId)
    {
        var result = await _learningClassService.DeleteClass(classId, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{classId:guid}/members")]
    [Authorize(Roles = "TEACHER, ADMIN")]
    [SwaggerOperation(Summary = "🏫 Add class members")]
    public async Task<ActionResult<ResponseDto>> AddMembers(Guid classId, [FromBody] AddLearningClassMembersDto dto)
    {
        if (!ModelState.IsValid) return ReturnInvalidInputResponse();
        var result = await _learningClassService.AddMembers(classId, dto, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{classId:guid}/members/{memberUserId}")]
    [Authorize(Roles = "TEACHER, ADMIN")]
    [SwaggerOperation(Summary = "🏫 Remove class member")]
    public async Task<ActionResult<ResponseDto>> RemoveMember(Guid classId, string memberUserId)
    {
        var result = await _learningClassService.RemoveMember(classId, memberUserId, User);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("admin/overview")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(Summary = "🏫 Admin class overview")]
    public async Task<ActionResult<ResponseDto>> GetAdminClassOverview()
    {
        var result = await _learningClassService.GetAdminClassOverview();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("admin/dashboard")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(Summary = "🏫 Admin class dashboard analytics")]
    public async Task<ActionResult<ResponseDto>> GetAdminClassDashboard(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? classId,
        [FromQuery] string? subject)
    {
        var result = await _learningClassService.GetAdminClassDashboard(month, year, classId, subject);
        return StatusCode(result.StatusCode, result);
    }
}
