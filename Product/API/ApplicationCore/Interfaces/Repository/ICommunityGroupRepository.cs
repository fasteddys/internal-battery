using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICommunityGroupRepository : IUpDiddyRepositoryBase<CommunityGroup>
    {
        IQueryable<CommunityGroup> GetAllCommunityGroupsAsync();

        Task<CommunityGroup> GetCommunityGroupByNameAsync(string name);
        Task<CommunityGroup> GetCommunityGroupByGuidAsync(Guid communityGroupGuid);

       
    }
}
