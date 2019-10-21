using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITraitifyService
    {
        Task<TraitifyDto> GetByAssessmentId(string assessmentId);
        Task CreateNewAssessment(TraitifyDto dto);
        Task<TraitifyDto> CompleteAssessment(TraitifyDto dto);

        Task CompleteSignup(string assessmentId, int subscriberId);
        Task<TraitifyDto> StartNewAssesment(TraitifyDto dto);

        TraitifyDto GetAssessment(string assessmentId);

        Task<bool> CompleteAssessment(string assessmentId);
    }
}
