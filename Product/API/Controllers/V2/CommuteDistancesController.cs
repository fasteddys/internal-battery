using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers.V2
{
    [Route("v2/")]
    public class CommuteDistancesController : BaseApiController
    {
        private readonly ICommuteDistancesService _commuteDistancesService;

        public CommuteDistancesController(IServiceProvider services)
        {
            _commuteDistancesService = services.GetService<ICommuteDistancesService>();
        }

        [HttpGet]
        [Route("commute-distances")]
        public async Task<IActionResult> GetCommuteDistances(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var response  = await _commuteDistancesService.GetCommuteDistances(limit, offset, sort, order);
            return Ok(response);
        }

        [HttpGet]
        [Route("commute-distances/{commuteDistanceGuid}")]
        public async Task<IActionResult> GetCommuteDistance(Guid commuteDistanceGuid)
        {
            var response = await _commuteDistancesService.GetCommuteDistance(commuteDistanceGuid);
            return Ok(response);
        }
    }
}
