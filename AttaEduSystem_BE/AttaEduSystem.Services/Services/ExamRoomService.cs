using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.ExamRoom.Room;
using AttaEduSystem.Models.DTOs.ExamRoom.TakeExam;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services
{
    public class ExamRoomService : IExamRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExamRoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // =========================================================
        // CREATE ROOM
        // =========================================================
        public async Task<ResponseDto> CreateRoom(CreateExamRoomDto dto, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var fullName = user.FindFirstValue("FullName");

                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var examPaper = await _unitOfWork.ExamPaper.GetAsync(e => e.ExamPaperId == dto.ExamPaperId);
                if (examPaper == null)
                    return ErrorResponse.Build("Exam paper not found", 404);

                if (examPaper.CreatedBy != userId)
                    return ErrorResponse.Build(
                        "You do not have permission to create room for this exam paper", 403);

                var vietnamNow = StaticOperationStatus.Timezone.Vietnam;

                // 🔴 SỬA Ở ĐÂY: Đồng bộ giờ FE gửi lên thành giờ VN nếu FE gửi UTC
                var startTime = dto.StartTime.Kind == DateTimeKind.Utc
                                ? dto.StartTime.AddHours(7)
                                : dto.StartTime;

                if (startTime < vietnamNow)
                    return ErrorResponse.Build("Start time must be in the future", 400);

                // Generate unique code
                string code;
                do { code = GenerateRoomCode(); }
                while (await _unitOfWork.ExamRoom.IsCodeExistsAsync(code));

                // Map DTO -> Entity
                var examRoom = _mapper.Map<ExamRoom>(dto);
                examRoom.RoomCode = code;
                examRoom.StartTime = startTime;
                examRoom.EndTime = startTime.AddMinutes(dto.TimeLimit);
                examRoom.CreatedBy = fullName;
                examRoom.CreatedTime = StaticOperationStatus.Timezone.Vietnam;
                examRoom.Status = StaticOperationStatus.ExamRoom.Waiting;

                await _unitOfWork.ExamRoom.AddAsync(examRoom);
                await _unitOfWork.SaveAsync();

                // Map Entity -> Response DTO
                var responseDto = _mapper.Map<ExamRoomResponseDto>(examRoom);
                responseDto.ExamTitle = examPaper.Title;
                responseDto.Status = StaticOperationStatus.ExamRoom.Waiting;
                responseDto.CurrentParticipants = 0;

                return SuccessResponse.Build("Exam room created successfully", 201, responseDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to create exam room: {ex.Message}", 500);
            }
        }

        // =========================================================
        // GET MY ROOMS
        // =========================================================
        public async Task<ResponseDto> GetMyRooms(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var rooms = await _unitOfWork.ExamRoom.GetByCreatorIdAsync(userId);

                var responseDtos = rooms.Select(r =>
                {
                    var dto = _mapper.Map<ExamRoomResponseDto>(r);
                    dto.Status = GetCurrentRoomStatus(r);
                    return dto;
                }).ToList();

                return SuccessResponse.Build("Rooms retrieved successfully", 200, responseDtos);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve rooms: {ex.Message}", 500);
            }
        }

        // =========================================================
        // CANCEL ROOM
        // =========================================================
        public async Task<ResponseDto> CancelRoom(Guid examRoomId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var room = await _unitOfWork.ExamRoom.GetByIdWithParticipantsAsync(examRoomId);
                if (room == null)
                    return ErrorResponse.Build("Exam room not found", 404);

                if (room.CreatedBy != userId)
                    return ErrorResponse.Build("You do not have permission to cancel this room", 403);

                if (GetCurrentRoomStatus(room) == StaticOperationStatus.ExamRoom.Finished)
                    return ErrorResponse.Build("Cannot cancel a finished room", 400);

                room.Status = StaticOperationStatus.ExamRoom.Cancelled;
                room.UpdatedBy = userId;
                room.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;

                _unitOfWork.ExamRoom.Update(room);
                await _unitOfWork.SaveAsync();

                return SuccessResponse.Build("Room cancelled successfully", 200);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to cancel room: {ex.Message}", 500);
            }
        }

        // =========================================================
        // GET ROOM BY CODE (Public)
        // =========================================================
        public async Task<ResponseDto> GetRoomByCode(string code)
        {
            try
            {
                var room = await _unitOfWork.ExamRoom.GetByCodeWithParticipantsAsync(code.ToUpper());
                if (room == null)
                    return ErrorResponse.Build("Room not found", 404);

                var responseDto = _mapper.Map<ExamRoomResponseDto>(room);
                responseDto.Status = GetCurrentRoomStatus(room);

                return SuccessResponse.Build("Room retrieved successfully", 200, responseDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve room: {ex.Message}", 500);
            }
        }

        // =========================================================
        // GET ROOM STATUS (Public)
        // =========================================================
        public async Task<ResponseDto> GetRoomStatus(string code)
        {
            try
            {
                var room = await _unitOfWork.ExamRoom.GetByCodeWithParticipantsAsync(code.ToUpper());
                if (room == null)
                    return ErrorResponse.Build("Room not found", 404);

                var currentStatus = GetCurrentRoomStatus(room);
                var statusDto = _mapper.Map<ExamRoomStatusDto>(room);

                // Set computed properties
                statusDto.Status = currentStatus;
                (statusDto.RemainingSeconds, statusDto.CanJoin, statusDto.Message) 
                    = GetStatusDetails(room, currentStatus);

                return SuccessResponse.Build("Room status retrieved successfully", 200, statusDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve room status: {ex.Message}", 500);
            }
        }

        // =========================================================
        // JOIN ROOM
        // =========================================================
        public async Task<ResponseDto> JoinRoom(string code, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var room = await _unitOfWork.ExamRoom.GetByCodeWithParticipantsAsync(code.ToUpper());
                if (room == null)
                    return ErrorResponse.Build("Room not found", 404);

                return await JoinRoomInternal(room, user, userId);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to join room: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> JoinRoomById(Guid examRoomId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var room = await _unitOfWork.ExamRoom.GetByIdWithParticipantsAsync(examRoomId);
                if (room == null)
                    return ErrorResponse.Build("Room not found", 404);

                return await JoinRoomInternal(room, user, userId);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to join room: {ex.Message}", 500);
            }
        }

        // =========================================================
        // GET PAPER FOR TAKING (Secure)
        // =========================================================
        public async Task<ResponseDto> GetPaperForTaking(string code, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var room = await _unitOfWork.ExamRoom.GetByCodeWithParticipantsAsync(code.ToUpper());
                if (room == null)
                    return ErrorResponse.Build("Room not found", 404);

                return await GetPaperForTakingInternal(room, userId);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve exam paper: {ex.Message}", 500);
            }
        }

        public async Task<ResponseDto> GetPaperForTakingByRoomId(Guid examRoomId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return ErrorResponse.Build(StaticOperationStatus.User.UserNotFound, 401);

                var room = await _unitOfWork.ExamRoom.GetByIdWithParticipantsAsync(examRoomId);
                if (room == null)
                    return ErrorResponse.Build("Room not found", 404);

                return await GetPaperForTakingInternal(room, userId);
            }
            catch (Exception ex)
            {
                return ErrorResponse.Build($"Failed to retrieve exam paper: {ex.Message}", 500);
            }
        }

        // =========================================================
        // HELPER METHODS
        // =========================================================

        private static string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GetCurrentRoomStatus(ExamRoom room)
        {
            if (room.Status == StaticOperationStatus.ExamRoom.Cancelled)
                return StaticOperationStatus.ExamRoom.Cancelled;

            var now = StaticOperationStatus.Timezone.Vietnam;

            if (now < room.StartTime)
                return StaticOperationStatus.ExamRoom.Waiting;

            if (room.EndTime.HasValue && now > room.EndTime.Value)
                return StaticOperationStatus.ExamRoom.Finished;

            return StaticOperationStatus.ExamRoom.InProgress;
        }

        private static (int? RemainingSeconds, bool CanJoin, string Message) GetStatusDetails(ExamRoom room, string status)
        {
            var now = StaticOperationStatus.Timezone.Vietnam;
            var currentCount = room.Participants?.Count ?? 0;

            return status switch
            {
                var s when s == StaticOperationStatus.ExamRoom.Waiting =>
                    ((int)(room.StartTime - now).TotalSeconds, true, $"Room will start in {(int)(room.StartTime - now).TotalSeconds} seconds"),

                var s when s == StaticOperationStatus.ExamRoom.InProgress =>
                    (room.EndTime.HasValue ? (int)(room.EndTime.Value - now).TotalSeconds : null,
                     currentCount < room.MaxParticipants,
                     currentCount < room.MaxParticipants ? "Room is in progress, you can still join" : "Room is full"),

                var s when s == StaticOperationStatus.ExamRoom.Finished =>
                    (null, false, "Room has ended"),

                var s when s == StaticOperationStatus.ExamRoom.Cancelled =>
                    (null, false, "Room has been cancelled"),

                _ => (null, false, "Unknown status")
            };
        }

        private static ResponseDto? ValidateJoinRoom(ExamRoom room, string currentStatus)
        {
            if (currentStatus == StaticOperationStatus.ExamRoom.Cancelled)
                return ErrorResponse.Build("This room has been cancelled", 400);

            if (currentStatus == StaticOperationStatus.ExamRoom.Finished)
                return ErrorResponse.Build("This room has ended", 400);

            var currentCount = room.Participants?.Count ?? 0;
            if (currentCount >= room.MaxParticipants)
                return ErrorResponse.Build("Room is full", 400);

            return null;
        }

        private static ExamAttempt CreateExamAttempt(Guid examPaperId, string userId, string? fullName)
        {
            return new ExamAttempt
            {
                ExamAttemptId = Guid.NewGuid(),
                ExamPaperId = examPaperId,
                UserId = userId,
                StartedAt = StaticOperationStatus.Timezone.Vietnam,
                Score = 0,
                CorrectCount = 0,
                TotalQuestions = 0,
                CreatedBy = fullName,
                CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                Status = "InProgress"
            };
        }

        private ResponseDto BuildJoinResponse(ExamRoom room, ExamRoomParticipant participant, string currentStatus, string message)
        {
            var responseDto = _mapper.Map<JoinExamRoomResponseDto>(room);
            responseDto.ParticipantId = participant.ParticipantId;
            responseDto.RoomStatus = currentStatus;
            responseDto.ParticipantStatus = participant.Status!;
            responseDto.ExamAttemptId = participant.ExamAttemptId;
            responseDto.RemainingSeconds = CalculateRemainingSeconds(room, currentStatus);

            return SuccessResponse.Build(message, 200, responseDto);
        }

        private static int? CalculateRemainingSeconds(ExamRoom room, string status)
        {
            var now = StaticOperationStatus.Timezone.Vietnam;

            return status switch
            {
                var s when s == StaticOperationStatus.ExamRoom.Waiting => (int)(room.StartTime - now).TotalSeconds,
                var s when s == StaticOperationStatus.ExamRoom.InProgress && room.EndTime.HasValue => (int)(room.EndTime.Value - now).TotalSeconds,
                _ => null
            };
        }

        private async Task<ResponseDto> JoinRoomInternal(ExamRoom room, ClaimsPrincipal user, string userId)
        {
            var currentStatus = GetCurrentRoomStatus(room);
            var validationResult = ValidateJoinRoom(room, currentStatus);
            if (validationResult != null)
                return validationResult;

            var existingParticipant = await _unitOfWork.ExamRoomParticipant.GetByRoomAndUserAsync(room.ExamRoomId, userId);
            if (existingParticipant != null)
                return BuildJoinResponse(room, existingParticipant, currentStatus, "You have already joined this room");

            var participant = new ExamRoomParticipant
            {
                ParticipantId = Guid.NewGuid(),
                ExamRoomId = room.ExamRoomId,
                UserId = userId,
                JoinedAt = StaticOperationStatus.Timezone.Vietnam,
                CreatedBy = user.FindFirstValue("FullName"),
                CreatedTime = StaticOperationStatus.Timezone.Vietnam,
                Status = StaticOperationStatus.ExamRoomParticipant.Joined
            };

            var fullName = user.FindFirstValue("FullName");
            var attempt = CreateExamAttempt(room.ExamPaperId, userId, fullName);
            attempt.Status = currentStatus == StaticOperationStatus.ExamRoom.Waiting ? "Waiting" : "InProgress";

            await _unitOfWork.ExamAttempt.AddAsync(attempt);
            participant.ExamAttemptId = attempt.ExamAttemptId;
            await _unitOfWork.ExamRoomParticipant.AddAsync(participant);
            await _unitOfWork.SaveAsync();

            return BuildJoinResponse(room, participant, currentStatus, "Joined room successfully");
        }

        private async Task<ResponseDto> GetPaperForTakingInternal(ExamRoom room, string userId)
        {
            var participant = await _unitOfWork.ExamRoomParticipant.GetByRoomAndUserAsync(room.ExamRoomId, userId);
            if (participant == null)
                return ErrorResponse.Build("You must join the room first before getting the exam paper", 403);

            var currentStatus = GetCurrentRoomStatus(room);
            if (currentStatus == StaticOperationStatus.ExamRoom.Waiting)
                return ErrorResponse.Build("The exam has not started yet", 400);
            if (currentStatus == StaticOperationStatus.ExamRoom.Cancelled || currentStatus == StaticOperationStatus.ExamRoom.Finished)
                return ErrorResponse.Build("The exam room is closed", 400);

            var paper = await _unitOfWork.ExamPaper.GetAsync(
                p => p.ExamPaperId == room.ExamPaperId,
                includeProperties: "Questions,Questions.Options");

            if (paper == null)
                return ErrorResponse.Build("Exam paper not found", 404);

            var resultDto = _mapper.Map<TakeExamPaperDto>(paper);
            resultDto.TimeLimit = room.TimeLimit;

            return SuccessResponse.Build("Exam paper retrieved successfully", 200, resultDto);
        }
    }
}
