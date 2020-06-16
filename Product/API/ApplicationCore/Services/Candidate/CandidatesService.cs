using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;


        public CandidatesService(
            ILogger<CandidatesService> logger,
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper
            )
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        // This empty service class will be used for stories:
        //   #2480 - Candidate 360: Personal Info
        //   #2481 - Candidate 360: Employment Preferences
        //   #2482 - Candidate 360: Role Preferences

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

        #endregion Role Preferences
    }
}
