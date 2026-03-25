using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AttaEduSystem.API.Hubs.Chat
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatAiService _chatAiService;
        private readonly IUsageTrackerService _usageTrackerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatAiService chatAiService,
            IUsageTrackerService usageTrackerService,
            IUnitOfWork unitOfWork,
            ILogger<ChatHub> logger)
        {
            _chatAiService = chatAiService;
            _usageTrackerService = usageTrackerService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =========================================================
        // SignalR Method: Send message with streaming response
        // =========================================================
        public async Task SendMessageStream(Guid conversationId, string message)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            try
            {
                // 1. Validate conversation
                var conversation = await _unitOfWork.ChatConversation
                    .GetByIdWithMessagesAsync(conversationId, userId);

                if (conversation == null)
                {
                    await Clients.Caller.SendAsync("Error", "Conversation not found");
                    return;
                }

                // 2. Save user message
                var userMessage = new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    ChatConversationId = conversationId,
                    Role = MessageRole.User,
                    Content = message,
                    CreatedBy = Context.User?.FindFirstValue("FullName"),
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };
                await _unitOfWork.ChatMessage.AddAsync(userMessage);
                await _unitOfWork.SaveAsync();

                // 3. Notify user message saved
                await Clients.Caller.SendAsync("UserMessageSaved", new
                {
                    userMessage.ChatMessageId,
                    userMessage.Content,
                    userMessage.CreatedTime
                });

                // 4. Build conversation history
                var history = conversation.Messages
                    .TakeLast(10)
                    .Select(m => (m.Role == MessageRole.User ? "user" : "assistant", m.Content))
                    .ToList();

                var aiResponse = TryGetPredefinedAnswer(message);
                if (string.IsNullOrWhiteSpace(aiResponse))
                {
                    var canUseAi = await _usageTrackerService.TryConsumeAsync(Context.User!, UsageType.Token, 1);
                    if (!canUseAi)
                    {
                        aiResponse = "Bạn đã dùng hết lượt AI trong gói hiện tại. Bạn vẫn có thể hỏi các câu hỏi cơ bản về tính năng hệ thống.";
                    }
                    else
                    {
                        aiResponse = await _chatAiService.GetChatResponseAsync(message, history);
                    }
                }

                // 6. Save AI response
                var assistantMessage = new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    ChatConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = aiResponse,
                    CreatedBy = "AI",
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };
                await _unitOfWork.ChatMessage.AddAsync(assistantMessage);

                // Update conversation timestamp
                conversation.UpdatedAt = StaticOperationStatus.Timezone.Vietnam;
                conversation.UpdatedBy = Context.User?.FindFirstValue("FullName");
                conversation.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
                _unitOfWork.ChatConversation.Update(conversation);

                await _unitOfWork.SaveAsync();

                // 7. Stream response (simulate chunk streaming)
                await Clients.Caller.SendAsync("StreamStart", assistantMessage.ChatMessageId);

                // Simulate streaming by sending chunks (split by sentences)
                var chunks = aiResponse.Split(new[] { ". ", ".\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var chunk in chunks)
                {
                    await Clients.Caller.SendAsync("StreamChunk", chunk + ". ");
                    await Task.Delay(50); // Simulate typing delay
                }

                await Clients.Caller.SendAsync("StreamComplete", new
                {
                    assistantMessage.ChatMessageId,
                    assistantMessage.Content,
                    assistantMessage.CreatedTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageStream");
                await Clients.Caller.SendAsync("Error", "Đã xảy ra lỗi khi xử lý tin nhắn.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} connected to ChatHub", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} disconnected from ChatHub", userId);
            await base.OnDisconnectedAsync(exception);
        }

        private static string? TryGetPredefinedAnswer(string message)
        {
            var normalized = (message ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return "Bạn hãy nhập câu hỏi cụ thể để mình hỗ trợ.";

            if (normalized.Contains("join phòng") || normalized.Contains("mã phòng") || normalized.Contains("tham gia phòng"))
                return "Bạn vào mục Phòng thi, nhập mã phòng hoặc mở link tham gia. Sau khi join thành công, hệ thống sẽ cấp đề và lượt làm bài.";

            if (normalized.Contains("nộp bài") || normalized.Contains("kết quả"))
                return "Sau khi nộp bài, hệ thống chấm điểm tự động và hiển thị kết quả chi tiết theo từng câu đúng/sai.";

            if (normalized.Contains("lớp học") || normalized.Contains("quản lý lớp"))
                return "Mục Lớp học cho phép tạo lớp, thêm học sinh/giáo viên và xem thống kê lớp.";

            return null;
        }
    }
}
