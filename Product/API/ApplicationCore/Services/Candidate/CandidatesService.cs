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
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models.Views;
using UpDiddyLib.Domain.AzureSearchDocuments;
using GeoJSON.Net.Geometry;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Services.AzureSearch;
using UpDiddyLib.Helpers;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISubscriberService _subscriberService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IHubSpotService _hubSpotService;
        private readonly IConfiguration _configuration;
        private readonly UpDiddyDbContext _db;
        private readonly IHangfireService _hangfireService;
        private readonly IAzureSearchService _azureSearchService;
        private readonly string _hostUrl;
        private readonly string _publicKey;

        public CandidatesService(
            ILogger<CandidatesService> logger,
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ISubscriberService subscriberService,
            IHubSpotService hubSpotService,
            IConfiguration configuration,
            UpDiddyDbContext context,
           IHangfireService hangfireService,
           IAzureSearchService azureSearchService
        )
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _subscriberService = subscriberService;
            _hubSpotService = hubSpotService;
            _configuration = configuration;
            _db = context;
            _hangfireService = hangfireService;
            _azureSearchService = azureSearchService;
            _publicKey = configuration["Traitify:PublicKey"];
            _hostUrl = configuration["Traitify:HostUrl"];
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
                // update candidate index
                await IndexCandidateBySubscriberAsync(subscriberGuid);

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
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetCandidateEmploymentPreferencesBySubscriberGuidAsync(subscriberGuid);

                return _mapper.Map<CandidateEmploymentPreferenceDto>(subscriber);
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
                // update candidate index
                await IndexCandidateBySubscriberAsync(subscriberGuid);
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
                // update candidate index
                await IndexCandidateBySubscriberAsync(subscriberGuid);
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
            // update candidate index
            await IndexCandidateBySubscriberAsync(subscriberGuid);
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
                // update candidate index
                await IndexCandidateBySubscriberAsync(subscriberGuid);

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

        #region Education & Training

        public async Task<EducationalDegreeTypeListDto> GetAllEducationalDegrees(int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetAllEducationalDegrees begin.");

            try
            {
                var educationalDegrees = await _repositoryWrapper.EducationalDegreeTypeRepository.GetAllDefinedEducationDegreeTypes(limit, offset, sort, order);
                if (educationalDegrees == null) return null;
                return _mapper.Map<EducationalDegreeTypeListDto>(educationalDegrees);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetAllEducationalDegrees  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetAllEducationalDegrees end.");

        }

        public async Task<SubscriberEducationHistoryDto> GetCandidateEducationHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateEducationHistory begin.");
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:GetCandidateEducationHistory subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

            try
            {
                var candidateEducationHistory = await _repositoryWrapper.SubscriberEducationHistoryRepository.GetEducationalHistoryBySubscriberGuid(subscriberGuid, limit, offset, sort, order);
                return _mapper.Map<SubscriberEducationHistoryDto>(candidateEducationHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidateEducationHistory  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetCandidateEducationHistory end.");
        }

        public async Task<TrainingTypesDto> GetAllTrainingTypes(int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetAllTrainingTypes begin.");

            try
            {
                var trainingTypes = await _repositoryWrapper.TrainingTypesRepository.GetAllTrainingTypes(limit, offset, sort, order);
                return _mapper.Map<TrainingTypesDto>(trainingTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetAllTrainingTypes  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetAllTrainingTypes end.");
        }

        public async Task<SubscriberTrainingHistoryDto> GetCandidateTrainingHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateTrainingHistory begin.");
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:GetCandidateTrainingHistory subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

            try
            {
                var candidateTrainingHistory = await _repositoryWrapper.SubscriberRepository.GetCandidateTrainingHistory(subscriberGuid, limit, offset, sort, order);
                if (candidateTrainingHistory == null) return null;
                return _mapper.Map<SubscriberTrainingHistoryDto>(candidateTrainingHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidateTrainingHistory  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetCandidateTrainingHistory end.");
        }

        public async Task UpdateCandidateEducationAndTraining(Guid subscriberGuid, SubscriberEducationAssessmentsDto subscriberEducationAssessmentsDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidateEducationAndTraining begin.");
            if (subscriberEducationAssessmentsDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEducationAndTraining subscriberEducationAssessmentsDto cannot be null (subscriberGuid:{subscriberGuid})");

            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEducationAndTraining subscriber guid cannot be empty({subscriberGuid})");
            if (subscriberEducationAssessmentsDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEducationAndTraining candidateEmploymentPreferenceDto cannot be null");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

            try
            {
                await _repositoryWrapper.SubscriberEducationHistoryRepository.UpdateCandidateEducationAndTraining(subscriberGuid, subscriberEducationAssessmentsDto);
                // update candidate index
                await IndexCandidateBySubscriberAsync(subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:UpdateCandidateEducationAndTraining  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CandidatesService:UpdateCandidateEducationAndTraining end.");
        }

        public async Task<AssessmentsDto> GetAssessments(Guid subscriber)
        {
            var traitify = await _repositoryWrapper.TraitifyRepository.GetMostRecentAssessmentBySubscriber(subscriber);
            var traitifyDto = _mapper.Map<TraitifyDto>(traitify);
            if (traitifyDto != null)
            {
                traitifyDto.HostUrl = _hostUrl;
                traitifyDto.PublicKey = _publicKey;
            }
            var isTraitifyAssessmentsVisibleToHiringManagers = (await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriber)).IsTraitifyAssessmentsVisibleToHiringManagers;
            return new AssessmentsDto()
            {
                Traitify = traitifyDto,
                IsTraitifyAssessmentsVisibleToHiringManagers = isTraitifyAssessmentsVisibleToHiringManagers.HasValue ? isTraitifyAssessmentsVisibleToHiringManagers.Value : false
            };
        }

        #endregion



        #region Candidate Indexing

        /// <summary>
        /// For the given subscriber, update or add their profile to the Candidate azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> IndexCandidateBySubscriberAsync(Guid subscriberGuid, bool nonBlocking = true)
        {

            _logger.LogInformation($"CandidateService:IndexCandidateBySubscriberAsync Starting subscriber = {subscriberGuid}  nonBlocking = {nonBlocking}");
              
            // fire off as background job 
            if (nonBlocking)
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CandidateIndexAddOrUpdate1(subscriberGuid));
            else
                await CandidateIndexAsync(subscriberGuid);

            _logger.LogInformation($"CandidateService:IndexCandidateBySubscriberAsync Done");
            return true;
       
            
        }

        /// <summary>
        /// For the given subscriber, update or add their profile to the Candidate azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        /// 


        public async Task<bool> IndexRemoveCandidateBySubscriberAsync(Guid subscriberGuid, bool nonBlocking = true)
        {
                       
             // fire off as background job 
             if (nonBlocking)
                 _hangfireService.Enqueue<ScheduledJobs>(j => j.CandidateIndexRemove(subscriberGuid));
             else
            {
                _logger.LogInformation($"CandidateService:IndexRemoveCandidateBySubscriberAsync Starting subscriber = {subscriberGuid}  nonBlocking = {nonBlocking}");

                // Get index info for subscriber
                v_CandidateAzureSearch candidateProfile = _db.CandidateAzureSearch
               .Where(p => p.SubscriberGuid == subscriberGuid)
               .FirstOrDefault();

                // make sure the user is found in the indexing view, this will not be the case if they are a hiring manager. 
                if (candidateProfile == null)
                {
                    _logger.LogInformation($"CandidateService:IndexRemoveCandidateBySubscriberAsync Unable to locate subscriber {subscriberGuid} in indexer view for deleteion, if they are not a hiring manager this will need to be investigated.");
                    return false;
                }
                await CandidateIndexRemoveAsync(candidateProfile);
            }
                
             _logger.LogInformation($"CandidateService:IndexRemoveCandidateBySubscriberAsync Done");

             return true;
                      
        }


        /// <summary>
        /// Indexes every candidate that has an index status of unindexed         
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IndexAllUnindexed(bool nonBlocking = true)
        {
            
            // Kick off job to index any unindexed candidate profiles 
            if ( nonBlocking)
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CandidateIndexUnindexedCandidates());
            else
            {
                // Get the indexer batch size   
                int CandidateIndexUnindexedProfilesBatchSize = int.Parse(_configuration["AzureSearch:CandidateIndexUnindexedProfilesBatchSize"]);
                _logger.LogInformation($"CandidateService.CandidateIndexUnindexedCandidates: Starting with a batch size of {CandidateIndexUnindexedProfilesBatchSize}");


                AzureIndexStatus noneStatus = _repositoryWrapper.AzureIndexStatusRepository.GetAll()
                    .Where(s => s.Name == Constants.AzureSearchIndexStatus.None)
                    .FirstOrDefault();

                if (noneStatus == null)
                {
                    _logger.LogError($"CandidateService.CandidateIndexUnindexedCandidates: Unable to locate candidate status for \"None\".");
                    return false;
                }

                List<v_CandidateAzureSearch> candidateProfiles = null;
                try
                {
                    candidateProfiles = _db.CandidateAzureSearch
                   .Where(p => p.AzureIndexStatusId == null || p.AzureIndexStatusId == noneStatus.AzureIndexStatusId)
                   .Take(CandidateIndexUnindexedProfilesBatchSize)
                  .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"CandidateService.CandidateIndexUnindexedCandidates: Error processing -> {ex.Message}");
                    return false;
                }


                _logger.LogInformation($"CandidateService.CandidateIndexUnindexedCandidates: Retreived {candidateProfiles.Count} Candidates for processing.");

                // short circuit if there is nothing to do
                if (candidateProfiles.Count == 0)
                    return true;

                // Call the candidateService to index the batch 
                await CandidateIndexBulk(candidateProfiles);

                // if the number of profiles retreived = the batch size, there may be more that needs to be indexed so
                // schedule this job to run again for another batch
                if (candidateProfiles.Count == CandidateIndexUnindexedProfilesBatchSize)
                {
                    _logger.LogInformation($"CandidateService.CandidateIndexUnindexedCandidates: Scheduling recursive call to index additional Candidates");
                    //Getrecurse delay variable 
                    int delay = int.Parse(_configuration["AzureSearch:IndexUnidexedCandidateRecurseDelayInMinutes"]);
                    _hangfireService.Schedule<ScheduledJobs>(j => j.CandidateIndexUnindexedCandidates(), TimeSpan.FromMinutes(delay));
                }

                _logger.LogInformation($"CandidateService.CandidateIndexUnindexedCandidates Done");

            }
            
        
            return true;
        }



        #endregion

        #region Work History

        public async Task<WorkHistoryListDto> GetCandidateWorkHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            try
            {
                var candidateWorkHistory = await _repositoryWrapper.SubscriberRepository.GetCandidateWorkHistory(subscriberGuid, limit, offset, sort, order);
                return _mapper.Map<WorkHistoryListDto>(candidateWorkHistory);
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred in CandidateService.GetCandidateWorkHistory: {e.Message}", e);
                throw new BadRequestException("An error occurred while processing the request.", e);
            }
        }

        public async Task UpdateCandidateWorkHistory(Guid subscriberGuid, WorkHistoryUpdateDto request)
        {
            try
            {
                await _repositoryWrapper.SubscriberRepository.UpdateCandidateWorkHistory(subscriberGuid, request);
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred in CandidateService.UpdateCandidateWorkHistory: {e.Message}", e);
                throw new BadRequestException("An error occurred while processing the request.", e);
            }
        }

        #endregion

        #region private helper functions 


 



        /// <summary>
        /// Index a batch of candidate profiles 
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>F
        private async Task<bool> CandidateIndexBulk(List<v_CandidateAzureSearch> profiles)
        {
            if (profiles == null || profiles.Count == 0)
            {
                _logger.LogInformation($"CandidateService:CandidateIndexBulk There are no profiles to index in the current batch, returing false");
                return false;
            }

            _logger.LogInformation($"CandidateService:CandidateIndexBulk Starting with a batch of  {profiles.Count} for indexing");
            List<CandidateSDOC> Docs = new List<CandidateSDOC>();
            foreach (v_CandidateAzureSearch p in profiles)
            {
                try
                {
                    CandidateSDOC indexDoc = await MapToCandidateSDOC(p);
                    Docs.Add(indexDoc);
                }
                catch (Exception ex)
                {
                    // mark subscriber record as bad index 
                    _subscriberService.UpdateCandidateIndexStatus(p.SubscriberGuid, $"Indexing Error in CandidateService.CandidateIndexBulk  Ex={ex.ToString()} ", Constants.AzureSearchIndexStatus.Error);
                }

            };

            // Don't queue the hangfire job if there is nothing to do
            if (Docs.Count == 0)
                return true;

            _logger.LogInformation($"CandidateService:CandidateIndexBulk Indexing Docs");
            AzureIndexResult info = await _azureSearchService.AddOrUpdateCandidateBulk(Docs);
            _logger.LogInformation($"CandidateService:CandidateIndexBulk Updateing Doc Statuses ");
            await UpdateAzureStatus(info, ResolveIndexStatusMessage(info.StatusMsg), info.StatusMsg);
            _logger.LogInformation($"CandidateService:CandidateIndexBulk Done");

            return true;
        }

        /// <summary>
        /// Index the specified document into azure search 
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        private async Task<bool> CandidateIndexAsync(Guid subscriberGuid)
        {

            try
            {
                v_CandidateAzureSearch candidateProfile = _db.CandidateAzureSearch
               .Where(p => p.SubscriberGuid == subscriberGuid)
               .FirstOrDefault();

                // make sure the user is found in the indexing view, this will not be the case if they are a hiring manager. 
                if (candidateProfile == null)
                {
                    _logger.LogInformation($"CandidateService:IndexCandidateBySubscriberAsync Unable to locate subscriber {subscriberGuid} in indexer view, if they are not a hiring manager this will need to be investigated.");
                    return false;
                }
                
                CandidateSDOC indexDoc = await MapToCandidateSDOC(candidateProfile);
     
                _logger.LogInformation($"CandidateService:CandidateIndexAsync Starting subscriber = {subscriberGuid}  ");
                AzureIndexResult info = await _azureSearchService.AddOrUpdateCandidate(indexDoc);
                // Update subscribers azure index status 
                await UpdateAzureStatus(info, Constants.AzureSearchIndexStatus.Indexed, info.StatusMsg);
                _logger.LogInformation($"CandidateService:CandidateIndexAsync Done");
                return true;
            }
            catch (Exception ex)
            {
                // mark subscriber record as bad index 
                _subscriberService.UpdateCandidateIndexStatus(subscriberGuid, $"Indexing Error in CandidateService.IndexCandidateBySubscriberAsync  Ex={ex.ToString()} ", Constants.AzureSearchIndexStatus.Error);
                return false;

            }

        }


        /// <summary>
        /// Remove the specified document from azure search 
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        private async Task<bool> CandidateIndexRemoveAsync(v_CandidateAzureSearch candidate)
        {
            try
            {
                _logger.LogInformation($"CandidateService:CandidateIndexRemoveAsync Starting subscriber = {candidate.SubscriberGuid}  ");

                CandidateSDOC indexDoc = await MapToCandidateSDOC(candidate);
                AzureIndexResult info = await _azureSearchService.DeleteCandidate(indexDoc);
                // Update subscribers azure index status 
                await UpdateAzureStatus(info, Constants.AzureSearchIndexStatus.Deleted, info.StatusMsg);

                _logger.LogInformation($"CandidateService:CandidateIndexRemoveAsync Done");
            }
            catch (Exception ex)
            {
                // mark subscriber record as bad index 
                _subscriberService.UpdateCandidateIndexStatus(candidate.SubscriberGuid, $"Indexing Deletion Error in CandidateService.IndexRemoveCandidateBySubscriberAsync  Ex={ex.ToString()} ", Constants.AzureSearchIndexStatus.Error);

                return false;
            }
            return true;
        }





        private static string ResolveIndexStatusMessage(string statusMsg)
        {
            if (string.IsNullOrEmpty(statusMsg)) { return Constants.AzureSearchIndexStatus.None; }
            if (statusMsg.StartsWith("Indexed On")) { return Constants.AzureSearchIndexStatus.Indexed; }
            if (statusMsg.StartsWith("Deleted On")) { return Constants.AzureSearchIndexStatus.Deleted; }
            if (statusMsg.StartsWith("StatusCode = ")) { return Constants.AzureSearchIndexStatus.Error; }
            if (statusMsg.Contains("error", StringComparison.CurrentCultureIgnoreCase)) { return Constants.AzureSearchIndexStatus.Error; }

            return Constants.AzureSearchIndexStatus.Pending;
        }


        private async Task<bool> UpdateAzureStatus(AzureIndexResult results, string statusName, string info)
        {
            // Call stored procedure 
            try
            {
                _repositoryWrapper.StoredProcedureRepository.UpdateCandidateAzureIndexStatuses(results?.DOCResults?.Value ?? new List<AzureIndexResultStatus>(), statusName, info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidateService:UpdateAzureStatus Error updating index statuses; message: {ex.Message}, stack trace: {ex.StackTrace}");
                throw;
            }
            return true;
        }

        // IMPORTANT!          
        // 1) Any colections of objects (e.g. Skills, Languages, etc.) must be hydrated with an empty list 
        private async Task<CandidateSDOC> MapToCandidateSDOC(v_CandidateAzureSearch candidate)
        {
            char recordDelim = Convert.ToChar(30);
            char fieldDelim = Convert.ToChar(29);
            try
            {
                CandidateSDOC indexDoc = _mapper.Map<CandidateSDOC>(candidate);
                // manually map the location.  todo find a way for automapper to do this 
                if (candidate.Location != null)
                {
                    Double lat = (double)candidate.Location.Lat;
                    Double lng = (double)candidate.Location.Long;
                    Position p = new Position(lat, lng);
                    indexDoc.Location = new Point(p);
                }
                // map skills to list 
                List<string> skillList = new List<string>();
                if (!string.IsNullOrEmpty(candidate.Skills))
                {
                    string[] skillArray = candidate.Skills.Split(recordDelim);

                    foreach (string skill in skillArray)
                        skillList.Add(skill);

                }
                indexDoc.Skills = skillList;

                // map  languages 
                indexDoc.Languages = new List<LanguageSDOC>();
                if (!string.IsNullOrEmpty(candidate.SubscriberLanguages))
                {
                    string[] languageArray = candidate.SubscriberLanguages.Split(recordDelim);
                    foreach (string languageInfo in languageArray)
                    {                       
                        try
                        {
                            string[] langInfo = languageInfo.Split(fieldDelim);
                            indexDoc.Languages.Add(new LanguageSDOC()
                            {             
                                Language = langInfo[0],
                                Proficiency = langInfo[1]
                            });
                        }
                        catch ( Exception ex )
                        {
                            LogMappingError(candidate, $"LanguageInfo = {languageInfo}", ex);
                        }                 
                    }
                }

                // map employment types to list 
                List<string> employmentTypes = new List<string>();
                if (!string.IsNullOrEmpty(candidate.EmploymentTypes))
                {
                    string[] info = candidate.EmploymentTypes.Split(recordDelim);

                    foreach (string employmentType in info)
                        employmentTypes.Add(employmentType);
                }
                indexDoc.EmploymentTypes = employmentTypes;

                // map trainings 
                indexDoc.Training = new List<TrainingSDOC>();
                if (!string.IsNullOrEmpty(candidate.SubscriberTraining))
                {
                    string[] info = candidate.SubscriberTraining.Split(recordDelim);
                    foreach (string data in info)
                    {                 
                        try
                        {
                            string[] trainingInfo = data.Split(fieldDelim);
                            indexDoc.Training.Add(new TrainingSDOC()
                            {
                                Type = trainingInfo[0],
                                Institution = trainingInfo[1],
                                Name = trainingInfo[2]
                            });
                        }
                        catch (Exception ex)
                        {
                            LogMappingError(candidate, $"TrainingInfo = {data}", ex);
                        }


                    }
                }

                // map Education 
                indexDoc.Education = new List<EducationSDOC>();
                if (!string.IsNullOrEmpty(candidate.SubscriberEducation))
                {
                    string[] info = candidate.SubscriberEducation.Split(recordDelim);
                    foreach (string data in info)
                    {
                        try
                        {
                            string[] educationInfo = data.Split(fieldDelim);
                            indexDoc.Education.Add(new EducationSDOC()
                            {
                                Institution = educationInfo[0],
                                DegreeType = educationInfo[1],
                                Degree = educationInfo[2]
                            });
                        }
                        catch (Exception ex)
                        {
                            LogMappingError(candidate, $"EducationInfo = {data}", ex);
                        }
                    }
                }

                // map job titles
                indexDoc.Titles = new List<string>();
                if (!string.IsNullOrEmpty(candidate.SubscriberTitles))
                {
                    string[] info = candidate.SubscriberTitles.Split(recordDelim);

                    foreach (string title in info)
                        indexDoc.Titles.Add(title);

                }

                // map work histories
                indexDoc.WorkHistories = new List<WorkHistorySDOC>();
                if (!string.IsNullOrEmpty(candidate.SubscriberWorkHistory))
                {
                    string[] info = candidate.SubscriberWorkHistory.Split(recordDelim);
                    foreach (string data in info)
                    {                       
                        try
                        {
                            string[] workInfo = data.Split(fieldDelim);
                            indexDoc.WorkHistories.Add(new WorkHistorySDOC()
                            {
                                CompanyName = workInfo[0],
                                Title = workInfo[1]
                            });
                        }
                        catch (Exception ex)
                        {
                            LogMappingError(candidate, $"WorkHistory = {data}", ex);
                        }
                    }
                }

                return indexDoc;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidateService:MapToCandidateSDOC Exception for subscriber {candidate.SubscriberGuid}; error: {ex.Message}, stack trace: {ex.StackTrace}");
                throw;
            }
        }



        private void LogMappingError(v_CandidateAzureSearch candidate, string info, Exception ex)
        {
            _logger.LogError($"CandidateService.LogMappingError: Mapping error for {candidate.SubscriberGuid} Info = {info} ", ex);
        }

        #endregion

    }
}
