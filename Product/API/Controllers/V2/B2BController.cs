using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class B2BController : BaseApiController
    {
        private readonly IHiringManagerService _hiringManagerService;
        private readonly IInterviewRequestService _interviewRequestService;
        private readonly IProfileService _profileService;
        private readonly IG2Service _g2Service;

        public B2BController(IHiringManagerService hiringManagerService, IInterviewRequestService interviewRequestService, IProfileService profileService, IG2Service g2Service)
        {
            _hiringManagerService = hiringManagerService;
            _interviewRequestService = interviewRequestService;
            _profileService = profileService;
            _g2Service = g2Service;
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("hiring-managers")]
        public async Task<IActionResult> GetHiringManager()
        {

            var rval = await _hiringManagerService.GetHiringManagerBySubscriberGuid(GetSubscriberGuid());
            return Ok(rval);
        }

        [HttpPut]
        [Authorize(Policy = "IsHiringManager")]
        [Route("hiring-managers")]
        public async Task<IActionResult> UpdateHiringManager([FromBody] HiringManagerDto request)
        {
            await _hiringManagerService.UpdateHiringManager(GetSubscriberGuid(), request);
            return Ok();
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

        #endregion Hiring Query Functions 

        [HttpPost("hiring-managers/request-interview/{profileGuid}")]
        [Authorize(Policy = "IsHiringManager")]
        public async Task<IActionResult> SubmitInterviewRequest(Guid profileGuid)
        {
            var hiringManager = await _hiringManagerService
                .GetHiringManagerBySubscriberGuid(GetSubscriberGuid());

            var interviewRequestId = await _interviewRequestService
                .SubmitInterviewRequest(hiringManager, profileGuid);

            return Ok(interviewRequestId);
        }
    }
}