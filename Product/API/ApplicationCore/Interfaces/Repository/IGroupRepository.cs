using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IGroupRepository : IUpDiddyRepositoryBase<Group>
    {
        Task<Group> GetGroupByName(string Name);
        Task<Group> GetGroupByGuid(Guid groupGuid);
        Task<Group> GetDefaultGroup();
    }
}
