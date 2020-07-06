using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICandidatesService
    {

        Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid);

        Task<CandidatePersonalInfoDto> GetCandidatePersonalInfo(Guid subscriberGuid);

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
    }
}
