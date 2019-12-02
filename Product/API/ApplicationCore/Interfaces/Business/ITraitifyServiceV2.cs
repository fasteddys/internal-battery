using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITraitifyServiceV2
    {
        Task CompleteAssessment(string assessmentId);
        Task<TraitifyResponseDto> StartNewAssesment(TraitifyRequestDto dto, Guid subscriberGuid);
        Task CompleteSignup(string assessmentId, Subscriber subscriber);
    }
}
