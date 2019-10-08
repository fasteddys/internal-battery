using Auth0.AuthenticationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Services.Auth0.Communication;

namespace UpDiddyApi.ApplicationCore.Services.Auth0.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(User user, params Role[] userRoles);
        Task<User> FindByEmailAsync(string email);
    }
}
