using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Threading.Tasks;

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
    }
}

