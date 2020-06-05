using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Services.CrossChq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CrossChqExtensionMethods
    {
        public static IServiceCollection AddCrossChq(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }

            services.AddHttpClient(nameof(CrossChqWebClient), c =>
            {
                c.BaseAddress = new Uri(config["BaseAddress"]);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(config["ApiKey"]);
            });

            return services
                .AddTransient<ICrosschqService, CrosschqService>()
                .AddTransient<ICrossChqWebClient, CrossChqWebClient>();
        }
    }
}
