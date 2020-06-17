using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
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

        public async Task<Candidate360RoleDto> GetCandidate360Role(Guid subscriberGuid)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Fetching Candidate 360 Role information for {subscriber}", subscriberGuid);

                var candidate360Role = await _repositoryWrapper.SubscriberRepository.GetCandidate360Role(subscriberGuid);
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Returning Candidate 360 Role information for {subscriber}", subscriberGuid);

                return candidate360Role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetCandidate360Role: Error while fetching Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        public async Task UpdateCandidate360Role(Guid subscriberGuid, Candidate360RoleDto candidate360Role)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Updating Candidate 360 Role information for {subscriber}", subscriberGuid);

                await _repositoryWrapper.SubscriberRepository.UpdateCandidate360Role(subscriberGuid, candidate360Role);
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Updated Candidate 360 Role information for {subscriber}", subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetCandidate360Role: Error while updating Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        #endregion Role Preferences
    }
}
