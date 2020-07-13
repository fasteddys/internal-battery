using System;
using System.Threading.Tasks;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IAccountManagementService
    {
        Task<UserStatsDto> GetUserStatsByEmail(string email);

        Task<UserStatsListDto> GetB2BUsersList(int limit, int offset, string sort, string order);

        Task ForceVerification(Guid subscriberGuid);

        Task SendVerificationEmail(Guid subscriberGuid);

        Task RemoveAccount(Guid subscriberGuid);
    }
}
