using AttaEduSystem.Utilities.Constants;

namespace AttaEduSystem.Utilities.Template.Email
{
    public class GenericEmailTemplate
    {
        public virtual string TemplateName { get; set; } = StaticEmailTemplates.Default;
        public virtual string? SenderName { get; set; } = StaticEmailSettings.SenderName;
        public virtual string? SenderEmail { get; set; } = StaticEmailSettings.SenderEmail;
        public virtual string Category { get; set; } = "General";
        public virtual string Subject { get; set; } = "No Subject";
        public virtual string? PreHeaderText { get; set; } = "";
        public virtual string? PersonalizationTags { get; set; } = "";
        public virtual string BodyContent { get; set; } = "Default body content.";
        public virtual string? FooterContent { get; set; } = "Default footer content.";
        public virtual string? CallToAction { get; set; } = "#";
        public virtual string? CallToActionText { get; set; } = "Click here";
        public virtual string? Language { get; set; } = "en-US";
        public virtual string? RecipientType { get; set; } = "General";

        /// <summary>
        ///     Replace placeholders in the email template with the values from the placeholders dictionary.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="placeholders"></param>
        /// <returns> The content with the placeholders set with actual values </returns>
        /*protected string ReplacePlaceholders(string content, Dictionary<string, string> placeholders)
        {
            foreach (var placeholder in placeholders)
                content = content.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);

            return content;
        }*/
        /*        protected string ReplacePlaceholders(string content, Dictionary<string, string> placeholders)
                {
                    foreach (var placeholder in placeholders)
                        content = content.Replace(placeholder.Key, placeholder.Value);

                    return content;
                }*/

        public string ReplacePlaceholders(string content, Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrEmpty(content) || placeholders == null)
                return content;

            foreach (var placeholder in placeholders)
            {
                var key = placeholder.Key;
                var value = placeholder.Value ?? string.Empty;
                content = content.Replace(key, value);
            }

            return content;
        }

        /// <summary>
        ///     Render: A method that renders the email template with the placeholders replaced with the actual values
        /// </summary>
        /// <param name="placeholders"></param>
        /// <returns> An HTML-formatted string that contains the passed values </returns>
        public virtual string Render(Dictionary<string, string> placeholders)
        {
            var subject = ReplacePlaceholders(Subject, placeholders);
            var preHeaderText = ReplacePlaceholders(PreHeaderText ?? "", placeholders);
            var bodyContent = ReplacePlaceholders(BodyContent, placeholders);
            var callToActionUrl = ReplacePlaceholders(CallToAction ?? "#", placeholders);
            var callToActionText = ReplacePlaceholders(CallToActionText ?? "", placeholders);
            var footerContent = ReplacePlaceholders(FooterContent ?? "", placeholders);

            var callToActionHtml = string.IsNullOrWhiteSpace(callToActionText)
        ? ""
        : $@"
            <div class='call-to-action'>
                <a href='{callToActionUrl}' class='button' style='color: #ffffff; text-decoration: none;'>{callToActionText}</a>
            </div>";

            return $@"
                <html lang='en' style='margin: 0; padding: 0;'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>{subject}</title>
                        <style>
                            body {{
                            margin: 0;
                            padding: 0;
                            background-color: white;
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            }}

                            .container {{width: 100%;
                                max-width: 602px;
                                margin: auto;
                                background: white;
                                border-radius: 10px;
                                overflow: hidden;
                                box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
                            }}

                            li {{
                            margin-bottom: 10px;
                            font-size: 18px;
                            }}

                            .header {{
                                background-color: white;
                                padding: 40px 0;
                                text-align: center;
                            }}
                            
                            .header img {{
                                width: 400px;
                                display: block;
                                margin: auto;
                            }}
                            
                            .content {{
                                padding: 30px;
                            }}
                            
                            h1 {{
                                font-size: 26px;
                                color: #c17037;
                                margin-bottom: 20px;
                            }}
                            
                            h2 {{
                                font-size: 22px;
                                color: #8a4814;
                                margin-bottom: 20px;
                            }}                          

                            p {{
                                font-size: 16px;
                                color: #555555;
                                margin-bottom: 20px;
                                line-height: 1.5;
                            }}

                            .call-to-action {{
                                margin-top: 20px;
                                text-align: center;
                            }}
                            
                            .button {{
                                display: inline-block;
                                padding: 12px 25px;
                                color: #ffffff;
                                background-color: #c17037;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                                transition: background-color 0.3s ease;
                                box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
                            }}
                            
                            .button:hover {{
                                background-color: #b17446;
                            }}
                            
                            .footer {{
                                background-color: #eeeeee;
                                padding: 20px;
                                text-align: center;
                                font-size: 14px;
                                color: #666;
                                border-top: 1px solid #ddd;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header' style='background-color: white; padding: 40px 0; text-align: center;'>
                                <img src='https://www.google.com/imgres?q=movie%20logo&imgurl=https%3A%2F%2Fstatic.vecteezy.com%2Fsystem%2Fresources%2Fthumbnails%2F012%2F657%2F549%2Fsmall_2x%2Fillustration-negative-film-reel-roll-tapes-for-movie-cinema-video-logo-vector.jpg&imgrefurl=https%3A%2F%2Fwww.vecteezy.com%2Ffree-vector%2Ffilm-logo&docid=mElbTrReWm4sNM&tbnid=B60hkh5GSq5z1M&vet=12ahUKEwjq8b71jteNAxXmT2wGHUBlJYoQM3oECBwQAA..i&w=400&h=400&hcb=2&ved=2ahUKEwjq8b71jteNAxXmT2wGHUBlJYoQM3oECBwQAA' alt='Company Logo'/>
                            </div>
                            <div class='content'>
                                                    <h1>{subject}</h1>
                                                    <h2>{preHeaderText}</h2>
                                                    <p>{bodyContent}</p>
                                                    {callToActionHtml}
                                                </div>
                                                <div class='footer'>
                                                    <p>&copy; {SenderName}, {DateTime.Now.Year}</p>
                                                    {footerContent}
                                                </div>
                                            </div>
                                        </body>
                                    </html>";
        }
    }
}
