using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfileForRecruiter(Guid profileGuid, Guid subscriberGuid);
        Task UpdateProfileForRecruiter(ProfileDto profileDto, Guid subscriberGuid);
        Task<Guid> CreateProfile(ProfileDto profileDto);
        Task DeleteProfile(Guid profileGuid);
        Task UpdateAzureIndexStatus(Guid profileGuid, string azureIndexStatusName, string azureSearchIndexInfo);
        Task<List<string>> GetProfileEmailsByGuidList(List<Guid> profileGuids);
        Task<List<Profile>> GetProfilesByGuidList(List<Guid> profileGuids);
        #region ContactTypes

        Task<ContactTypeListDto> GetContactTypeList();

        Task<ContactTypeDto> GetContactTypeDetail(Guid contactTypeId);

        #endregion ContactTypes
    }
}
