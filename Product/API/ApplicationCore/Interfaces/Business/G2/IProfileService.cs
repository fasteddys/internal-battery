using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfile(Guid profileGuid);
        Task UpdateProfile(ProfileDto profile);
        Task<Guid> CreateProfile(ProfileDto profile);
        Task DeleteProfile(Guid profileGuid);
    }
}
