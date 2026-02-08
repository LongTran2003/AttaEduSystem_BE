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

        // ========================================
        // 1. REGISTRATION FLOW
        // ========================================

        [HttpPost("register/student")]
        [SwaggerOperation(Summary = "✨ Register a new student account",
            Description = "Creates a new student account and sends OTP verification email.")]
        public async Task<ActionResult<ResponseDto>> SignUpStudent([FromBody] SignUpStudentDto signUpStudentDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _authService.SignUpStudent(signUpStudentDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("register/teacher")]
        [SwaggerOperation(Summary = "✨ Register a new teacher account",
            Description = "Creates a new teacher account. Requires Admin role.")]
        public async Task<ActionResult<ResponseDto>> SignUpTeacher([FromBody] SignUpTeacherDto signUpTeacherDto)
        {
            if (!ModelState.IsValid) return ReturnInvalidInputResponse();

            var result = await _authService.SignUpTeacher(signUpTeacherDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("verification/otp")]
        [SwaggerOperation(Summary = "✨ Verify new account with OTP",
            Description = "Activates the newly registered student account using the OTP sent via email.")]
        public async Task<IActionResult> SendVerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var result = await _authService.VerifyOtp(verifyOtpDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("verification/resend-otp")]
        [SwaggerOperation(Summary = "✨ Resend verification OTP",
            Description = "Resends a new OTP if the previous one expired.")]
        public async Task<IActionResult> ResendAccountOTP([FromBody] EmailDto emailDto)
        {
            var result = await _authService.ResendOTP(emailDto.Email);
            return StatusCode(result.StatusCode, result);
        }

        // ========================================
        // 2. LOGIN FLOW
        // ========================================

        [HttpPost("login")]
        [SwaggerOperation(Summary = "🔑 Sign in user",
            Description = "Authenticates a user by email and password. Requires verified email.")]
        public async Task<ActionResult<ResponseDto>> SignIn([FromBody] SignInDto signInDto)
        {
            var responseDto = await _authService.SignIn(signInDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        // ========================================
        // 3. FORGOT PASSWORD FLOW (OTP-based)
        // ========================================

        [HttpPost("password/forgot")]
        [SwaggerOperation(Summary = "🆘 Send forgot password OTP",
            Description = "Sends a 6-digit OTP to the registered email for password reset.")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailDto forgotPasswordDto)
        {
            var responseDto = await _authService.ForgotPassword(forgotPasswordDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/verify-reset-otp")]
        [SwaggerOperation(Summary = "🆘 Verify password reset OTP",
            Description = "Verifies the OTP sent for password reset and returns a reset token.")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            var result = await _authService.VerifyResetOtp(verifyOtpDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("password/reset")]
        [SwaggerOperation(Summary = "🆘 Reset password (New Password)",
            Description = "Resets password using the token from verify-reset-otp.")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var responseDto = await _authService.ResetPassword(resetPasswordDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        // ========================================
        // 4. CHANGE PASSWORD FLOW (Logged-in user)
        // ========================================

        [HttpPost("password/send-otp")]
        [SwaggerOperation(Summary = "🔐 Send OTP for password change for logged-in user",
            Description = "Sends a one-time password (OTP) to change account password (for logged-in users).")]
        public async Task<IActionResult> SendOTP([FromBody] EmailDto emailDto)
        {
            var responseDto = await _authService.SendOTP(emailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("password/change")]
        [SwaggerOperation(Summary = " 🔐 Change password for logged-in user",
            Description = "Changes password for the currently logged-in user using OTP verification.")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var responseDto = await _authService.ChangePassword(changePasswordDto, User);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        // ========================================
        // LEGACY (Hidden from Swagger)
        // ========================================

        [HttpPost("verification/email")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SwaggerOperation(Summary = "⚠️ Send verification email",
            Description = "Sends a verification email to a registered email address.")]
        public async Task<ActionResult<ResponseDto>> SendVerifyEmail([FromBody] EmailDto emailDto)
        {
            var responseDto = await _authService.SendVerifyEmail(emailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }

        [HttpPost("verification/confirm")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SwaggerOperation(Summary = "⚠️ Confirm email verification",
            Description = "Confirms a user's email verification code.")]
        public async Task<ActionResult<ResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var responseDto = await _authService.VerifyEmail(verifyEmailDto);
            return StatusCode(responseDto.StatusCode, responseDto);
        }
    }
}
