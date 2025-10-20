using AttaEduSystem.Utilities.Constants;
using StackExchange.Redis;

namespace AttaEduSystem.API.Extension
{
    public static class RedisServiceExtensions
    {
        public static WebApplicationBuilder AddRedisCache(this WebApplicationBuilder builder)
        {
            string connectionString =
                builder.Configuration.GetSection("Redis")[StaticConnectionString.RedisConnectionString];
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            return builder;
        }
    }
}
