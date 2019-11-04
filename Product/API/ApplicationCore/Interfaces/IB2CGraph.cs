using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    [Obsolete("This can be removed once we are satisfied with the ADB2C -> Auth0 migration", false)]
    public interface IB2CGraph
    {
        Task<IList<Group>> GetUserGroupsByObjectId(string objectId);
        Task<User> CreateUser(string name, string email, string password);
        Task<string> DisableUser(Guid subscriberGuid);
        Task<User> GetUserBySignInEmail(string email);
        Task<string> SendGraphPostRequest(string api, string json);
        Task<string> SendGraphGetRequest(string api, string query);
        Task<string> SendGraphDeleteRequest(string api);
    }
}