using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IProfileRepository : IUpDiddyRepositoryBase<Profile>
    {
        Task<Profile> GetProfileForRecruiter(Guid profileGuid, Guid subscriberGuid);
        Task UpdateProfileForRecruiter(ProfileDto profileDto, Guid subscriberGuid);
        Task DeleteProfile(Guid profileGuid);
        Task<Guid> CreateProfile(ProfileDto profileDto);
        Task UpdateAzureIndexStatus(Guid profileGuid, Guid azureIndexStatusGuid, string azureSearchIndexInfo);

        Task<List<Profile>> GetProfilesByGuidList(List<Guid> profilesGuids);

    }
}
