using Auth0.ManagementApi.Models;
using AutoMapper;
using Braintree;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.Admin
{
    public class AccountManagementService : IAccountManagementService
    {

        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IG2Service _g2Service;
        private readonly IHubSpotService _hubSpotService;
        private readonly IHangfireService _hangfireService;
        private readonly ITraitifyService _traitifyService;
        private readonly ISysEmail _emailService;
        private readonly IMapper _mapper;
        private IUserService _userService { get; set; }
        private readonly IConfiguration _configuration;


        public AccountManagementService(
            ILogger<AccountManagementService> logger,
            IRepositoryWrapper repository,
            IG2Service g2Service,
            IHubSpotService hubSpotService,
            IHangfireService hangfireService,
            ITraitifyService traitifyService,
            ISysEmail emailService,
            IMapper mapper,
            IUserService userService,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _repository = repository;
            _g2Service = g2Service;
            _hubSpotService = hubSpotService;
            _hangfireService = hangfireService;
            _traitifyService = traitifyService;
            _emailService = emailService;
            _mapper = mapper;
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<UserStatsDto> GetUserStatsByEmail(string email)
        {
            _logger.LogInformation($"AccountManagementService:GetUserStatsByEmail begin.");
            try
            {
                if (String.IsNullOrWhiteSpace(email))
                    throw new FailedValidationException($"AccountManagementService:GetUserStatsByEmail email cannot be null or empty.");
                var subscriber = _repository.SubscriberRepository.GetSubscriberByEmail(email);
                if (subscriber == null)
                    throw new NotFoundException($"Subscriber with email {email} does not exist exist");

                var subscriberAccountDetails = await _repository.SubscriberRepository.GetSubscriberAccountDetailsByGuidAsync(subscriber.SubscriberGuid.Value);
                var hiringManager = await _repository.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId);

                var userStatsDto = _mapper.Map<UserStatsDto>(subscriberAccountDetails);

                //mapped outside of automapper
                userStatsDto.IsHiringManager = hiringManager != null;

                return userStatsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:GetUserStatsByEmail  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:GetUserStatsByEmail end.");
        }

        public async Task<bool> GetAuth0VerificationStatus(Guid subscriberGuid)
        {
            _logger.LogInformation($"AccountManagementService:GetAuth0VerificationStatus begin.");
            try
            {
                if (subscriberGuid == Guid.Empty)
                    throw new FailedValidationException($"AccountManagementService:GetAuth0VerificationStatus subscriber guid cannot be empty({subscriberGuid})");
                var subscriber = await _repository.SubscriberRepository.GetSubscriberAccountDetailsByGuidAsync(subscriberGuid);
                if (subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
                var user = await _userService.GetUserByEmailAsync(subscriber.Email);

                return user.User?.EmailVerified ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:GetAuth0VerificationStatus  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:GetAuth0VerificationStatus end.");
        }

        public async Task ForceVerification(Guid subscriberGuid)
        {

            _logger.LogInformation($"AccountManagementService:ForceVerification begin.");
            try
            {
                if (subscriberGuid == Guid.Empty)
                    throw new FailedValidationException($"AccountManagementService:ForceVerification subscriber guid cannot be empty({subscriberGuid})");
                var subscriber = await _repository.SubscriberRepository.GetSubscriberAccountDetailsByGuidAsync(subscriberGuid);
                if (subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

                _logger.LogInformation($"AccountManagementService:ForceVerification invoking hangfire job for userService.ResendVerificationEmailToUserAsync.");

                //To test use direct service call
                //_userService.ResetAccountVerificationFlagForUserAsync(subscriber.Email);

                _hangfireService.Enqueue(() => _userService.SetEmailVerificationFlagForUserAsync(subscriber.Email));
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:ForceVerification  Error: {ex.ToString()}");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:ForceVerification end.");
        }

        public async Task SendVerificationEmail(Guid subscriberGuid)
        {
            _logger.LogInformation($"AccountManagementService:SendVerificationEmail begin.");
            try
            {
                if (subscriberGuid == Guid.Empty)
                    throw new FailedValidationException($"AccountManagementService:SendVerificationEmail subscriber guid cannot be empty({subscriberGuid})");
                var subscriber = await _repository.SubscriberRepository.GetSubscriberAccountDetailsByGuidAsync(subscriberGuid);
                if (subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

                _logger.LogInformation($"AccountManagementService:SendVerificationEmail invoking hangfire job for userService.ResendVerificationEmailToUserAsync.");
                
                //To test use direct service call
                //_userService.ResendVerificationEmailToUserAsync(subscriber.Email);
                _hangfireService.Enqueue(() => _userService.ResendVerificationEmailToUserAsync(subscriber.Email));
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:SendVerificationEmail  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:SendVerificationEmail end.");
        }

        public async Task<EmailStatisticsListDto> GetEmailStatistics(
            Guid subscriberGuid,
            TimeSpan range,
            string sort,
            string order,
            int limit,
            int offset)
        {
            var subscriber = await _repository.SubscriberRepository
                .GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber == null)
            {
                throw new NotFoundException("No subscriber for the given subscriberGuid found");
            }

            return await _repository.StoredProcedureRepository
                .GetEmailStatistics(subscriber.Email, range, sort, order, limit, offset);
        }

        public async Task RemoveAccount(Guid subscriberGuid)
        {
            await DeleteG2Profile(subscriberGuid);
            await DeleteB2BProfile(subscriberGuid);
            await RemoveAzureIndex(subscriberGuid);
            await RemoveHubSpotContact(subscriberGuid);
            RemoveFromTalentCloud(subscriberGuid);

            var subscriber = await _repository.SubscriberRepository
                .GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber != null)
            {
                await RemoveAuth0Account(subscriber);
                await DeleteSubscriber(subscriber);
                NotifyUser(subscriber);
            }
        }

        private async Task DeleteG2Profile(Guid subscriberGuid)
            => await _repository.StoredProcedureRepository
                .DeleteSubscriberG2Profiles(subscriberGuid);

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

        private async Task DeleteSubscriber(Subscriber subscriber)
        {
            _repository.SubscriberRepository.LogicalDelete(subscriber);
            await _repository.SubscriberRepository.SaveAsync();
        }

        private async Task RemoveAuth0Account(Subscriber subscriber)
            => await _userService.DeleteUserAsync(subscriber.Auth0UserId);

        private async Task RemoveAzureIndex(Guid subscriberGuid)
            => await _g2Service.G2IndexRemoveSubscriberAsync(subscriberGuid);

        private async Task RemoveHubSpotContact(Guid subscriberGuid)
            => await _hubSpotService.RemoveContactBySubscriberGuid(subscriberGuid);

        private void RemoveFromTalentCloud(Guid subscriberGuid)
            => _hangfireService.Enqueue<ScheduledJobs>(j =>
                j.CloudTalentDeleteProfile(subscriberGuid, null));

        private void NotifyUser(Subscriber subscriber)
            => _hangfireService.Enqueue(() => _emailService.SendTemplatedEmailAsync(
                new[] { subscriber.Email },
                _configuration["SysEmail:Transactional:TemplateIds:AccountDeletionNotification"],
                null,
                Constants.SendGridAccount.Transactional,
                null,
                null,
                null,
                null,
                null,
                _configuration.GetValue<string[]>("Admin:ccEmail")));
    }
}
