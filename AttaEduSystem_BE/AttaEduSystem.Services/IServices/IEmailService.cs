namespace AttaEduSystem.Services.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendVerificationEmailAsync(string toEmail, string emailConfirmationLink, string fullName);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetPasswordLink);
        Task<bool> SendChangePasswordEmailAsync(string toEmail, string changePasswordDto);
    }
}
