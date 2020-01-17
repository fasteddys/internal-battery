using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.ActionFilter;
namespace UpDiddyApi.Controllers.V2
{
    [ServiceFilter(typeof(ActionFilter))]
    [Route("/V2/[controller]/")]
    [ApiController]
    public class TraitifyController : BaseApiController
    {
        private readonly ITraitifyServiceV2 _traitifyService;
        public TraitifyController(ITraitifyServiceV2 traitifyService)
        {
            _traitifyService = traitifyService;
        }

        [HttpPost]
        public async Task<IActionResult> StartNewAssesment(TraitifyRequestDto dto)
        {
            var responseDto = await _traitifyService.StartNewAssesment(dto, GetSubscriberGuid());
            return StatusCode(201, responseDto);
        }

        [HttpPut]
        [Route("{assessmentId:length(36)}/complete")]
        public async Task<IActionResult> CompleteAssessment(string assessmentId)
        {
            await _traitifyService.CompleteAssessment(assessmentId);
            return StatusCode(200);
        }
    }
}

