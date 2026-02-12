using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ChatBox;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Models.Enums;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatAiService _chatAiService;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IUnitOfWork unitOfWork,
            IChatAiService chatAiService,
            IMapper mapper,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _chatAiService = chatAiService;
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
            ChatMessageDto? aiResponseDto = null;
            try
            {
                var aiResponse = await _chatAiService.GetChatResponseAsync(dto.Message);

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

                aiResponseDto = _mapper.Map<ChatMessageDto>(assistantMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI response");

                // Vẫn trả về error message cho user
                aiResponseDto = new ChatMessageDto
                {
                    ChatMessageId = Guid.NewGuid(),
                    Role = "Assistant",
                    Content = "Sorry, I'm having trouble. Please try again later.",
                    CreatedTime = StaticOperationStatus.Timezone.Vietnam
                };
            }

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
            string aiResponseContent;
            try
            {
                aiResponseContent = await _chatAiService.GetChatResponseAsync(dto.Content, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI response");
                aiResponseContent = "Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại sau.";
            }

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
    }
}
