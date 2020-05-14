using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
    using UpDiddyApi.ApplicationCore.Services.B2B.CareerTalentPipeline;

    public static class ExtensionMethods
    {
        public static IServiceCollection AddCareerTalentPipelineService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }

            return services
                .Configure<CareerTalentPipelineOptions>(config)
                .AddTransient<ICareerTalentPipelineService, CareerTalentPipelineService>();
        }

        public static IServiceCollection AddCareerTalentPipelineService(this IServiceCollection services, Action<CareerTalentPipelineOptions> configureOptions)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }

            return services
                .Configure(configureOptions)
                .AddTransient<ICareerTalentPipelineService, CareerTalentPipelineService>();
        }

    }
}
