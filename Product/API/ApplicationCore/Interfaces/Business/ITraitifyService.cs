using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITraitifyService
    {
        Task<TraitifyDto> GetByAssessmentId(string assessmentId);
        Task CreateNewAssessment(TraitifyDto dto);

        Task CompleteSignup(string assessmentId, int subscriberId);
        Task<TraitifyDto> StartNewAssesment(TraitifyDto dto);

        Task<TraitifyDto> GetAssessment(string assessmentId);

        Task<TraitifyDto> CompleteAssessment(string assessmentId);
    }
}
