﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using System.Security.Claims;

namespace AttaEduSystem.API.BackgroundServices
{
    public class ExamTimerBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExamTimerBackgroundService> _logger;

        public ExamTimerBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ExamTimerBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExamTimerBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessActiveRooms(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ExamTimerBackgroundService");
                }

                // Chạy mỗi 1 giây
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessActiveRooms(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var examHubService = scope.ServiceProvider.GetRequiredService<IExamHubService>();
            var examTakingService = scope.ServiceProvider.GetRequiredService<IExamTakingService>();

            var now = StaticOperationStatus.Timezone.Vietnam;

            // Lấy tất cả phòng đang InProgress
            var activeRooms = await unitOfWork.ExamRoom.GetAllAsync(r =>
                r.Status == StaticOperationStatus.ExamRoom.InProgress ||
                (r.Status == StaticOperationStatus.ExamRoom.Waiting && r.StartTime <= now));

            foreach (var room in activeRooms)
            {
                // Nếu phòng Waiting nhưng đã đến giờ -> Chuyển sang InProgress
                if (room.Status == StaticOperationStatus.ExamRoom.Waiting && room.StartTime <= now)
                {
                    room.Status = StaticOperationStatus.ExamRoom.InProgress;
                    unitOfWork.ExamRoom.Update(room);
                    await unitOfWork.SaveAsync();

                    await examHubService.SendRoomStatusChanged(
                        room.RoomCode,
                        StaticOperationStatus.ExamRoom.InProgress,
                        "Exam has started!");

                    _logger.LogInformation("Room {RoomCode} started.", room.RoomCode);
                }

                // Nếu phòng InProgress -> Gửi timer update
                if (room.Status == StaticOperationStatus.ExamRoom.InProgress && room.EndTime.HasValue)
                {
                    var remainingSeconds = (int)(room.EndTime.Value - now).TotalSeconds;

                    if (remainingSeconds > 0)
                    {
                        // Gửi timer update mỗi giây
                        await examHubService.SendTimerUpdate(room.RoomCode, remainingSeconds);
                    }
                    else
                    {
                        // Hết giờ -> Force submit & Chuyển sang Finished
                        await examHubService.ForceSubmitAll(room.RoomCode);

                        // NEW: Auto-submit cho toàn bộ participant chưa nộp
                        var participants = await unitOfWork.ExamRoomParticipant.GetByRoomIdAsync(room.ExamRoomId);
                        foreach (var p in participants)
                        {
                            var attemptId = p.ExamAttemptId;
                            var attempt = attemptId.HasValue
                                ? await unitOfWork.ExamAttempt.GetAttemptWithDetailsAsync(attemptId.Value)
                                : null;

                            if (attempt != null && !attempt.CompletedAt.HasValue)
                            {
                                try
                                {
                                    // Gọi trực tiếp service auto-submit ở quyền hệ thống (không cần user)
                                    await examTakingService.AutoSubmitExam(attempt.ExamAttemptId, CreateSystemPrincipal(attempt.UserId));
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Auto-submit failed for attempt {AttemptId}", attempt.ExamAttemptId);
                                }
                            }
                        }

                        room.Status = StaticOperationStatus.ExamRoom.Finished;
                        unitOfWork.ExamRoom.Update(room);
                        await unitOfWork.SaveAsync();

                        await examHubService.SendRoomStatusChanged(
                            room.RoomCode,
                            StaticOperationStatus.ExamRoom.Finished,
                            "Exam has ended!");

                        _logger.LogInformation("Room {RoomCode} finished.", room.RoomCode);
                    }
                }
            }
        }

        private static ClaimsPrincipal CreateSystemPrincipal(string userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "system"),
                new Claim(ClaimTypes.Role, "System")
            }, "System");
            return new ClaimsPrincipal(identity);
        }
    }
}
