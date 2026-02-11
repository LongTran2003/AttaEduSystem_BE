using AttaEduSystem.API.Hubs;
using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.DataAccess.Repositories;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Services.Mapping;
using AttaEduSystem.Services.Services;
using AttaEduSystem.Services.Services.CloudinaryModule.Invoker;
using StackExchange.Redis;

namespace AttaEduSystem.API.Extension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services,
    ConfigurationManager builderConfiguration)
        {
            // 1. Redis
            // Đọc chuỗi kết nối Redis từ file cấu hình
            var redisConnectionString = builderConfiguration.GetValue<string>("Redis:ConnectionString");
            // Đăng ký IConnectionMultiplexer
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

            // 2. AutoMapper & UnitOfWork
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Các Service cơ bản
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IManageUserAccountService, ManageUserAccountService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IAdminService, AdminService>();

            // 4. Cloudinary
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<CloudinaryServiceControl>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            // 5. Exam Core Services
            services.AddScoped<IExamScanningService, ExamScanningService>();
            services.AddScoped<IExamSolvingService, ExamSolvingService>();
            services.AddScoped<IExamGeneratingService, ExamGeneratingService>();
            services.AddScoped<IOcrService, GoogleVisionOcrService>();
            services.AddScoped<IExamFormatParser, RegexExamFormatParser>();
            services.AddScoped<IExamTakingService, ExamTakingService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IExamQuestionService, ExamQuestionService>();
            services.AddScoped<IExamShuffleService, ExamShuffleService>();
            services.AddScoped<IExamRoomService, ExamRoomService>();
            services.AddScoped<IExportService, ExportService>();

            // --- A. Cấu hình cho OpenAI ---
            // OpenAiService dùng IHttpClientFactory, nên đăng ký Scoped bình thường
            services.AddScoped<IOpenAiService, OpenAiService>();

            // Đăng ký một HttpClient có tên là "OpenAI" để OpenAiService gọi .CreateClient("OpenAI")
            services.AddHttpClient("OpenAI", client =>
            {
                client.BaseAddress = new Uri("https://api.openai.com/v1/");
                client.Timeout = TimeSpan.FromMinutes(2); // Timeout dài hơn để AI kịp trả lời
            });

            // --- B. Cấu hình cho Gemini ---
            // GeminiService dùng HttpClient trực tiếp trong Constructor
            // Nên dùng AddHttpClient<Interface, Class> để nó tự inject HttpClient vào
            services.AddHttpClient<IGeminiAiService, GeminiAiService>();

            // 6. Payment Services
            services.AddScoped<IPayOsService, PayOsService>();

            // 7. Usage Tracker Service
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IUsageTrackerService, UsageTrackerService>();

            // 8. QR Code Service
            services.AddScoped<IQrCodeService, QrCodeService>();

            // 9. SignalR Hub Service
            services.AddScoped<IExamHubService, ExamHubService>();



            return services;
        }
    }
}
