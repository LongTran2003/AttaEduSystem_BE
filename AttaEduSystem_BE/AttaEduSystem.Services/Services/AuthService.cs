using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.Authentication;
using AttaEduSystem.Models.DTOs.Email;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using AttaEduSystem.Utilities.Template.Email;

namespace AttaEduSystem.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService
        (
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ITokenService tokenService,
            IEmailService emailService
        )
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _emailService = emailService;
        }


        public async Task<ResponseDto> SignIn(SignInDto signInDto)
        {
            var user = await _userManager.FindByEmailAsync(signInDto.Email);
            
            // User không tồn tại -> Trả về 401 (Chuẩn bảo mật, không lộ email)
            if (user == null)
                return ErrorResponse.Build("Incorrect email or password", 401);

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, signInDto.Password);

            // Sai mật khẩu -> Trả về 401 (Thay vì 400 Bad Request)
            if (!isPasswordCorrect)
                return ErrorResponse.Build("Incorrect email or password", 401);
            
            // Check email chưa confirmed
            if (!user.EmailConfirmed)
                return ErrorResponse.Build("You need to confirm email!", 401);
            
            // Check user có bị ban acc
            if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.UtcNow)
                return ErrorResponse.Build("User has been locked", 403);

            // 1) Lấy plan code từ DB (mặc định FREE nếu không có subscription)
            var planCode = "FREE";
            var sub = await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(user.Id);
            if (sub?.Plan != null && !string.IsNullOrWhiteSpace(sub.Plan.Code))
            {
                planCode = sub.Plan.Code;
            }

            // 2) Chuẩn bị extra claims cho token
            var extraClaims = new List<Claim>{new Claim("plan", planCode)};

            // 3) Sinh token kèm extra claims
            var accessToken = await _tokenService.GenerateJwtAccessTokenAsync(user, extraClaims);
            var refreshToken = await _tokenService.GenerateJwtRefreshTokenAsync(user);
            await _tokenService.StoreRefreshToken(user.Id, refreshToken);

            return SuccessResponse.Build("Sign in successfully", 200, new SignInResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        public async Task<ResponseDto> SignUpStudent(SignUpStudentDto signUpStudentDto)
        {
            // Kiểm tra email đã tồn tại
            var isEmailExit = await _userManager.FindByEmailAsync(signUpStudentDto.Email);
            if (isEmailExit is not null)
                return new ResponseDto
                {
                    Message = "Email is being used by another user",
                    Result = null,
                    IsSuccess = false,
                    StatusCode = 400
                };

            // Kiểm tra số điện thoại đã tồn tại
            var isPhoneNumberExit = await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == signUpStudentDto.PhoneNumber);
            if (isPhoneNumberExit)
                return new ResponseDto
                {
                    Message = "Phone number is being used by another user",
                    Result = null,
                    IsSuccess = false,
                    StatusCode = 400
                };

            // Tạo đối tượng ApplicationUser mới
            var newUser = _mapper.Map<ApplicationUser>(signUpStudentDto);

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                // 1. Sinh OTP kích hoạt ngay lúc tạo
                var otp = GenerateOtp();
                newUser.OtpCode = otp;
                newUser.OtpExpiry = DateTime.UtcNow.AddMinutes(10); // Hết hạn sau 10 phút

                var createUserResult = await _userManager.CreateAsync(newUser, signUpStudentDto.Password);

                if (!createUserResult.Succeeded)
                {
                    return new ResponseDto
                    {
                        Message = "Create user failed",
                        IsSuccess = false,
                        StatusCode = 400,
                        Result = createUserResult.Errors
                    };
                }

                var studentCode = await _unitOfWork.Student.GetNextStudentCodeAsync();

                var student = new Student
                {
                    StudentId = Guid.NewGuid(),
                    UserId = newUser.Id,
                    StudentCode = studentCode // ✅ Gán StudentCode
                };

                var isRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.Student);

                if (!isRoleExist) await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.Student));

                // Thêm role "Student" cho người dùng
                var isRoleAdded = await _userManager.AddToRoleAsync(newUser, StaticUserRoles.Student);

                if (!isRoleAdded.Succeeded)
                    return new ResponseDto
                    {
                        Message = "Error adding role",
                        IsSuccess = false,
                        StatusCode = 500,
                        Result = null
                    };

                // Lưu thay đổi vào cơ sở dữ liệu
                await _unitOfWork.Student.AddAsync(student);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                
                try 
                {
                    await _emailService.SendAccountVerificationOtpAsync(
                        newUser.Email, 
                        otp, 
                        newUser.FullName
                    );
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần, nhưng không chặn return
                    Console.WriteLine("Error sending OTP email: " + ex.Message);
                }
                
                return new ResponseDto
                {
                    Message = "Sign up successfully. Please check email for verification",
                    IsSuccess = true,
                    StatusCode = 201,
                    Result = new
                    {
                        Email = newUser.Email,
                        FullName = newUser.FullName
                    }
                };
            }
        }

        public async Task<ResponseDto> SignUpTeacher(SignUpTeacherDto signUpTeacherDto)
        {
            // Kiểm tra email đã tồn tại
            var isEmailExit = await _userManager.FindByEmailAsync(signUpTeacherDto.Email);
            if (isEmailExit is not null)
                return new ResponseDto
                {
                    Message = "Email is being used by another user",
                    Result = null,
                    IsSuccess = false,
                    StatusCode = 400
                };

            // Kiểm tra số điện thoại đã tồn tại
            var isPhoneNumberExit = await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == signUpTeacherDto.PhoneNumber);
            if (isPhoneNumberExit)
                return new ResponseDto
                {
                    Message = "Phone number is being used by another user",
                    Result = null,
                    IsSuccess = false,
                    StatusCode = 400
                };

            // Tạo đối tượng ApplicationUser mới
            ApplicationUser newUser;
            newUser = _mapper.Map<ApplicationUser>(signUpTeacherDto);
            newUser.EmailConfirmed = true;

            // Bắt đầu transaction qua UnitOfWork
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                // Thêm người dùng mới vào database
                var createUserResult = await _userManager.CreateAsync(newUser, signUpTeacherDto.Password);

                // Kiểm tra lỗi khi tạo
                if (!createUserResult.Succeeded)
                    return new ResponseDto
                    {
                        Message = "Create user failed",
                        IsSuccess = false,
                        StatusCode = 400,
                        Result = null
                    };

                var teacherCode = await _unitOfWork.Teacher.GetNextTeacherCodeAsync();
                Teacher teacher = new()
                {
                    UserId = newUser.Id,
                    TeacherCode = teacherCode
                };

                var isRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.Teacher);

                if (!isRoleExist) await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.Teacher));

                // Thêm role "Customer" cho người dùng
                var isRoleAdded = await _userManager.AddToRoleAsync(newUser, StaticUserRoles.Teacher);

                if (!isRoleAdded.Succeeded)
                    return new ResponseDto
                    {
                        Message = "Error adding role",
                        IsSuccess = false,
                        StatusCode = 500,
                        Result = null
                    };

                // Lưu thay đổi vào cơ sở dữ liệu
                await _unitOfWork.Teacher.AddAsync(teacher);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                return new ResponseDto
                {
                    Message = "User created successfully",
                    IsSuccess = true,
                    StatusCode = 201,
                    Result = new
                    {
                        Email = newUser.Email,
                        FullName = newUser.FullName
                    }
                };
            }
        }

        public async Task<ResponseDto> VerifyEmail(VerifyEmailDto verifyEmailDto)
        {
            var user = await _userManager.FindByIdAsync(verifyEmailDto.UserId);

            if (user!.EmailConfirmed)
                return new ResponseDto
                {
                    Message = "Your email has been confirmed!",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = null
                };

            var decodedToken = Uri.UnescapeDataString(verifyEmailDto.Token);

            var confirmResult = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!confirmResult.Succeeded)
                return new ResponseDto
                {
                    Message = "Invalid token",
                    StatusCode = 400,
                    IsSuccess = false,
                    Result = null
                };

            return new ResponseDto
            {
                Message = "Confirm Email Successfully",
                IsSuccess = true,
                StatusCode = 200,
                Result = null
            };
        }

        public async Task<ResponseDto> VerifyOtp(VerifyOtpDto verifyOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(verifyOtpDto.Email);
            if (user == null)
            {
                return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 404);
            }

            // Nếu đã xác thực rồi thì báo OK luôn
            if (user.EmailConfirmed)
            {
                return SuccessResponse.Build("Account is already verified.", 200);
            }

            // Kiểm tra OTP
            if (user.OtpCode != verifyOtpDto.OtpCode || user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
            {
                return ErrorResponse.Build("Invalid or expired OTP code", 400);
            }

            // OTP Đúng -> Kích hoạt
            user.EmailConfirmed = true; 
            user.Status = "Active"; 
    
            // Xóa OTP
            user.OtpCode = null;
            user.OtpExpiry = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return ErrorResponse.Build("Failed to verify account", 500);
            }

            return SuccessResponse.Build("Account verified successfully.", 200);
        }

        public async Task<ResponseDto> VerifyResetOtp(VerifyOtpDto verifyOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(verifyOtpDto.Email);
            if (user == null)
                return ErrorResponse.Build("User not found", 404);

            // Kiểm tra OTP
            if (user.OtpCode != verifyOtpDto.OtpCode || user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
                return ErrorResponse.Build("Invalid or expired OTP code", 400);

            // ✅ OTP đúng → Sinh temporary token để reset password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return SuccessResponse.Build("OTP verified successfully. Use the token to reset password.", 200, new
            {
                ResetToken = resetToken,
                Email = user.Email
            });
        }

        public async Task<ResponseDto> SendVerifyEmail(EmailDto emailDto)
        {
            {
                // Tìm user theo email
                var user = await _userManager.FindByEmailAsync(emailDto.Email);
                if (user == null)
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404,
                        Result = null
                    };

                // Nếu email đã được xác nhận
                if (user.EmailConfirmed)
                    return new ResponseDto
                    {
                        IsSuccess = true,
                        Message = "Your email has already been confirmed",
                        StatusCode = 200,
                        Result = null
                    };

                // Sinh token xác nhận email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Xây dựng liên kết xác thực.
                // Lưu ý: thay đổi URL cho phù hợp với môi trường (local hay production)
                var verificationLink =
                    $"https://http://localhost:5432/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Gọi EmailService để gửi email xác thực sử dụng template VerificationEmailTemplate
                var emailSent = await _emailService.SendVerificationEmailAsync(user.Email!, verificationLink, user.FullName);

                if (emailSent)
                    return new ResponseDto
                    {
                        IsSuccess = true,
                        Message = "Verification email sent successfully.",
                        StatusCode = 200,
                        Result = null
                    };

                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to send verification email.",
                    StatusCode = 500,
                    Result = null
                };
            }
        }

        public async Task<ResponseDto> ForgotPassword(EmailDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            // Không tìm thấy email -> Trả về 404 (Thay vì 400)
            if (user == null)
                return ErrorResponse.Build("No account found matching the provided email.", 404);

            ////token reset password
            //var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //// build link reset password
            //// string resetLink = $"http://localhost:5173/reset-password?email={user.Email}&token={Uri.UnescapeDataString(token)}";
            //var resetLink =
            //    $"http://www.spicypox.com/reset-password?email={user.Email}&token={HttpUtility.UrlEncode(token)}";
            //var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email!, resetLink);


            // ✅ SỬA: Sinh OTP thay vì token link
            var otp = GenerateOtp();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10); // Hết hạn sau 10 phút
            await _userManager.UpdateAsync(user);

            // ✅ SỬA: Gửi OTP qua email
            var emailSent = await _emailService.SendPasswordResetOtpAsync(user.Email!, otp, user.FullName);

            if (emailSent) 
                return SuccessResponse.Build(
                    "Password reset OTP sent to your email. Please check your inbox.", 200);

            return ErrorResponse.Build("Failed to send password reset email.", 500);
        }

        public async Task<ResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            // Validate password match
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                return ErrorResponse.Build("New password and confirmation password do not match.", 400);

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return ErrorResponse.Build("User not found", 404);

            // ✅ Reset password với token từ VerifyResetOtp
            var result = await _userManager.ResetPasswordAsync(
                user, 
                resetPasswordDto.Token, 
                resetPasswordDto.NewPassword);

            if (!result.Succeeded)
                return ErrorResponse.Build(
                    "Reset password failed. Invalid token or password requirements not met.", 
                    400);

            // ✅ Xóa OTP sau khi reset thành công
            user.OtpCode = null;
            user.OtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return SuccessResponse.Build("Password has been reset successfully.", 200);
        }

        public async Task<ResponseDto> ChangePassword(ChangePasswordDto changePasswordDto, ClaimsPrincipal User)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Kiểm tra OTP
            if (user.OtpCode != changePasswordDto.OTPNumbers || user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "OTP is invalid or expired",
                    StatusCode = 400
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Your new password is the same with the old password. Please make a new one.",
                    StatusCode = 400,
                    Result = null
                };
            }

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Your comfirm password must be the same with the new password.",
                    StatusCode = 400,
                    Result = null
                };
            }

            // Xóa OTP sau khi dùng
            user.OtpCode = null;
            user.OtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return new ResponseDto
            {
                IsSuccess = true,
                Message = "Password changed successfully",
                StatusCode = 200
            };
        }

        public async Task<ResponseDto> SendOTP(EmailDto sendOTPDto)
        {
            var user = await _userManager.FindByEmailAsync(sendOTPDto.Email);
            if (user == null)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found",
                    StatusCode = 404,
                    Result = null
                };
            }
            /*// Sinh OTP ở AuthService
            var randomGenerator = RandomNumberGenerator.Create();
            byte[] data = new byte[4];
            randomGenerator.GetBytes(data);
            var randomNumber = BitConverter.ToUInt32(data, 0);*/

            // Sinh OTP mới
            var otp = GenerateOtp();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            var sendOTP = await _emailService.SendChangePasswordEmailAsync(user.Email!, otp);
            if (sendOTP)
                return new ResponseDto
                {
                    IsSuccess = true,
                    Message = "OTP sent successfully. Please check your mail.",
                    StatusCode = 200,
                    Result = null
                };

            return new ResponseDto
            {
                IsSuccess = false,
                Message = "Failed to send OTP.",
                StatusCode = 500,
                Result = null
            };
        }

        public async Task<ResponseDto> ResendOTP(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return ErrorResponse.Build("User not found", 404);

            if (user.EmailConfirmed) return ErrorResponse.Build("Account already verified", 400);

            // Sinh OTP mới
            var otp = GenerateOtp();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            // Gửi lại mail
            await _emailService.SendAccountVerificationOtpAsync(
                user.Email, 
                otp, 
                user.FullName
            );

            return SuccessResponse.Build("OTP resent successfully", 200);
        }

        /*public async Task<ResponseDto> FetchUserByToken(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return new ResponseDto
                {
                    Message = "Invalid user",
                    StatusCode = 400,
                    IsSuccess = false,
                    Result = null
                };

            // Lấy role từ UserManager
            var roles = await _userManager.GetRolesAsync(user);

            // Tạo GetUserDto từ claims
            var userDto = new GetUserDto
            {
                Id = user.Id,
                FullName = principal.FindFirst("FullName")!.Value,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                BirthDate = user.BirthDate,
                Address = principal.FindFirst("Address")?.Value,
                Gender = user.Gender,
                ImageUrl = principal.FindFirst("ImageUrl")?.Value,
                UserName = user.UserName!,
                Age = DateTime.UtcNow.Year - user.BirthDate.Year -
                (DateTime.UtcNow.Date < user.BirthDate.AddYears
                (DateTime.UtcNow.Year - user.BirthDate.Year) ? 1 : 0),
                Roles = roles.ToList()
            };

            return new ResponseDto
            {
                Message = "Get info successfully",
                StatusCode = 200,
                IsSuccess = true,
                Result = userDto
            };
        }*/

        /*public async Task<ResponseDto> UpdateUserProfile(ClaimsPrincipal userPrincipal,
        UpdateUserProfileDto updateUserProfileDto)
        {
            // Lấy thông tin người dùng từ token JWT
            var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new ResponseDto
                {
                    Message = "Unauthorized",
                    StatusCode = 401,
                    IsSuccess = false,
                    Result = null
                };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ResponseDto
                {
                    Message = "Invalid user",
                    StatusCode = 400,
                    IsSuccess = false,
                    Result = null
                };

            // Sử dụng mapping với overload có destination để cập nhật đối tượng user hiện có
            _mapper.Map(updateUserProfileDto, user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new ResponseDto
                {
                    Message = "Update user profile failed",
                    StatusCode = 400,
                    IsSuccess = false,
                    Result = result.Errors
                };

            // Nếu muốn trả về dữ liệu cập nhật, bạn có thể map lại đối tượng user sang DTO trả về
            var updatedUserDto = _mapper.Map<ApplicationUser, UpdateUserProfileDto>(user);
            return new ResponseDto
            {
                Message = "Update user profile successfully",
                StatusCode = 200,
                IsSuccess = true,
                Result = updatedUserDto
            };
        }*/

        /*public async Task<ResponseDto> RefreshAccessToken(RefreshTokenDto refreshTokenDto)
        {
            var principal = await _tokenService.GetPrincipalFromToken(refreshTokenDto.RefreshToken);
            if (principal is null)
                ErrorResponse.Build(
                    StaticOperationStatus.Token.TokenInvalid,
                    StaticOperationStatus.StatusCode.Unauthorized);

            var userId = principal!.FindFirstValue(ClaimTypes.NameIdentifier);
            var userFromDb = await _userManager.FindByIdAsync(userId);

            if (userFromDb is null)
                ErrorResponse.Build(
                    StaticOperationStatus.User.UserNotFound,
                    StaticOperationStatus.StatusCode.NotFound);

            var storedRefreshToken = await _tokenService.RetrieveRefreshTokenAsync(userId);

            if (storedRefreshToken != refreshTokenDto.RefreshToken)
                ErrorResponse.Build(
                    StaticOperationStatus.Token.TokenInvalid,
                    StaticOperationStatus.StatusCode.Unauthorized);
            // New access token creation
            var newAccessToken = await _tokenService.GenerateJwtAccessTokenAsync(userFromDb);

            return SuccessResponse.Build(
                StaticOperationStatus.Token.TokenRefreshed,
                StaticOperationStatus.StatusCode.Ok,
                newAccessToken);
        }*/
        
        private string GenerateOtp()
        {
            var randomGenerator = RandomNumberGenerator.Create();
            byte[] data = new byte[4];
            randomGenerator.GetBytes(data);
            var randomNumber = BitConverter.ToUInt32(data, 0);
            return (randomNumber % 1000000).ToString("D6");
        }
    }
}
