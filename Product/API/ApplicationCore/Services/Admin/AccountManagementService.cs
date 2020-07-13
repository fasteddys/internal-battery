using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Services.Admin
{
    public class AccountManagementService: IAccountManagementService
    {
        public async Task<UserStatsDto> GetUserStatsByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> GetAuth0VerificationStatus(Guid subscriber)
        {
            throw new NotImplementedException();
        }

        public async Task ForceVerification(Guid subscriberGuid)
        {
            throw new NotImplementedException();
        }

        public async Task SendVerificationEmail(Guid subscriberGuid)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAccount(Guid subscriberGuid)
        {
            throw new NotImplementedException();
        }
    }
}
