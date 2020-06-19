using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;


        public CandidatesService(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILogger<CandidatesService> logger
            )
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        #region Personal Info

        #endregion Personal Info

        #region Employment Preferences

        public async Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:GetCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");

            try
            {
                var subscriberEmploymentTypes = await _repositoryWrapper.SubscriberRepository.GetCandidateEmploymentPreferencesBySubscriberGuidAsync(subscriberGuid);

                if (subscriberEmploymentTypes == null)
                {
                    throw new FailedValidationException($"CandidatesService:GetCandidateEmploymentPreference Cannot locate subscriber: {subscriberGuid}");
                }

                return _mapper.Map<CandidateEmploymentPreferenceDto>(subscriberEmploymentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidateEmploymentPreference  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CandidatesService:GetCandidateEmploymentPreference end.");

        }

        public async Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty) 
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");
            if(candidateEmploymentPreferenceDto == null) 
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference candidateEmploymentPreferenceDto cannot be null");

            try
            {
                await _repositoryWrapper.SubscriberRepository.UpdateCandidateEmploymentPreferencesBySubscriberGuidAsync(subscriberGuid, candidateEmploymentPreferenceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:UpdateCandidateEmploymentPreference  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CandidatesService:UpdateCandidateEmploymentPreference end.");
        }
        #endregion Employment Preferences

        #region Role Preferences

        public async Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetRolePreference: Fetching Candidate 360 Role information for {subscriber}", subscriberGuid);

                var rolePreference = await _repositoryWrapper.SubscriberRepository.GetRolePreference(subscriberGuid);
                _logger.LogDebug("CandidatesService:GetRolePreference: Returning Candidate 360 Role information for {subscriber}", subscriberGuid);

                return rolePreference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetRolePreference: Error while fetching Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        public async Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference)
        {
            if (rolePreference == null) { throw new ArgumentNullException(nameof(rolePreference)); }

            var hasDuplicates = rolePreference.SocialLinks
                .GroupBy(sl => sl.FriendlyName)
                .Any(sl => sl.Count() > 1);

            if (hasDuplicates) { throw new FailedValidationException("Cannot specify more than one social link of the same type"); }

            try
            {
                _logger.LogDebug("CandidatesService:UpdateRolePreference: Updating Candidate 360 Role information for {subscriber}", subscriberGuid);

                await _repositoryWrapper.SubscriberRepository.UpdateRolePreference(subscriberGuid, rolePreference);
                _logger.LogDebug("CandidatesService:UpdateRolePreference: Updated Candidate 360 Role information for {subscriber}", subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:UpdateRolePreference: Error while updating Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        #endregion Role Preferences
    }
}
