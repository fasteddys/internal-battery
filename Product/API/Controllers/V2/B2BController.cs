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

        public B2BController(IHiringManagerService hiringManagerService, IInterviewRequestService interviewRequestService, IProfileService profileService)
        {
            _hiringManagerService = hiringManagerService;
            _interviewRequestService = interviewRequestService;
            _profileService = profileService;
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

        [HttpPost("request-interview/{profileGuid}")]
        [Authorize(/*Policy = "IsHiringManager"*/)]
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