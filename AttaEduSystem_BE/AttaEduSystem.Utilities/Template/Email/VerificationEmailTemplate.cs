using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.Utilities.Template.Email
{
    public class VerificationEmailTemplate : GenericEmailTemplate
    {
        public override string TemplateName { get; set; } = StaticEmailTemplates.Verification;
        public override string Subject { get; set; } = "Welcome to Movok!";
        public override string Category { get; set; } = "Verification";
        public override string PreHeaderText { get; set; } = "Thank you for signing up!";
        public override string BodyContent { get; set; } = "Thank you for signing up, {{UserName}}! We are excited to have you on board.";
        public override string CallToAction { get; set; } = "{{EmailConfirmationLink}}";
        public override string CallToActionText { get; set; } = "Verify your email";
        public override string FooterContent { get; set; } = $"If you have any questions, feel free to contact us at {StaticEmailSettings.SenderEmail}.";
        public override string RecipientType { get; set; } = StaticUserRoles.Student;
        public VerificationEmailTemplate() { }
    }
}
