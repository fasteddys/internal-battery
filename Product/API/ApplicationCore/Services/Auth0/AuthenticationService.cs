using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth0.AuthenticationApi.Models;
using UpDiddyApi.ApplicationCore.Services.Auth0.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services.Auth0
{
    public class AuthenticationService : IAuthenticationService
    {
        public Task<AccessTokenResponse> CreateAccessTokenAsync(string email, string password)
        {
            throw new NotImplementedException();
        }
    }
}
