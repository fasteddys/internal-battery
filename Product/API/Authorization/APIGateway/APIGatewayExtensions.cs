using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Authorization.APIGateway
{
    public static class APIGatewayExtensions
    {
        public static AuthenticationBuilder AddAPIGatewayAuth(this AuthenticationBuilder builder, Action<APIGatewayOptions> configureOptions)
        {
            return builder.AddScheme<APIGatewayOptions, APIGatewayAuthHandler>(APIGatewayDefaults.AuthenticationScheme, APIGatewayDefaults.DisplayName, configureOptions);
        }
    }
}
