using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Services.AzureAPIManagement;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IAPIGateway
    {
        Task<string> GetUserIdAsync(string subscriptionId, string key);
        Task<User> GetUserAsync(string userId);
    }
}
