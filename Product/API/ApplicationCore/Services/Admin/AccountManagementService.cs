using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto.User;
using Auth0.ManagementApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.Admin
{
    public class AccountManagementService: IAccountManagementService
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
                userStatsDto.IsHiringManager = userStatsDto != null;

                return userStatsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:GetUserStatsByEmail  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:GetUserStatsByEmail end.");
            throw new NotImplementedException();
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

                return subscriber.EmailVerification != null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:GetAuth0VerificationStatus  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:GetAuth0VerificationStatus end.");
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
