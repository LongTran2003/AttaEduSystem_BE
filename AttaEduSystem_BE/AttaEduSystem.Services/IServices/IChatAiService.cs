namespace AttaEduSystem.Services.IServices
{
    public interface IChatAiService
    {
        /// <summary>
        /// Gửi message và nhận AI response
        /// </summary>
        /// <param name="userMessage">Tin nhắn của user</param>
        /// <param name="conversationHistory">Lịch sử chat (optional, để AI có context)</param>
        /// <returns>AI response string</returns>
        Task<string> GetChatResponseAsync(string userMessage, List<(string Role, string Content)>? conversationHistory = null);
        Task<(bool IsOk, string Status, string Detail)> CheckHealthAsync(bool runProbe = false);
    }
}
