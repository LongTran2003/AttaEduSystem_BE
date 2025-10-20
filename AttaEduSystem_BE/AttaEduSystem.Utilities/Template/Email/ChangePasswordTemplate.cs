using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.Utilities.Template.Email
{
    public class ChangePasswordTemplate : GenericEmailTemplate
    {
        public override string TemplateName { get; set; } = StaticEmailTemplates.ChangePassword;
        public override string Subject { get; set; } = "OTP change password";
        public override string BodyContent { get; set; } = "Your OTP is: <span style='color: red; font-weight: bold; font-size: 24px;'>{{OTPnumbers}}</span>";
        public override string? CallToAction { get; set; } = "";
        public override string? CallToActionText { get; set; } = "";
        public override string FooterContent { get; set; } = "This OTP will expire in 3 minutes. QUICK!!!!";


        public ChangePasswordTemplate() { }
    }
}
