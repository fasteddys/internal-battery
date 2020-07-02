using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models.Candidate360;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberRepository : IUpDiddyRepositoryBase<Subscriber>
    {
        IQueryable<Subscriber> GetAllSubscribersAsync();
        Task<SubscriberSourceDto> GetSubscriberSource(int subscriberId);
        Task<List<SubscriberEmploymentTypes>> GetCandidateEmploymentPreferencesBySubscriberGuidAsync(Guid subscriberGuid);
        Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid);
        Task<Subscriber> GetSubscriberPersonalInfoByGuidAsync(Guid subscriberGuid);
        Subscriber GetSubscriberByGuid(Guid subscriberGuid);
        Task<Subscriber> GetSubscriberByEmailAsync(string email);
        Subscriber GetSubscriberByEmail(string email);
        Task<Subscriber> GetSubscriberByIdAsync(int subscriberId);
        Task<IList<Partner>>  GetPartnersAssociatedWithSubscriber(int subscriberId);
        Task<int> GetSubscribersCountByStartEndDates(DateTime? startDate = null, DateTime? endDate = null);
        Task UpdateHubSpotDetails(Guid subscriberId, long hubSpotVid);
        Task UpdateHubSpotDetails(int subscriberId, long hubSpotVid);
        Task UpdateCandidateEmploymentPreferencesBySubscriberGuidAsync(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto);
        Task UpdateSubscriberPersonalInfo(Guid subscriberGuid, State subscriberState, CandidatePersonalInfoDto candidatePersonalInfoDto);
        Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid);
        Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference);
        Task UpdateEmailVerificationStatus(string email, bool isVerified);
    }
}