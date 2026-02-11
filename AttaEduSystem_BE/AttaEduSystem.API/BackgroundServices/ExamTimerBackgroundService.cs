using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;

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
    }
}
