using Auth0.AuthenticationApi.Models;
using System;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.Auth0.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AccessTokenResponse> CreateAccessTokenAsync(string email, string password);
    }
}
