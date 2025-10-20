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
            // Đọc chuỗi kết nối Redis từ file cấu hình
            var redisConnectionString = builderConfiguration.GetValue<string>("Redis:ConnectionString");
            // Đăng ký IConnectionMultiplexer
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<CloudinaryServiceControl>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IManageUserAccountService, ManageUserAccountService>();

            // add services here



            return services;
        }
    }
}
