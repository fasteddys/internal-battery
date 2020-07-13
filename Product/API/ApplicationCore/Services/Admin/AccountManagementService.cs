using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto.User;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.InteropServices;

namespace UpDiddyApi.ApplicationCore.Services.Admin
{
    public class AccountManagementService : IAccountManagementService
    {

        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;

        public AccountManagementService(
            ILogger<AccountManagementService> logger,
            IRepositoryWrapper repository,
            IMapper mapper
            )
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

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
            await DeleteG2Profile(subscriberGuid);
            await DeleteB2BProfile(subscriberGuid);
            await DeleteSubscriber(subscriberGuid);

            /* TODO:
             * 1. Update Azure Index
             * 2. HubSpot Remove contact
             * 3. CloudTalent Remove Profile
             * 4. Traitify remove account
             */

            // Other TODOs before I forget:  Creating a new subscriber with same email:  test that it works!
        }

        private async Task DeleteG2Profile(Guid subscriberGuid)
        {
            var g2 = await _repository.ProfileRepository.GetAllWithTracking()
                .Include(profile => profile.Subscriber)
                .SingleOrDefaultAsync(profile =>
                    profile.IsDeleted == 0 &&
                    profile.Subscriber.IsDeleted == 0 &&
                    profile.Subscriber.SubscriberGuid == subscriberGuid);

            if (g2 != null)
            {
                _repository.ProfileRepository.LogicalDelete(g2);
                await _repository.ProfileRepository.SaveAsync();
            }
        }

        private async Task DeleteB2BProfile(Guid subscriberGuid)
        {
            var b2b = await _repository.HiringManagerRepository.GetAllWithTracking()
                .Include(hm => hm.Subscriber)
                .SingleOrDefaultAsync(hm =>
                    hm.IsDeleted == 0 &&
                    hm.Subscriber.IsDeleted == 0 &&
                    hm.Subscriber.SubscriberGuid == subscriberGuid);

            if (b2b != null)
            {
                _repository.HiringManagerRepository.LogicalDelete(b2b);
                await _repository.HiringManagerRepository.SaveAsync();
            }
        }

        private async Task DeleteSubscriber(Guid subscriberGuid)
        {
            var subscriber = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber != null)
            {
                _repository.SubscriberRepository.LogicalDelete(subscriber);
                await _repository.SubscriberRepository.SaveAsync(); 
            }
        }
    }
}
