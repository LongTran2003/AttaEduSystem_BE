namespace AttaEduSystem.Utilities.Template.Email
{
    public class NotificationEmailTemplate : GenericEmailTemplate
    {
        public override string TemplateName { get; set; } = "NotificationEmail";
        public override string Subject { get; set; } = "{{Title}}";
        public override string PreHeaderText { get; set; } = "Bạn có thông báo mới từ AttaEdu";
        public override string BodyContent { get; set; } = "{{Message}}";
        public override string? CallToAction { get; set; } = "{{ActionUrl}}";
        public override string? CallToActionText { get; set; } = "{{ActionText}}";

        public NotificationEmailTemplate() { }
    }
}
