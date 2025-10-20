namespace AttaEduSystem.Models.DTOs.Email
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string OTPNumbers { get; set; }
    }
}
