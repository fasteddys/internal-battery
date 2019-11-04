using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Threading.Tasks;
using System;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TraitifyController : ControllerBase
    {
        private readonly ITraitifyService _traitifyService;
        public TraitifyController(ITraitifyService traitifyService)
        {
            _traitifyService = traitifyService;
        }

        [HttpPost]
        [Route("api/[controller]/new")]
        public async Task<TraitifyDto> StartNewAssesment(TraitifyDto dto)
        {
            return await _traitifyService.StartNewAssesment(dto);
        }

        [HttpGet]
        [Route("api/[controller]/{assessmentId:length(36)}")]
        public async Task<TraitifyDto> GetAssessment(string assessmentId)
        {
            return await _traitifyService.GetAssessment(assessmentId);
        }

        [HttpGet]
        [Route("api/[controller]/complete/{assessmentId:length(36)}")]
        public async Task<TraitifyDto> CompleteAssessment(string assessmentId)
        {
            return await _traitifyService.CompleteAssessment(assessmentId);
        }

        [HttpPut]
        [Route("api/[controller]/{assessmentId}/subscriber/{subscriberGuid}")]
        public async Task<BasicResponseDto> AssociateSubscriberWithAssessmentAsync(string assessmentId, Guid subscriberGuid)
        {
            try
            {
                await _traitifyService.CompleteSignup(assessmentId, subscriberGuid);
            }
            catch (Exception e)
            {
                return new BasicResponseDto() { StatusCode = 500, Description = e.Message };
            }

            return new BasicResponseDto() { StatusCode = 200, Description = "The assessment was associated with the subscriber successfully." };
        }
    }
}

