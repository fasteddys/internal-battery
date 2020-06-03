using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Services.G2.CrossChq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CrossChqExtensionMethods
    {
        public static IServiceCollection AddCrossChq(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }

            services.AddHttpClient(nameof(CrossChqWebClient), c =>
            {
                c.BaseAddress = new Uri(config["baseAddress"]);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(config["apiKey"]);
            });

            return services
                .AddTransient<ICrossChqWebClient, CrossChqWebClient>();
        }
    }
}
