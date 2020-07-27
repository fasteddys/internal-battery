using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IAccountManagementService
    {
        Task<UserStatsDto> GetUserStatsByEmail(string email);

        Task<bool> GetAuth0VerificationStatus(Guid subscriber);

        Task ForceVerification(Guid subscriberGuid);

        Task SendVerificationEmail(Guid subscriberGuid);

        Task<EmailStatisticsListDto> GetEmailStatistics(Guid subscriberGuid);

        Task RemoveAccount(Guid subscriberGuid);
    }
}
