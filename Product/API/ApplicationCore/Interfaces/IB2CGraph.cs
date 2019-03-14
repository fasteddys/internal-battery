using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IB2CGraph
    {
        Task<IList<Group>> GetUserGroupsByObjectId(string objectId);
        Task<User> CreateUser(string name, string email, string password);
        Task<string> DisableUser(Guid subscriberGuid);
        Task<User> GetUserBySignInEmail(string email);
    }
}