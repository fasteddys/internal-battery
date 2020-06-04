using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.B2B;
using UpDiddyLib.Dto;

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
        [Route("crosschq/status")]
        public async Task<IActionResult> UpdateReferenceChkStatus([FromBody]CrosschqWebhookDto request)
        {
            await _crosschqService.UpdateReferenceChkStatus(request);
            return Ok();
        }

        #endregion
    }
}