using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICandidatesService
    {
        Task<EducationalDegreeTypeListDto> GetAllEducationalDegrees(int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending");
        Task<SubscriberEducationHistoryDto> GetCandidateEducationHistory(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending");
        Task<TrainingTypesDto> GetAllTrainingTypes(int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending");
        Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid);

        Task<CandidatePersonalInfoDto> GetCandidatePersonalInfo(Guid subscriberGuid);
        Task<SubscriberTrainingHistoryDto> GetCandidateTrainingHistory(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending");
        Task UpdateCandidateEducationAndTraining(Guid subscriberGuid, SubscriberEducationAssessmentsDto subscriberEducationAssessmentsDto);

        Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto);

        Task UpdateCandidatePersonalInfo(Guid subscriberGuid, CandidatePersonalInfoDto candidatePersonalInfoDto);

        Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid);

        Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference);

        Task<SkillListDto> GetSkills(Guid subscriberGuid, int limit, int offset, string sort, string order);

        Task UpdateSkills(Guid subscriberGuid, List<string> skillNames);

        Task<LanguageListDto> GetLanguageList();

        Task<ProficiencyLevelListDto> GetProficiencyLevelList();

        Task<LanguageProficiencyListDto> GetLanguageProficiencies(Guid subscriberGuid);

        Task<Guid> CreateLanguageProficiency(LanguageProficiencyDto languageProficiency, Guid subscriberGuid);

        Task UpdateLanguageProficiency(LanguageProficiencyDto languageProficiency, Guid languageProficiencyGuid, Guid subscriberGuid);

        Task DeleteLanguageProficiency(Guid languageProficiencyGuid, Guid subscriberGuid);

        Task<CompensationPreferencesDto> GetCompensationPreferences(Guid subscriberGuid);

        Task UpdateCompensationPreferences(CompensationPreferencesDto compensationPreferences, Guid subscriberGuid);

        Task<AssessmentsDto> GetAssessments(Guid subscriber);

        #region Candidate Indexing 

        Task<bool> CandidateIndexAsync(CandidateSDOC candidate);
        Task<bool> CandidateIndexBulkAsync(List<CandidateSDOC> candidateList);
        Task<bool> CandidateIndexRemoveAsync(CandidateSDOC candidate);

        Task<bool> IndexCandidateBySubscriberAsync(Guid subscriberGuid, bool nonBlocking = true);
        Task<bool> IndexRemoveCandidateBySubscriberAsync(Guid subscriberGuid, bool nonBlocking = true);        
        Task<bool> IndexAllUnindexed( bool nonBlocking = true);

        #endregion

        #region work history
        Task<WorkHistoryListDto> GetCandidateWorkHistory(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task UpdateCandidateWorkHistory(Guid subscriberGuid, WorkHistoryUpdateDto request);

        #endregion





    }
}
