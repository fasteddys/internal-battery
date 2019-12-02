using System;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITraitifyServiceV2
    {
        Task CompleteAssessment(string assessmentId);
        Task<TraitifyResponseDto> StartNewAssesment(TraitifyRequestDto dto, Guid subscriberGuid);
    }
}
