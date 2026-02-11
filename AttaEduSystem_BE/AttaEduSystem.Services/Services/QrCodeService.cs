using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace AttaEduSystem.Services.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly string _frontendBaseUrl;

        public QrCodeService(IConfiguration configuration)
        {
            // Đọc từ appsettings.json, mặc định localhost nếu chưa config
            _frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "https://attaedu.site";
        }

        // =========================================================
        // GENERATE QR CODE (PNG byte array)
        // =========================================================
        public byte[] GenerateQrCode(string content, int pixelsPerModule = 10)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(pixelsPerModule);
        }

        // =========================================================
        // GET EXAM ROOM JOIN URL
        // =========================================================
        public string GetExamRoomJoinUrl(string roomCode)
        {
            // URL: https://attaedu.site/join-room/ABC123
            return $"{_frontendBaseUrl}/join-room/{roomCode}";
        }

        // =========================================================
        // GET EXAM PAPER SHARE URL
        // =========================================================
        public string GetExamPaperShareUrl(Guid examPaperId)
        {
            // URL: https://attaedu.site/exam/practice/{id}
            return $"{_frontendBaseUrl}/exam/practice/{examPaperId}";
        }
    }
}
