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

        //Can we implement all service calls talk through HiringManagerService???
        private readonly IHiringManagerService _hiringManagerService;
        private readonly IG2Service _g2Service;

        public B2BController(IServiceProvider services)
        {
            _hiringManagerService = services.GetService<IHiringManagerService>();
            _g2Service = services.GetService<IG2Service>();
        }



        [HttpPut]
        [Authorize]
        //[Authorize(Policy = "IsHiringManager")]
        [Route("hiring-manager")]
        public async Task<IActionResult> UpdateHiringManager([FromBody] HiringManagerDto request)
        {
            //HM update
            var rVal = await _hiringManagerService.AddHiringManager(GetSubscriberGuid(), true);
            return Ok(rVal);
        }

        #region Hiring  Query Functions 
        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("profiles/query")]
        public async Task<IActionResult> SearchG2(Guid cityGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0, bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null)
        {
            var rVal = await _g2Service.B2BSearchAsync(GetSubscriberGuid(), cityGuid, limit, offset, sort, order, keyword, sourcePartnerGuid, radius, isWillingToRelocate, isWillingToTravel, isActiveJobSeeker, isCurrentlyEmployed, isWillingToWorkProBono);
            return Ok(rVal);
        }

        #endregion



    }
}