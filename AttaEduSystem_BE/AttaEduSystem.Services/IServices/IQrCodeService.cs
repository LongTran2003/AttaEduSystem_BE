namespace AttaEduSystem.Services.IServices
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Generate QR Code image as PNG byte array
        /// </summary>
        /// <param name="content">URL or text to encode</param>
        /// <param name="pixelsPerModule">Size of each QR module (default: 10)</param>
        /// <returns>PNG image as byte array</returns>
        byte[] GenerateQrCode(string content, int pixelsPerModule = 10);

        /// <summary>
        /// Generate QR Code for Exam Room join link
        /// </summary>
        string GetExamRoomJoinUrl(string roomCode);

        /// <summary>
        /// Generate QR Code for Exam Paper share link
        /// </summary>
        string GetExamPaperShareUrl(Guid examPaperId);
    }
}
