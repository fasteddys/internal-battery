using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfileForRecruiter(Guid profileGuid, Guid subscriberGuid);
        Task UpdateProfileForRecruiter(ProfileDto profileDto, Guid subscriberGuid);
        Task<Guid> CreateProfile(ProfileDto profileDto);
        Task DeleteProfile(Guid profileGuid);
    }
}
