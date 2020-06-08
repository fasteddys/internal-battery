using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class CrosschqController : BaseApiController
    {
        private readonly ICrosschqService _crosschqService;

        public CrosschqController(IServiceProvider services)
        {
            _crosschqService = services.GetService<ICrosschqService>();
        }

        #region webhook

        [HttpPost]
        [Route("status")]
        public async Task<IActionResult> UpdateReferenceChkStatus([FromBody]CrosschqWebhookDto request)
        {
            await _crosschqService.UpdateReferenceChkStatus(request);
            return StatusCode(202);
        }

        #endregion
    }
}