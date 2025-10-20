using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Authentication;
using AttaEduSystem.Models.DTOs.Email;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/auth")]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("students-register")]
        [SwaggerOperation(Summary = "API creates a new student account", Description = "")]
        [ProducesResponseType(typeof(ResponseDto), 201)]
        [ProducesResponseType(typeof(ResponseDto), 400)]
        public async Task<ActionResult<ResponseDto>> SignUpStudent([FromBody] SignUpStudentDto signUpStudentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid input data.",
                    Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var result = await _authService.SignUpStudent(signUpStudentDto);
            return result.IsSuccess
                ? CreatedAtAction(nameof(SignUpStudent), result.Result, result)
                : BadRequest(result);
        }

        [HttpPost("teacher-register")]
        [SwaggerOperation(Summary = "API creates a new teacher account", Description = "")]
        public async Task<ActionResult<ResponseDto>> SignUpTeacher([FromBody] SignUpTeacherDto signUpTeacherDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid input data.",
                    Result = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var result = await _authService.SignUpTeacher(signUpTeacherDto);
            return result.IsSuccess
                ? CreatedAtAction(nameof(SignUpTeacher), result.Result, result)
                : BadRequest(result);
        }

        [HttpPost("sign-in")]
        [SwaggerOperation(Summary = "API for user sign-in", Description = "")]
        public async Task<ActionResult<ResponseDto>> SignIn([FromBody] SignInDto signInDto)
        {
            var responseDto = await _authService.SignIn(signInDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("email/verification/send")]
        [SwaggerOperation(Summary = "API to send verification email", Description = "")]
        public async Task<ActionResult<ResponseDto>> SendVerifyEmail([FromBody] EmailDto emailDto)
        {
            var responseDto = await _authService.SendVerifyEmail(emailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("email/verification/confirm")]
        [SwaggerOperation(Summary = "API to verify email", Description = "")]
        public async Task<ActionResult<ResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var responseDto = await _authService.VerifyEmail(verifyEmailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/forgot")]
        [SwaggerOperation(Summary = "API sends forgot available account's pasword email",
        Description = "Requires customer's, staff's  account")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailDto forgotPasswordDto)
        {
            var responseDto = await _authService.ForgotPassword(forgotPasswordDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/reset")]
        [SwaggerOperation(Summary = "API resets available account's pasword",
            Description = "Requires customer's, staff's  account")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var responseDto = await _authService.ResetPassword(resetPasswordDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/otp")]
        [SwaggerOperation(Summary = "API sends OPT code to change account's pasword",
            Description = "Requires customer's, staff's  account")]
        public async Task<IActionResult> SendOTP([FromBody] EmailDto emailDto)
        {
            var responseDto = await _authService.SendOTP(emailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/change")]
        [SwaggerOperation(Summary = "API changes available account's pasword",
            Description = "Requires customer's account")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var responseDto = await _authService.ChangePassword(changePasswordDto, User);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpGet("user")]
        [SwaggerOperation(Summary = "API gets user info by user's token",
            Description = "Requires customer's, staff's token")]
        public async Task<IActionResult> GetUserByToken()
        {
            var responseDto = await _authService.FetchUserByToken(User);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPut("profile")]
        [SwaggerOperation(Summary = "API updates user profile",
        Description = "Requires customer's, staff's  account")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            var responseDto = await _authService.UpdateUserProfile(User, updateUserProfileDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("refresh-token")]
        [SwaggerOperation(Summary = "API refreshes access token",
        Description = "Requires customer's or staff's refresh token")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var responseDto = await _authService.RefreshAccessToken(refreshTokenDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }
    }
}
