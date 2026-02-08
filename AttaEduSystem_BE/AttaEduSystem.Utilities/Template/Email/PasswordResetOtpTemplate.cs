namespace AttaEduSystem.Utilities.Template.Email
{
    public class PasswordResetOtpTemplate : GenericEmailTemplate
    {
        public override string Subject => "AttaEdu - Reset Password OTP";

        public override string BodyContent => @"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Password OTP</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }
        .container {
            max-width: 600px;
            margin: 50px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        .header {
            background-color: #4CAF50;
            color: white;
            padding: 20px;
            text-align: center;
        }
        .content {
            padding: 30px;
        }
        .otp-box {
            background-color: #f9f9f9;
            border-left: 4px solid #4CAF50;
            padding: 20px;
            margin: 20px 0;
            text-align: center;
        }
        .otp-code {
            font-size: 32px;
            font-weight: bold;
            color: #4CAF50;
            letter-spacing: 5px;
        }
        .footer {
            background-color: #f4f4f4;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #888;
        }
        .warning {
            color: #ff5722;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class=""content"">
            <p>Hi <strong>{{UserName}}</strong>,</p>
            <p>We received a request to reset your password for your AttaEdu account.</p>
            <p>Please use the following OTP code to reset your password:</p>
            
            <div class=""otp-box"">
                <p style=""margin: 0; font-size: 14px; color: #666;"">Your OTP Code:</p>
                <p class=""otp-code"">{{OTPnumbers}}</p>
                <p style=""margin: 0; font-size: 12px; color: #999;"">This code is valid for <span class=""warning"">10 minutes</span></p>
            </div>

            <p><strong class=""warning"">⚠️ Important:</strong></p>
            <ul>
                <li>Do not share this OTP code with anyone</li>
                <li>If you didn't request a password reset, please ignore this email</li>
                <li>The OTP will expire after 10 minutes</li>
            </ul>

            <p>If you're having trouble, please contact our support team.</p>
            <p>Best regards,<br><strong>AttaEdu Team</strong></p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 AttaEdu. All rights reserved.</p>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>
        ";
    }
}
