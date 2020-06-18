using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISubscriberService _subscriberService;


        public CandidatesService(
            ILogger<CandidatesService> logger,
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ISubscriberService subscriberService
        )
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _subscriberService = subscriberService;
        }

        // This empty service class will be used for stories:
        //   #2480 - Candidate 360: Personal Info
        //   #2481 - Candidate 360: Employment Preferences
        //   #2482 - Candidate 360: Role Preferences

        #region Personal Info
        public async Task<CandidatePersonalInfoDto> GetCandidatePersonalInfo(Guid subscriberGuid)
        {
            _logger.LogInformation($"CandidatesService:GetCandidatePersonalInfo begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:GetCandidatePersonalInfo subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            try
            {
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberPersonalInfoByGuidAsync(subscriberGuid);
                return _mapper.Map<CandidatePersonalInfoDto>(subscriber);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidatePersonalInfo  Error: {ex.ToString()} ");
                throw ex;
            }

        }

        public async Task UpdateCandidatePersonalInfo(Guid subscriberGuid, CandidatePersonalInfoDto candidatePersonalInfoDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidatePersonalInfo begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:UpdateCandidatePersonalInfo subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            if (candidatePersonalInfoDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference candidatePersonalInfoDto cannot be null");

            try
            {
                Models.State candidateState = null;
                if (!String.IsNullOrWhiteSpace(candidatePersonalInfoDto.State) && candidatePersonalInfoDto.State.Trim().Length == 2)
                {
                    candidateState = await _repositoryWrapper.State.GetUSCanadaStateByCode(candidatePersonalInfoDto.State.Trim());
                    //add the missing address and then return
                    if(candidateState == null)
                    {
                        //add state if not recognised - assume its USA.
                        var country = await _repositoryWrapper.Country.GetCountryByCode3("USA");
                        await _repositoryWrapper.State.AddUSState(new State { 
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                StateGuid = Guid.NewGuid(),
                                Code = candidatePersonalInfoDto?.State,
                                CountryId = country.CountryId,
                                Name = candidatePersonalInfoDto?.State //Name will the new state code.
                                //Sequence will default to 0
                        });

                        candidateState = await _repositoryWrapper.State.GetUSCanadaStateByCode(candidatePersonalInfoDto.State.Trim());
                        if(candidateState==null) 
                            throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference newly added state:{candidatePersonalInfoDto.State}, not found.");
                    }
                }
                await _repositoryWrapper.SubscriberRepository.UpdateSubscriberPersonalInfo(subscriberGuid, candidateState, candidatePersonalInfoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:UpdateCandidatePersonalInfo  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:UpdateCandidatePersonalInfo begin.");

        }
        #endregion Personal Info

        #region Employment Preferences

        public async Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:GetCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
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

        }

        public async Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty) 
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");
            if(candidateEmploymentPreferenceDto == null) 
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference candidateEmploymentPreferenceDto cannot be null");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
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
