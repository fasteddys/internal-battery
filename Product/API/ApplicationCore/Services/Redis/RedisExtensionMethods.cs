using Microsoft.Extensions.Configuration;
using System;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Services.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisExtensionMethods
    {
        public static IServiceCollection AddRedisClient(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }

            return services
                .Configure<RedisOptions>(config)
                .AddSingleton<IRedisService, RedisService>();
        }

        public static IServiceCollection AddRedisClient(this IServiceCollection services, Action<RedisOptions> configureOptions)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }

            return services
                .Configure(configureOptions)
                .AddSingleton<IRedisService, RedisService>();
        }

    }
}
