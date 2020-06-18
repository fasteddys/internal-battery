using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICandidatesService
    {
        // This empty service interface will be used for stories:
        //   #2480 - Candidate 360: Personal Info
        //   #2481 - Candidate 360: Employment Preferences
        //   #2482 - Candidate 360: Role Preferences

        Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid);
        Task<CandidatePersonalInfoDto> GetCandidatePersonalInfo(Guid subscriberGuid);
        Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto);
        Task UpdateCandidatePersonalInfo(Guid subscriberGuid, CandidatePersonalInfoDto candidatePersonalInfoDto);
    }
}
