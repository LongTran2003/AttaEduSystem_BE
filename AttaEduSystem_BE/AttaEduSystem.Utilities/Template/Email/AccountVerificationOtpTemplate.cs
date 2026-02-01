namespace AttaEduSystem.Utilities.Template.Email;

public class AccountVerificationOtpTemplate : GenericEmailTemplate
{
    public override string TemplateName { get; set; } = "AccountVerificationOtp";
    public override string Subject { get; set; } = "Verify your account - AttaEdu";
    // Nội dung chứa OTP
    public override string BodyContent { get; set; } = "Welcome to AttaEdu! Your verification code is: <span style='color: #c17037; font-weight: bold; font-size: 24px;'>{{OTPnumbers}}</span>";
        
    public override string? CallToAction { get; set; } = ""; // Không cần nút bấm link
    public override string? CallToActionText { get; set; } = "";
    public override string FooterContent { get; set; } = "Please enter this code in the app to activate your account. This code expires in 10 minutes.";

    public AccountVerificationOtpTemplate() { }
}