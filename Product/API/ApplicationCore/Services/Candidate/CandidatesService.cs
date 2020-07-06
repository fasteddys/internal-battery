using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISubscriberService _subscriberService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IHubSpotService _hubSpotService;

        public CandidatesService(
            ILogger<CandidatesService> logger,
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ISubscriberService subscriberService,
            IHubSpotService hubSpotService
        )
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _subscriberService = subscriberService;
            _hubSpotService = hubSpotService;
        }

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
                    if (candidateState == null)
                    {
                        //add state if not recognised - assume its USA.
                        var country = await _repositoryWrapper.Country.GetCountryByCode3("USA");
                        await _repositoryWrapper.State.AddUSState(new State
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            StateGuid = Guid.NewGuid(),
                            Code = candidatePersonalInfoDto?.State,
                            CountryId = country.CountryId,
                            Name = candidatePersonalInfoDto?.State //Name will the new state code.
                                                                   //Sequence will default to 0
                        });

                        candidateState = await _repositoryWrapper.State.GetUSCanadaStateByCode(candidatePersonalInfoDto.State.Trim());
                        if (candidateState == null)
                            throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference newly added state:{candidatePersonalInfoDto.State}, not found.");
                    }
                }
                await _repositoryWrapper.SubscriberRepository.UpdateSubscriberPersonalInfo(subscriberGuid, candidateState, candidatePersonalInfoDto);

                // Call Hubspot to update the following properties which are a part of 'personal information': FirstName, LastName
                await _hubSpotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid);
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
            if (candidateEmploymentPreferenceDto == null)
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

                // Call Hubspot to update the following properties which are a part of 'role preferences': SelfCuratedSkills
                await _hubSpotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:UpdateRolePreference: Error while updating Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        #endregion Role Preferences

        #region Skills

        public async Task<SkillListDto> GetSkills(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NotFoundException("subscriberGuid cannot be null or empty");

            var candidateSkills = await _repositoryWrapper.SubscriberSkillRepository.GetCandidateSkills(subscriberGuid, limit, offset, sort, order);

            return _mapper.Map<SkillListDto>(candidateSkills);
        }
        public async Task UpdateSkills(Guid subscriberGuid, List<string> skillNames)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NotFoundException("subscriberGuid cannot be null or empty");

            await _repositoryWrapper.SubscriberSkillRepository.UpdateCandidateSkills(subscriberGuid, skillNames);
        }

        #endregion

        #region Language Proficiencies

        public async Task<LanguageListDto> GetLanguageList()
        {
            var languages = await _repositoryWrapper.SubscriberRepository.GetLanguages();
            return _mapper.Map<LanguageListDto>(languages);
        }

        public async Task<ProficiencyLevelListDto> GetProficiencyLevelList()
        {
            var proficiencyLevels = await _repositoryWrapper.SubscriberRepository.GetProficiencyLevels();
            return _mapper.Map<ProficiencyLevelListDto>(proficiencyLevels);
        }

        public async Task<LanguageProficiencyListDto> GetLanguageProficiencies(Guid subscriberGuid)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetLanguagesAndProficiencies: Fetching Candidate 360 languages and proficiencies for subscriber {subscriber}", subscriberGuid);

                var languageProficiencyLevels = await _repositoryWrapper.SubscriberRepository.GetSubscriberLanguageProficiencies(subscriberGuid);
                var languageProficiencyListDto = _mapper.Map<LanguageProficiencyListDto>(languageProficiencyLevels);
                _logger.LogDebug("CandidatesService:GetLanguagesAndProficiencies: Returning Candidate 360 languages and proficiencies for subscriber {subscriber}", subscriberGuid);

                return languageProficiencyListDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetLanguagesAndProficiencies: Error while fetching Candidate 360 languages and proficiencies for subscriber {subscriber}", subscriberGuid);
                throw;
            }
        }

        public async Task<Guid> CreateLanguageProficiency(LanguageProficiencyDto languageProficiency, Guid subscriberGuid)
        {
            if (languageProficiency == null) { throw new ArgumentNullException(nameof(languageProficiency)); }

            try
            {
                _logger.LogDebug("CandidatesService:CreateLanguageProficiency: Creating Candidate 360 languages and proficiencies for subscriber {subscriber}", subscriberGuid);

                var languageProficiencyGuid = await _repositoryWrapper.SubscriberRepository.CreateSubscriberLanguageProficiency(languageProficiency, subscriberGuid);

                _logger.LogDebug("CandidatesService:CreateLanguageProficiency: Created Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", languageProficiencyGuid, subscriberGuid);

                return languageProficiencyGuid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:CreateLanguageProficiency: Error while creating Candidate 360 languages and proficiencies for subscriber {subscriber}", subscriberGuid);
                throw;
            }
        }

        public async Task UpdateLanguageProficiency(LanguageProficiencyDto languageProficiency, Guid languageProficiencyGuid, Guid subscriberGuid)
        {
            if (languageProficiency == null) { throw new ArgumentNullException(nameof(languageProficiency)); }

            try
            {
                _logger.LogDebug("CandidatesService:UpdateLanguageProficiency: Updating Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", languageProficiencyGuid, subscriberGuid);

                languageProficiency.LanguageProficiencyGuid = languageProficiencyGuid;
                await _repositoryWrapper.SubscriberRepository.UpdateSubscriberLanguageProficiency(languageProficiency, subscriberGuid);

                _logger.LogDebug("CandidatesService:UpdateLanguageProficiency: Updated Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", languageProficiencyGuid, subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:UpdateLanguageProficiency: Error while updating Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", languageProficiencyGuid, subscriberGuid);
                throw;
            }
        }

        public async Task DeleteLanguageProficiency(Guid languageProficiencyGuid, Guid subscriberGuid)
        {
            try
            {
                _logger.LogDebug("CandidatesService:DeleteLanguageProficiency: Deleting Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", subscriberGuid);

                await _repositoryWrapper.SubscriberRepository.DeleteSubscriberLanguageProficiency(languageProficiencyGuid, subscriberGuid);

                _logger.LogDebug("CandidatesService:DeleteLanguageProficiency: Updated Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:DeleteLanguageProficiency: Error while updating Candidate 360 language and proficiency {languageProficiency} for subscriber {subscriber}", subscriberGuid);
                throw;
            }
        }

        #endregion Language Proficiencies

        #region CompensationPreferences

        public async Task<CompensationPreferencesDto> GetCompensationPreferences(Guid subscriberGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null) { throw new InsufficientPermissionException("Unable to find subscriber record"); }

            return new CompensationPreferencesDto
            {
                CurrentRate = subscriber.CurrentRate,
                CurrentSalary = subscriber.CurrentSalary,
                DesiredRate = subscriber.DesiredRate,
                DesiredSalary = subscriber.DesiredSalary
            };
        }

        public async Task UpdateCompensationPreferences(CompensationPreferencesDto compensationPreferences, Guid subscriberGuid)
        {
            if (compensationPreferences == null) { throw new ArgumentNullException(nameof(compensationPreferences)); }
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null) { throw new InsufficientPermissionException("Unable to find subscriber record"); }

            subscriber.CurrentRate = compensationPreferences.CurrentRate;
            subscriber.CurrentSalary = compensationPreferences.CurrentSalary;
            subscriber.DesiredRate = compensationPreferences.DesiredRate;
            subscriber.DesiredSalary = compensationPreferences.DesiredSalary;
            await _repositoryWrapper.SubscriberRepository.SaveAsync();
        }

        #endregion CompensationPreferences
    }
}
