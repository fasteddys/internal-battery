using Auth0.AuthenticationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.Auth0.Interfaces
{
    public interface IUserService
    {
        Task<AccessTokenResponse> CreateUserAsync(User user, params Role[] userRoles);
        Task<User> FindByEmailAsync(string email);
    }
}
