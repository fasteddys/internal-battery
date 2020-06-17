using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICandidatesService
    {

        Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid);

        Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto);

        Task<Candidate360RoleDto> GetCandidate360Role(Guid subscriberGuid);

        Task UpdateCandidate360Role(Guid subscriberGuid, Candidate360RoleDto candidate360Role);
    }
}
