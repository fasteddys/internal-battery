using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Services.Admin
{
    public class AccountManagementService: IAccountManagementService
    {

        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;
        private readonly ISubscriberService _subscriberService;

        public AccountManagementService(
            ILogger<AccountManagementService> logger,
            IRepositoryWrapper repository,
            IMapper mapper,
            ISubscriberService subscriberService
            )
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _subscriberService = subscriberService;
        }

        public async Task<UserStatsDto> GetUserStatsByEmail(string email)
        {
            _logger.LogInformation($"AccountManagementService:UpdateCandidateEmploymentPreference begin.");

            if (String.IsNullOrWhiteSpace(email))
                throw new FailedValidationException($"AccountManagementService:UpdateCandidateEmploymentPreference email cannot be null or empty.");
            var Subscriber = await _subscriberService.GetSubscriberByEmail(email);
            if (Subscriber == null)
                throw new NotFoundException($"Subscriber with email {email} does not exist exist");
            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:UpdateCandidateEmploymentPreference  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:UpdateCandidateEmploymentPreference end.");
            throw new NotImplementedException();
        }

        public async Task<bool> GetAuth0VerificationStatus(Guid subscriberGuid)
        {
            _logger.LogInformation($"AccountManagementService:UpdateCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"AccountManagementService:UpdateCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError($"AccountManagementService:UpdateCandidateEmploymentPreference  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"AccountManagementService:UpdateCandidateEmploymentPreference end.");
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
