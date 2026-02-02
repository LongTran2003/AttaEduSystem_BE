using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Authentication;
using AttaEduSystem.Models.DTOs.Email;
using AttaEduSystem.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AttaEduSystem.API.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    [SwaggerTag("Authentication and Account Management APIs")]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        // Helper validate
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
        
        [HttpPost("register/student")]
        [SwaggerOperation(Summary = "Register a new student account", Description = "Creates a new student account. Requires Guest or Teacher role.")]
        public async Task<ActionResult<ResponseDto>> SignUpStudent([FromBody] SignUpStudentDto signUpStudentDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse(); // Hàm helper đã viết ở phần trước hoặc dùng BadRequest chuẩn

            var result = await _authService.SignUpStudent(signUpStudentDto);
    
            // SỬA: Luôn dùng StatusCode lấy từ Service (201, 400, 500...)
            // Thay vì CreatedAtAction hoặc BadRequest
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("register/teacher")]
        [SwaggerOperation(Summary = "Register a new teacher account", Description = "Creates a new teacher account. Requires Admin role.")]
        public async Task<ActionResult<ResponseDto>> SignUpTeacher([FromBody] SignUpTeacherDto signUpTeacherDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _authService.SignUpTeacher(signUpTeacherDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("account/sign-in")]
        [SwaggerOperation(Summary = "Sign in user", Description = "Authenticates a user by email and password. Requires verified email.")]
        public async Task<ActionResult<ResponseDto>> SignIn([FromBody] SignInDto signInDto)
        {
            var responseDto = await _authService.SignIn(signInDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("verification/email")]
        [SwaggerOperation(Summary = "Send verification email", Description = "Sends a verification email to a registered email address.")]
        public async Task<ActionResult<ResponseDto>> SendVerifyEmail([FromBody] EmailDto emailDto)
        {
            var responseDto = await _authService.SendVerifyEmail(emailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }
        
        [HttpPost("verification/otp")]
        [SwaggerOperation(Summary = "Verify new account with OTP", Description = "Activates the newly registered student account using the OTP sent via email.")]
        public async Task<IActionResult> SendVerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var result = await _authService.VerifyOtp(verifyOtpDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("verification/confirm")]
        [SwaggerOperation(Summary = "Confirm email verification", Description = "Confirms a user's email verification code.")]
        public async Task<ActionResult<ResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var responseDto = await _authService.VerifyEmail(verifyEmailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }
        
        [HttpPost("verification/resend-otp")]
        [SwaggerOperation(Summary = "Resend verification OTP", Description = "Resends a new OTP if the previous one expired.")]
        public async Task<IActionResult> ResendAccountOTP([FromBody] EmailDto emailDto)
        {
            var result = await _authService.ResendOTP(emailDto.Email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("password/forgot")]
        [SwaggerOperation(Summary = "Send forgot password email",
        Description = "Sends a password reset email for an existing account.")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailDto forgotPasswordDto)
        {
            var responseDto = await _authService.ForgotPassword(forgotPasswordDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/reset")]
        [SwaggerOperation(Summary = "Reset password",
            Description = "Resets password for an existing account.")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var responseDto = await _authService.ResetPassword(resetPasswordDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/otp")]
        [SwaggerOperation(Summary = "Send OTP for password change",
            Description = "Sends a one-time password (OTP) to change account password.")]
        public async Task<IActionResult> SendOTP([FromBody] EmailDto emailDto)
        {
            var responseDto = await _authService.SendOTP(emailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/change")]
        [SwaggerOperation(Summary = "Change password",
            Description = "Changes password for the currently logged-in user.")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var responseDto = await _authService.ChangePassword(changePasswordDto, User);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        /*[HttpGet("user")]
        [SwaggerOperation(Summary = "Get user info",
            Description = "Fetches user info from JWT token.")]
        public async Task<IActionResult> GetUserByToken()
        {
            var responseDto = await _authService.FetchUserByToken(User);
            return StatusCode(responseDto.StatusCode, responseDto);
        }*/

        /*[HttpPut("profile")]
        [SwaggerOperation(Summary = "Update user profile",
       Description = "Updates the logged-in user's profile.")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            var responseDto = await _authService.UpdateUserProfile(User, updateUserProfileDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }*/

        /*[HttpPost("token/refresh")]
        [SwaggerOperation(Summary = "Refresh access token",
        Description = "Refreshes access token using refresh token.")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var responseDto = await _authService.RefreshAccessToken(refreshTokenDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }*/
        
        
    }
}
