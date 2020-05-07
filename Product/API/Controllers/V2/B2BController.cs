using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class B2BController : BaseApiController
    {

        private readonly IHiringManagerService _hiringManagerService;

        public B2BController(IServiceProvider services)
        {
            _hiringManagerService = services.GetService<IHiringManagerService>();
        }

        [HttpGet]
        [Authorize]
        //[Authorize(Policy = "IsHiringManager")] ???
        [Route("hiring-managers")]
        public async Task<IActionResult> GetHiringManager()
        {

            var rval = await _hiringManagerService.GetHiringManagerBySubscriberGuid(GetSubscriberGuid());
            return Ok(rval);
        }

        [HttpPut]
        [Authorize]
        //[Authorize(Policy = "IsHiringManager")] ???
        [Route("hiring-managers")]
        public async Task<IActionResult> UpdateHiringManager([FromBody] HiringManagerDto request)
        {
            await _hiringManagerService.UpdateHiringManager(GetSubscriberGuid(), request);
            return Ok();
        }

    }
}