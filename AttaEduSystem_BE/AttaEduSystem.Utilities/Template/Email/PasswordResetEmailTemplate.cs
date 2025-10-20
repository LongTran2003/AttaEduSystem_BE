using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.Utilities.Template.Email
{
    public class PasswordResetEmailTemplate : GenericEmailTemplate
    {
        public override string TemplateName { get; set; } = StaticEmailTemplates.PasswordReset;
        public override string Subject { get; set; } = "Reset your password";
        public override string BodyContent { get; set; } = "You have requested to reset your password. Click the link below to reset your password.";
        public override string CallToAction { get; set; } = "{{ResetPasswordLink}}";
        public override string CallToActionText { get; set; } = "Reset Password";
        public override string FooterContent { get; set; } = "If you did not request to reset your password, please ignore this email.";

        public PasswordResetEmailTemplate() { }
    }
}
