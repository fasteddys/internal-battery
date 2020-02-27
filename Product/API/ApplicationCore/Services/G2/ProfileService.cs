using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services.G2
{
    public class ProfileService : IProfileService
    {
        public Task<Guid> CreateProfile(ProfileDto profile)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProfile(Guid profileGuid)
        {
            throw new NotImplementedException();
        }

        public Task<ProfileDto> GetProfile(Guid profileGuid)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProfile(ProfileDto profile)
        {
            throw new NotImplementedException();
        }
    }
}
