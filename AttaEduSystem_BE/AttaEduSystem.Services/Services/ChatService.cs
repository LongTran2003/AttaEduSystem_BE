using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ChatBot;
using AttaEduSystem.Models.DTOs.ChatBox;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;

namespace AttaEduSystem.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatAiService _chatAiService;
        private readonly IUsageTrackerService _usageTrackerService;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IUnitOfWork unitOfWork,
            IChatAiService chatAiService,
            IUsageTrackerService usageTrackerService,
            IMapper mapper,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _chatAiService = chatAiService;
            _usageTrackerService = usageTrackerService;
            _mapper = mapper;
            _logger = logger;
        }

        // =========================================================
        // POST - Tạo conversation mới + gửi message + nhận AI reply
        // =========================================================
        public async Task<ResponseDto> CreateConversationAsync(CreateConversationDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build("Unauthorized", 401);
            }

            // 1. Tạo conversation
            var conversation = new ChatConversation
            {
                ChatConversationId = Guid.NewGuid(),
                UserId = userId,
                Title = GenerateTitle(dto.Message), // Auto-generate từ message
                CreatedBy = user.FindFirstValue("FullName"),
                CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                UpdatedAt = StaticOperationStatus.Timezone.Vietnam,
                Status = "Active"
            };
            await _unitOfWork.ChatConversation.AddAsync(conversation);

            // 2. Lưu user message
            var userMessage = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid(),
                ChatConversationId = conversation.ChatConversationId,
                Role = MessageRole.User,
                Content = dto.Message,
                CreatedBy = user.FindFirstValue("FullName"),
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };
            await _unitOfWork.ChatMessage.AddAsync(userMessage);

            // 3. Gọi AI
            var aiResponse = await GetAssistantResponseAsync(user, dto.Message, null, false);
            var assistantMessage = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid(),
                ChatConversationId = conversation.ChatConversationId,
                Role = MessageRole.Assistant,
                Content = aiResponse,
                CreatedBy = "AI",
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };
            await _unitOfWork.ChatMessage.AddAsync(assistantMessage);
            var aiResponseDto = _mapper.Map<ChatMessageDto>(assistantMessage);

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build(
                message: "Message sent successfully",
                statusCode: 201,
                result: new
                {
                    ConversationId = conversation.ChatConversationId,
                    Title = conversation.Title,
                    AiResponse = aiResponseDto
                });
        }

        // =========================================================
        // GET - Lấy danh sách conversations
        // =========================================================
        public async Task<ResponseDto> GetConversationsAsync(ClaimsPrincipal user, int page = 1, int pageSize = 20)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build("Unauthorized", 401);
            }

            var (conversations, totalCount) = await _unitOfWork.ChatConversation
                .GetConversationsAsync(userId, page, pageSize);

            // Map sang DTO
            var conversationDtos = _mapper.Map<List<ChatConversationDto>>(conversations);

            return SuccessResponse.Build(
                message: "Conversations retrieved successfully",
                statusCode: 200,
                result: new
                {
                    Data = conversationDtos,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
        }

        // =========================================================
        // GET - Lấy chi tiết conversation + messages
        // =========================================================
        public async Task<ResponseDto> GetConversationByIdAsync(Guid conversationId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build("Unauthorized", 401);
            }

            var conversation = await _unitOfWork.ChatConversation
                .GetByIdWithMessagesAsync(conversationId, userId);

            if (conversation == null)
            {
                return ErrorResponse.Build("Conversation not found", 404);
            }

            // Map sang DTO
            var result = _mapper.Map<ConversationDetailDto>(conversation);

            return SuccessResponse.Build(
                message: "Conversation retrieved successfully",
                statusCode: 200,
                result: result);
        }

        // =========================================================
        // POST - Gửi message và nhận AI reply
        // =========================================================
        public async Task<ResponseDto> SendMessageAsync(Guid conversationId, SendMessageDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build("Unauthorized", 401);
            }

            var conversation = await _unitOfWork.ChatConversation
                .GetByIdWithMessagesAsync(conversationId, userId);

            if (conversation == null)
            {
                return ErrorResponse.Build("Conversation not found", 404);
            }

            // 1. Lưu user message
            var userMessage = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid(),
                ChatConversationId = conversationId,
                Role = MessageRole.User,
                Content = dto.Content,
                CreatedBy = user.FindFirstValue("FullName"),
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };
            await _unitOfWork.ChatMessage.AddAsync(userMessage);

            // 2. Build conversation history cho AI (lấy tối đa 10 messages gần nhất)
            var history = conversation.Messages
                .TakeLast(10)
                .Select(m => (m.Role == MessageRole.User ? "user" : "assistant", m.Content))
                .ToList();

            // 3. Gọi AI
            var aiResponseContent = await GetAssistantResponseAsync(user, dto.Content, history, false);

            // 4. Lưu AI response
            var assistantMessage = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid(),
                ChatConversationId = conversationId,
                Role = MessageRole.Assistant,
                Content = aiResponseContent,
                CreatedBy = "AI",
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };
            await _unitOfWork.ChatMessage.AddAsync(assistantMessage);

            // 5. Update conversation timestamp
            conversation.UpdatedAt = StaticOperationStatus.Timezone.Vietnam;
            conversation.UpdatedBy = user.FindFirstValue("FullName");
            conversation.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.ChatConversation.Update(conversation);
            await _unitOfWork.SaveAsync();

            // Map sang DTO
            return SuccessResponse.Build(
                message: "Message sent successfully",
                statusCode: 200,
                result: new
                {
                    UserMessage = _mapper.Map<ChatMessageDto>(userMessage),
                    AiResponse = _mapper.Map<ChatMessageDto>(assistantMessage)
                });
        }

        // =========================================================
        // DELETE - Xóa conversation (soft delete)
        // =========================================================
        public async Task<ResponseDto> DeleteConversationAsync(Guid conversationId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build("Unauthorized", 401);
            }

            var conversation = await _unitOfWork.ChatConversation
                .GetAsync(c => c.ChatConversationId == conversationId && c.UserId == userId);

            if (conversation == null)
            {
                return ErrorResponse.Build("Conversation not found", 404);
            }

            // Soft delete
            conversation.Status = "Deleted";
            conversation.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
            conversation.UpdatedBy = user.FindFirstValue("FullName");

            await _unitOfWork.SaveAsync();

            return SuccessResponse.Build(
                message: "Conversation deleted successfully",
                statusCode: 200);
        }

        // =========================================================
        // ✅ NEW: POST - Hỏi về đề thi cụ thể
        // =========================================================
        public async Task<ResponseDto> AskAboutExamAsync(Guid examId, AskAboutExamDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorResponse.Build("Unauthorized", 401);
            }

            // 1. Validate exam access permission
            var exam = await _unitOfWork.ExamPaper.GetByIdWithUserAsync(examId);
            if (exam == null)
            {
                return ErrorResponse.Build("Exam not found", 404);
            }

            // Check permission: Own exam OR public exam
            bool hasAccess = exam.Creator?.Id == userId ||
                             exam.CreatedBy == userId ||
                             exam.Status == StaticOperationStatus.ExamPaper.Ready;

            if (!hasAccess)
            {
                return ErrorResponse.Build("You do not have permission to access this exam", 403);
            }

            // 2. Load exam questions (limit to first 5 for context efficiency)
            var questions = await _unitOfWork.ExamQuestion.GetByExamPaperIdWithOptionsAsync(examId);
            var contextQuestions = questions.Take(5).ToList();

            // 3. Load solution (if requested and exists)
            string? solutionText = null;
            if (dto.IncludeSolutions)
            {
                var solution = await _unitOfWork.ExamSolution.GetAsync(s => s.ExamPaperId == examId);
                if (solution != null)
                {
                    solutionText = solution.SolutionContentJson;
                }
            }

            // 4. Build exam context for AI
            var examContext = BuildExamContext(exam, contextQuestions, solutionText);

            // 5. Get or create conversation
            ChatConversation conversation;
            if (dto.ConversationId.HasValue)
            {
                // Attach to existing conversation
                conversation = await _unitOfWork.ChatConversation
                    .GetByIdWithMessagesAsync(dto.ConversationId.Value, userId);

                if (conversation == null)
                {
                    return ErrorResponse.Build("Conversation not found", 404);
                }

                // Update exam link if not set
                if (!conversation.ExamPaperId.HasValue)
                {
                    conversation.ExamPaperId = examId;
                    conversation.UpdatedAt = StaticOperationStatus.Timezone.Vietnam;
                    conversation.UpdatedBy = user.FindFirstValue("FullName");
                    _unitOfWork.ChatConversation.Update(conversation);
                }
            }
            else
            {
                // Create new conversation
                conversation = new ChatConversation
                {
                    ChatConversationId = Guid.NewGuid(),
                    UserId = userId,
                    Title = $"Về đề: {exam.Title}",
                    ExamPaperId = examId,
                    CreatedBy = user.FindFirstValue("FullName"),
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                    UpdatedAt = StaticOperationStatus.Timezone.Vietnam,
                    Status = "Active"
                };
                await _unitOfWork.ChatConversation.AddAsync(conversation);
            }

            // 6. Save user message
            var userMessage = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid(),
                ChatConversationId = conversation.ChatConversationId,
                Role = MessageRole.User,
                Content = dto.Message,
                CreatedBy = user.FindFirstValue("FullName"),
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };
            await _unitOfWork.ChatMessage.AddAsync(userMessage);

            // 7. Build conversation history
            var history = conversation.Messages
                .TakeLast(10)
                .Select(m => (m.Role == MessageRole.User ? "user" : "assistant", m.Content))
                .ToList();

            // 8. Prepend exam context to current message
            var enhancedMessage = $"{examContext}\n\n---\n\nCâu hỏi của học sinh: {dto.Message}";

            // 9. Call AI with exam context
            var aiResponseContent = await GetAssistantResponseAsync(user, enhancedMessage, history, true);

            // 10. Save AI response
            var assistantMessage = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid(),
                ChatConversationId = conversation.ChatConversationId,
                Role = MessageRole.Assistant,
                Content = aiResponseContent,
                CreatedBy = "AI",
                CreatedTime = StaticOperationStatus.Timezone.Vietnam
            };
            await _unitOfWork.ChatMessage.AddAsync(assistantMessage);

            // 11. Update conversation timestamp
            conversation.UpdatedAt = StaticOperationStatus.Timezone.Vietnam;
            conversation.UpdatedBy = user.FindFirstValue("FullName");
            conversation.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

            _unitOfWork.ChatConversation.Update(conversation);
            await _unitOfWork.SaveAsync();

            // 12. Build response
            var response = new AskAboutExamResponseDto
            {
                ConversationId = conversation.ChatConversationId,
                ConversationTitle = conversation.Title,
                ExamPaperId = examId,
                ExamTitle = exam.Title,
                UserMessage = _mapper.Map<ChatMessageDto>(userMessage),
                AiResponse = _mapper.Map<ChatMessageDto>(assistantMessage)
            };

            return SuccessResponse.Build(
                message: "Message sent successfully",
                statusCode: 200,
                result: response);
        }

        // =========================================================
        // ✅ NEW: Streaming message via SignalR
        // =========================================================
        public async Task SendMessageStreamAsync(Guid conversationId, SendMessageDto dto, ClaimsPrincipal user, string connectionId)
        {
            // Implementation will use SignalR hub context to stream chunks
            // This is a placeholder - actual implementation in ChatHub
            throw new NotImplementedException("Use SignalR ChatHub.SendMessageStream instead");
        }

        // =========================================================
        // Helper: Generate title từ tin nhắn đầu tiên
        // =========================================================
        private static string GenerateTitle(string firstMessage)
        {
            if (string.IsNullOrWhiteSpace(firstMessage))
                return "New Conversation";

            // Lấy 50 ký tự đầu tiên
            var title = firstMessage.Length > 50
                ? firstMessage[..50] + "..."
                : firstMessage;

            // Remove newlines
            return title.Replace("\n", " ").Replace("\r", "").Trim();
        }

        private async Task<string> GetAssistantResponseAsync(
            ClaimsPrincipal user,
            string message,
            List<(string Role, string Content)>? history,
            bool isExamContext)
        {
            var cleanMessage = (message ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanMessage))
                return "Bạn hãy nhập câu hỏi cụ thể để mình hỗ trợ chính xác hơn.";

            if (!isExamContext)
            {
                var predefinedAnswer = TryGetPredefinedAnswer(cleanMessage);
                if (!string.IsNullOrWhiteSpace(predefinedAnswer))
                    return predefinedAnswer;
            }

            var canUseAi = await _usageTrackerService.TryConsumeAsync(user, UsageType.Token, 1);
            if (!canUseAi)
            {
                return "Bạn đã dùng hết lượt AI trong gói hiện tại. Mình vẫn hỗ trợ câu hỏi cơ bản về tính năng hệ thống, hoặc bạn có thể nâng cấp gói để tiếp tục hỏi AI.";
            }

            try
            {
                return await _chatAiService.GetChatResponseAsync(cleanMessage, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI response");
                return isExamContext
                    ? "Mình chưa phân tích đề thi được lúc này. Bạn thử gửi lại sau vài phút."
                    : "Mình đang bận xử lý hệ thống. Bạn thử lại sau ít phút nhé.";
            }
        }

        private static string? TryGetPredefinedAnswer(string message)
        {
            var normalized = message.Trim().ToLowerInvariant();

            if (normalized.Contains("tham gia phòng") || normalized.Contains("join phòng") || normalized.Contains("mã phòng"))
                return "Bạn vào mục Phòng thi, nhập mã phòng hoặc mở link join theo roomId. Khi vào phòng thành công, hệ thống sẽ cấp đề và tạo lượt làm bài tự động.";

            if (normalized.Contains("nộp bài") || normalized.Contains("submit") || normalized.Contains("kết quả"))
                return "Sau khi bấm Nộp bài, hệ thống chấm điểm tự động và trả kết quả ngay. Bạn có thể xem chi tiết câu đúng/sai và gợi ý ôn tập trong phần kết quả.";

            if (normalized.Contains("quên mật khẩu") || normalized.Contains("đổi mật khẩu"))
                return "Bạn dùng chức năng Quên mật khẩu ở trang đăng nhập để nhận hướng dẫn đặt lại mật khẩu qua email đã đăng ký.";

            if (normalized.Contains("gói") || normalized.Contains("nâng cấp") || normalized.Contains("free") || normalized.Contains("pro"))
                return "Hệ thống có giới hạn lượt AI theo gói dịch vụ. Bạn có thể xem mức sử dụng trong trang tài khoản và nâng cấp gói để mở rộng lượt dùng.";

            if (normalized.Contains("lớp học") || normalized.Contains("quản lý lớp"))
                return "Mục Lớp học cho phép tạo lớp, thêm giáo viên/học sinh, xem thống kê lớp và theo dõi các hoạt động học tập.";

            if (normalized.Contains("đề thi") || normalized.Contains("tạo đề") || normalized.Contains("ocr"))
                return "Bạn có thể tạo đề từ OCR, chỉnh sửa câu hỏi/đáp án, sau đó mở phòng thi hoặc làm đề luyện tập trực tiếp.";

            if (normalized.Contains("xin chào") || normalized.Contains("hello") || normalized == "hi")
                return "Chào bạn, mình là trợ lý hỗ trợ học tập của AttaEdu. Bạn cần hỗ trợ về lớp học, phòng thi, nộp bài hay AI giải thích đề?";

            return null;
        }

        // =========================================================
        // Helper: Build Exam Context for AI
        // =========================================================
        private static string BuildExamContext(ExamPaper exam, List<ExamQuestion> questions, string? solution)
        {
            var sb = new StringBuilder();

            sb.AppendLine("📝 BỐI CẢNH ĐỀ THI:");
            sb.AppendLine($"- Tiêu đề: {exam.Title}");
            sb.AppendLine($"- Môn học: {exam.Subject ?? "Chung"}");
            sb.AppendLine($"- Mô tả: {exam.Description ?? "Không có"}");
            sb.AppendLine($"- Tổng số câu hỏi: {questions.Count}");
            sb.AppendLine();

            if (questions.Any())
            {
                sb.AppendLine("📌 MẪU CÂU HỎI (5 câu đầu tiên):");
                foreach (var q in questions)
                {
                    sb.AppendLine($"\n{q.QuestionIdLabel}: {q.Content}");
                    if (q.QuestionType == "MultipleChoice" && q.Options.Any())
                    {
                        foreach (var opt in q.Options.OrderBy(o => o.Label))
                        {
                            sb.AppendLine($"  {opt.Label}. {opt.Content}");
                        }
                        if (!string.IsNullOrEmpty(q.CorrectAnswer))
                        {
                            sb.AppendLine($"  ✅ Đáp án: {q.CorrectAnswer}");
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(solution))
            {
                sb.AppendLine("\n📖 LỜI GIẢI (nếu có):");
                sb.AppendLine(solution);
            }

            sb.AppendLine("\n---");
            sb.AppendLine("🎯 NHIỆM VỤ CỦA BẠN:");
            sb.AppendLine("Trả lời câu hỏi của học sinh dựa trên thông tin đề thi ở trên.");
            sb.AppendLine("Sử dụng LaTeX ($...$) cho công thức toán học.");

            return sb.ToString();
        }
    }
}
