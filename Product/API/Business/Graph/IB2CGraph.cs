using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Business.Graph
{
    public interface IB2CGraph
    {
        Task<IList<Group>> GetUserGroupsByObjectId(string objectId);
        Task<string> GetUserByObjectId(string objectId);
        Task<string> AddUserToGroup(string objectId, string groupId);
        Task<string> RemoveUserFromGroup(string objectId, string groupId);
    }
}
